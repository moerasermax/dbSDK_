using MongoDB.Bson;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoMap
    {
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
