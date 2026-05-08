using MongoDB.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    // 對齊客戶原 SDK 的 _searchDal.SearchByDDB(KeyList) 端點
    // Dual Engine 流程: ES 取 CoomNo[] → 此 DAL 用 coom_no $in KeyList 從 Mongo 抓完整 MongoOrder
    // 依 DBSDK Part I §A: MongoMap 注入觸發兩階段靜態初始化 (DateTime serializer + ClassMap)
    // ctor 接 IMongoCollection<MongoOrder> 不接 MongoDBDriver — 避免 srv schema 寫死、支援 docker 本地連線
    public class MongoSearchDal
    {
        private readonly IMongoCollection<MongoOrder> _collection;

        public MongoSearchDal(IMongoCollection<MongoOrder> collection, MongoMap mongoMap)
        {
            // mongoMap 注入即觸發 EnsureDateTimeSerializersRegistered + EnsureClassMapsRegistered (DBSDK §A)
            _ = mongoMap;
            _collection = collection;
        }

        // mirror 客戶原 SDK: var DDBData = _searchDal.SearchByDDB(KeyList);
        public async Task<IEnumerable<MongoOrder>> SearchByDDBAsync(List<string?> keyList)
        {
            if (keyList is null || keyList.Count == 0)
                return Array.Empty<MongoOrder>();

            var validKeys = keyList.Where(k => !string.IsNullOrEmpty(k)).Cast<string>().ToList();
            if (validKeys.Count == 0)
                return Array.Empty<MongoOrder>();

            var filter = Builders<MongoOrder>.Filter.In(x => x.CoomNo, validKeys);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}
