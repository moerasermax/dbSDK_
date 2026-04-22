using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Globalization;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serializers
{
    /// <summary>
    /// 支援多語系日期格式的 DateTime Serializer
    /// 解決「下午」等非標準格式導致的反序列化失敗問題
    /// </summary>
    public class MultiCultureDateTimeSerializer : SerializerBase<DateTime>
    {
        /// <summary>
        /// 支援的日期格式清單
        /// </summary>
        private static readonly string[] SupportedFormats = new[]
        {
            // ISO 8601 標準格式
            "yyyy-MM-ddTHH:mm:ss.fffZ",
            "yyyy-MM-ddTHH:mm:ssZ",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd",
            
            // 台灣常見格式
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd",
            
            // 中文上午/下午格式 (台灣)
            "yyyy年MM月dd日 tt hh:mm:ss",
            "yyyy年MM月dd日",
            "yyyy/MM/dd tt hh:mm:ss",
            "yyyy/MM/dd 下午 hh:mm:ss",
            "yyyy/MM/dd 上午 hh:mm:ss",
            
            // 其他常見格式
            "MM/dd/yyyy HH:mm:ss",
            "MM/dd/yyyy",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy"
        };

        /// <summary>
        /// 支援的文化特性清單
        /// </summary>
        private static readonly CultureInfo[] SupportedCultures = new[]
        {
            CultureInfo.InvariantCulture,
            new CultureInfo("zh-TW"),
            new CultureInfo("zh-CN"),
            new CultureInfo("en-US"),
            new CultureInfo("en-GB")
        };

        public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();

            switch (bsonType)
            {
                case BsonType.DateTime:
                    // 標準 BSON DateTime
                    return new DateTime(context.Reader.ReadDateTime());

                case BsonType.String:
                    // 字串格式日期 - 嘗試多種格式解析
                    var dateString = context.Reader.ReadString();
                    return ParseMultiCultureDateTime(dateString);

                case BsonType.Null:
                    context.Reader.ReadNull();
                    return DateTime.MinValue;

                default:
                    throw new FormatException($"無法將 BsonType {bsonType} 轉換為 DateTime");
            }
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
        {
            // 統一序列化為 BSON DateTime (UTC)
            if (value == DateTime.MinValue)
            {
                context.Writer.WriteNull();
            }
            else
            {
                // 轉換為 UTC 並寫入
                var utcValue = value.Kind == DateTimeKind.Local 
                    ? value.ToUniversalTime() 
                    : DateTime.SpecifyKind(value, DateTimeKind.Utc);
                context.Writer.WriteDateTime(utcValue.Ticks / 10000 - 62135596800000);
            }
        }

        /// <summary>
        /// 嘗試使用多種文化特性解析日期字串
        /// </summary>
        private static DateTime ParseMultiCultureDateTime(string dateString)
        {
            // 先嘗試標準 ISO 8601 格式
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
            {
                return result;
            }

            // 嘗試各種文化特性
            foreach (var culture in SupportedCultures)
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            // 嘗試精確格式匹配
            foreach (var format in SupportedFormats)
            {
                foreach (var culture in SupportedCultures)
                {
                    if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out result))
                    {
                        return result;
                    }
                }
            }

            // 特殊處理：替換中文上午/下午為 AM/PM
            var normalizedString = dateString
                .Replace("上午", "AM")
                .Replace("下午", "PM")
                .Replace("上午", "AM")
                .Replace("下午", "PM");

            if (DateTime.TryParse(normalizedString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            // 最後嘗試：使用各種格式解析正規化後的字串
            foreach (var format in SupportedFormats)
            {
                if (DateTime.TryParseExact(normalizedString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            throw new FormatException($"無法解析日期字串: '{dateString}'");
        }
    }

    /// <summary>
    /// 可為 Null 的 DateTime Serializer
    /// </summary>
    public class NullableMultiCultureDateTimeSerializer : SerializerBase<DateTime?>
    {
        private static readonly MultiCultureDateTimeSerializer _innerSerializer = new();

        public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.GetCurrentBsonType();

            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }

            return _innerSerializer.Deserialize(context, args);
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
        {
            if (value.HasValue)
            {
                _innerSerializer.Serialize(context, args, value.Value);
            }
            else
            {
                context.Writer.WriteNull();
            }
        }
    }
}
