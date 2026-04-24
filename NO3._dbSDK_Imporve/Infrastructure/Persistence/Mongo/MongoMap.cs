using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serializers;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoMap
    {
        // 使用靜態欄位追蹤是否已註冊
        private static bool _serializersRegistered = false;
        private static bool _classMapsRegistered = false;
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

        /// <summary>
        /// 註冊實體類別的 BsonClassMap，確保業務主鍵欄位以 String 格式儲存
        /// 注意：不使用 MapIdMember，讓 MongoDB 原生產生 ObjectId 作為 _id
        /// </summary>
        public static void EnsureClassMapsRegistered()
        {
            if (_classMapsRegistered) return;

            lock (_lock)
            {
                if (_classMapsRegistered) return;

                // 註冊 Orders 基礎類別
                if (!BsonClassMap.IsClassMapRegistered(typeof(Orders)))
                {
                    BsonClassMap.RegisterClassMap<Orders>(cm =>
                    {
                        cm.AutoMap();
                        // PK 欄位作為業務主鍵，明確設定為 String 序列化
                        cm.MapMember(c => c.PK)
                          .SetSerializer(new StringSerializer(BsonType.String));
                    });
                }

                // 註冊 OrderSummary 基礎類別
                if (!BsonClassMap.IsClassMapRegistered(typeof(OrderSummary)))
                {
                    BsonClassMap.RegisterClassMap<OrderSummary>(cm =>
                    {
                        cm.AutoMap();
                        // PK 欄位作為業務主鍵，明確設定為 String 序列化
                        cm.MapMember(c => c.PK)
                          .SetSerializer(new StringSerializer(BsonType.String));
                    });
                }

                // 註冊 EventGiftModel 衍生類別
                if (!BsonClassMap.IsClassMapRegistered(typeof(EventGiftModel)))
                {
                    BsonClassMap.RegisterClassMap<EventGiftModel>(cm =>
                    {
                        cm.AutoMap();
                        // S11 修復：AutoMap() 會將名為 "id" 的屬性自動映射為 _id (ObjectId)
                        // 使用 SetIdMember(null) 取消此慣例，讓 MongoDB 原生產生 ObjectId 作為 _id
                        cm.SetIdMember(null);
                        // 重新將 id 作為普通字串欄位映射
                        cm.MapMember(c => c.id)
                          .SetSerializer(new StringSerializer(BsonType.String));
                        // S11 補丁：讀取時資料庫文件含有原生 _id (ObjectId)，
                        // 但 ClassMap 已無對應成員，加入此設定避免 "Element '_id' does not match any field" 錯誤
                        cm.SetIgnoreExtraElements(true);
                    });
                }

                // 註冊 EventGiftSummaryModel 衍生類別
                if (!BsonClassMap.IsClassMapRegistered(typeof(EventGiftSummaryModel)))
                {
                    BsonClassMap.RegisterClassMap<EventGiftSummaryModel>(cm =>
                    {
                        cm.AutoMap();
                        // 同 EventGiftModel：取消 Id 的 _id 自動映射，改為普通字串欄位
                        cm.SetIdMember(null);
                        cm.MapMember(c => c.Id)
                          .SetSerializer(new StringSerializer(BsonType.String));
                        cm.SetIgnoreExtraElements(true);
                    });
                }

                _classMapsRegistered = true;
            }
        }

        public MongoMap()
        {
            // 在實例化時確保 Serializer 與 ClassMap 已註冊
            EnsureDateTimeSerializersRegistered();
            EnsureClassMapsRegistered();
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
