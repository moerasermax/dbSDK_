using MongoDB.Bson;
using System.Globalization;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Utils
{
    /// <summary>
    /// MongoDB 指令建構工具
    /// 將 MongoRepository 的核心扁平化邏輯抽離為公開靜態方法，
    /// 供 SDK 內部、Mock 倉儲與沙盒場景統一呼叫，確保邏輯一致性。
    /// </summary>
    public static class MongoCommandBuilder
    {
        /// <summary>
        /// 將 BsonDocument 扁平化為點符號路徑格式，並套用以下規則：
        ///   1. 忽略最外層的 _id（防止 Duplicate Key）
        ///   2. 忽略 null 值（不覆蓋資料庫舊資料）
        ///   3. 日期字串自動轉換為 BsonDateTime
        ///   4. 陣列欄位保留為整體（不展開，支援 $set 整個陣列）
        /// </summary>
        public static BsonDocument Flatten(BsonDocument doc, string prefix = "")
        {
            var flatDoc = new BsonDocument();

            foreach (var element in doc.Elements)
            {
                // 1. 忽略最外層 _id
                if (prefix == "" && element.Name == "_id")
                    continue;

                // 2. 忽略 null 值
                if (element.Value.IsBsonNull)
                    continue;

                if (element.Value.IsBsonDocument)
                {
                    var nestedDoc = Flatten(
                        element.Value.AsBsonDocument,
                        prefix + element.Name + ".");
                    flatDoc.Merge(nestedDoc);
                }
                else
                {
                    // 3. 日期字串轉換 + 4. 陣列/純量直接加入
                    var processedValue = TryConvertToBsonDateTime(element.Value);
                    flatDoc.Add(prefix + element.Name, processedValue);
                }
            }

            return flatDoc;
        }

        /// <summary>
        /// 嘗試將 BsonValue 中的日期字串轉換為 BsonDateTime。
        /// 支援多語系格式（含「下午」等中文格式）。
        /// 非字串或無法解析的值原樣回傳。
        /// </summary>
        public static BsonValue TryConvertToBsonDateTime(BsonValue value)
        {
            if (value.BsonType == BsonType.DateTime)
                return value;

            if (value.BsonType != BsonType.String)
                return value;

            var stringValue = value.AsString;
            if (string.IsNullOrWhiteSpace(stringValue))
                return value;

            if (TryParseMultiCultureDateTime(stringValue, out var dateTime))
            {
                var utcTicks     = dateTime.Kind == DateTimeKind.Local
                    ? dateTime.ToUniversalTime().Ticks
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).Ticks;
                var milliseconds = utcTicks / 10000 - 62135596800000;
                return new BsonDateTime(milliseconds);
            }

            return value;
        }

        private static bool TryParseMultiCultureDateTime(string dateString, out DateTime result)
        {
            result = default;

            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.fffZ", "yyyy-MM-ddTHH:mm:ssZ", "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "yyyy/MM/dd HH:mm:ss", "yyyy/MM/dd",
                "yyyy年MM月dd日 tt hh:mm:ss", "yyyy年MM月dd日", "yyyy/MM/dd tt hh:mm:ss",
                "yyyy/MM/dd 下午 hh:mm:ss", "yyyy/MM/dd 上午 hh:mm:ss"
            };

            var cultures = new[]
            {
                CultureInfo.InvariantCulture,
                new CultureInfo("zh-TW"),
                new CultureInfo("zh-CN"),
                new CultureInfo("en-US")
            };

            foreach (var culture in cultures)
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out result))
                    return true;
            }

            foreach (var format in formats)
            {
                foreach (var culture in cultures)
                {
                    if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out result))
                        return true;
                }
            }

            var normalized = dateString.Replace("上午", "AM").Replace("下午", "PM");
            if (DateTime.TryParse(normalized, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(normalized, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;
            }

            return false;
        }
    }
}
