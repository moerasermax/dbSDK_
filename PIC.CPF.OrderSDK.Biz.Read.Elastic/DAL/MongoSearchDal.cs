using MongoDB.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    // 對齊客戶原 SDK 的 _searchDal.SearchByDDB / GetUserByCuamCidFromDDB (客戶端 DDB=DynamoDB,dbSDK 用 MongoDB)
    // 兩條路徑:
    //   Search 2/3 Dual Engine: ES 取 CoomNo[] → SearchByMongoAsync 用 coom_no $in KeyList 從 Mongo 抓完整 MongoOrder
    //   Search 7 單筆 LoadAsync: GetUserByCuamCidFromMongoAsync 從 Users collection 用 cuam_cid 抓 MongoUser
    // 依 DBSDK Part I §A: MongoMap 注入觸發兩階段靜態初始化 (DateTime serializer + ClassMap)
    public class MongoSearchDal
    {
        private readonly IMongoCollection<MongoOrder> _collection;
        private readonly IMongoCollection<MongoUser> _userCollection;

        public MongoSearchDal(
            IMongoCollection<MongoOrder> collection,
            IMongoCollection<MongoUser> userCollection,
            MongoMap mongoMap)
        {
            // mongoMap 注入即觸發 EnsureDateTimeSerializersRegistered + EnsureClassMapsRegistered (DBSDK §A)
            _ = mongoMap;
            _collection = collection;
            _userCollection = userCollection;
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

        // mirror 客戶原 SDK pattern: UserModel UserResult = await _searchDal.GetUserByCuamCidFromDDB(cuamCid)
        // 客戶 DDB LoadAsync 單筆讀 — 對應到 Mongo Find 單筆
        // 測資 cuam_cid 為 string、SDK 介面 int → ToString() 轉接
        public async Task<MongoUser?> GetUserByCuamCidFromMongoAsync(int cuamCid)
        {
            var filter = Builders<MongoUser>.Filter.Eq(u => u.CuamCid, cuamCid.ToString());
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
