using MongoDB.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    // 對齊客戶原 SDK 的 _searchDal.SearchByDDB(KeyList) 端點 (客戶端 DDB=DynamoDB,dbSDK 用 MongoDB → method 命名 SearchByMongoAsync)
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

        // mirror 客戶原 SDK pattern: var DDBData = _searchDal.SearchByDDB(KeyList) — 客戶 DDB=DynamoDB,我們對應到 MongoDB
        public async Task<IEnumerable<MongoOrder>> SearchByMongoAsync(List<string?> keyList)
        {
            if (keyList is null || keyList.Count == 0)
                return Array.Empty<MongoOrder>();

            var validKeys = keyList.Where(k => !string.IsNullOrEmpty(k)).Cast<string>().ToList();
            if (validKeys.Count == 0)
                return Array.Empty<MongoOrder>();

            var filter = Builders<MongoOrder>.Filter.In(x => x.CoomNo, validKeys);
            var docs = await _collection.Find(filter).ToListAsync();

            // Mongo Find($in) 不保證 input keyList 順序;依 ES 給的 KeyList 重排對齊客戶排序合約
            var lookup = docs
                .Where(d => !string.IsNullOrEmpty(d.CoomNo))
                .ToDictionary(d => d.CoomNo!, d => d);
            return validKeys
                .Where(k => lookup.ContainsKey(k))
                .Select(k => lookup[k]);
        }
    }
}
