using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoMap
    {
        /// <summary>
        /// 將任何物件轉換為 BsonDocument
        /// </summary>
        /// <typeparam name="T">物件的類型</typeparam>
        /// <param name="obj">要轉換的物件實例</param>
        /// <returns>轉換後的 BsonDocument</returns>
        public BsonDocument ToBsonDocument<T>(T obj)
        {
            if (obj == null) return new BsonDocument();

            // 直接利用官方的序列化器，這會尊重類別中的所有 Bson 特性（如 [BsonIgnore]）
            return obj.ToBsonDocument();
        }

        /// <summary>
        /// 將物件清單轉換為 BsonDocument 清單
        /// </summary>
        public IEnumerable<BsonDocument> ToBsonDocumentList<T>(IEnumerable<T> list)
        {
            return list.Select(item => ToBsonDocument(item));
        }



        public static MongoMap getInstance() { return _Instance; }
        private static MongoMap _Instance = new MongoMap();
        private MongoMap() { }

    }
}
