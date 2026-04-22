using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serializers;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoMap
    {
        // 使用靜態欄位追蹤是否已註冊
        private static bool _serializersRegistered = false;
        private static readonly object _lock = new();

        /// <summary>
        /// 註冊支援多語系日期格式的 DateTime Serializer
        /// </summary>
        public static void EnsureDateTimeSerializersRegistered()
        {
            if (_serializersRegistered) return;

            lock (_lock)
            {
                if (_serializersRegistered) return;

                try
                {
                    // 直接註冊，如果已註冊會拋出例外，我們忽略它
                    BsonSerializer.RegisterSerializer(typeof(DateTime), new MultiCultureDateTimeSerializer());
                }
                catch (BsonSerializationException)
                {
                    // 已註冊，忽略
                }

                try
                {
                    BsonSerializer.RegisterSerializer(typeof(DateTime?), new NullableMultiCultureDateTimeSerializer());
                }
                catch (BsonSerializationException)
                {
                    // 已註冊，忽略
                }

                _serializersRegistered = true;
            }
        }

        public MongoMap()
        {
            // 在實例化時確保 Serializer 已註冊
            EnsureDateTimeSerializersRegistered();
        }

        public BsonDocument ToBsonDocument<T>(T obj)
        {
            if (obj == null) return new BsonDocument();
            return obj.ToBsonDocument();
        }

        /// <summary>
        /// 專門為了 Upsert 設計：轉換為扁平化的 $set 字典 (點符號)
        /// </summary>
        public BsonDocument ToPatchDocument<T>(T obj)
        {
            var rootDoc = ToBsonDocument(obj);
            rootDoc.Remove("_id"); // Upsert 不更新 ID

            var patchDoc = new BsonDocument();
            FlattenAndFill(patchDoc, "", rootDoc);
            return patchDoc;
        }

        private void FlattenAndFill(BsonDocument result, string prefix, BsonDocument current)
        {
            foreach (var element in current)
            {
                var path = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";

                // 如果是子物件，繼續遞迴扁平化
                if (element.Value.IsBsonDocument)
                {
                    FlattenAndFill(result, path, element.Value.AsBsonDocument);
                }
                else
                {
                    // 這是最終要 $set 的點符號路徑
                    result.Add(path, element.Value);
                }
            }
        }
    }
}
