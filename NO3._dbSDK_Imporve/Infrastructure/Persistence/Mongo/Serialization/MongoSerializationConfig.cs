using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization
{
    /// <summary>
    /// MongoDB 序列化器集中註冊點
    /// 在 DI 啟動時呼叫一次，避免 Program.cs 雜亂
    /// </summary>
    public static class MongoSerializationConfig
    {
        private static bool _registered = false;
        private static readonly object _lock = new();

        public static void Register()
        {
            if (_registered) return;

            lock (_lock)
            {
                if (_registered) return;

                try { BsonSerializer.RegisterSerializer(new MongoDateTimeSerializer()); }
                catch (BsonSerializationException) { /* 已註冊，忽略 */ }

                try { BsonSerializer.RegisterSerializer(new MongoNullableDateTimeSerializer()); }
                catch (BsonSerializationException) { /* 已註冊，忽略 */ }

                _registered = true;
            }
        }
    }
}
