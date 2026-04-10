using MongoDB.Bson;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using System.Reflection;


namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoRepository<T> : IRepository<T>
    {

        IMongoCollection<T> _Collection { get; set; }
        private readonly MongoMap _map;
        private readonly IDTO _dto;

        public MongoRepository(MongoDBDriver driver, MongoMap mongoMap, IDTO dto , string collectionName) 
        {
            
            var db = driver.GetDatabase(collectionName);
            _Collection = db.GetCollection<T>(collectionName);
            _map = mongoMap;
            _dto = dto;
        }

        async Task<IResult> IRepository<T>.GetData(string ConditionData_Json)
        {
           
            BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

            try
            {
                var cursor = await _Collection.FindAsync(filter);

                var data = await cursor.ToListAsync();

                if (data.Count != 0)
                {
                    return Result.SetResult("[MongoDBSDK]查詢資料成功。", data.ToJson());
                }
                else
                {
                    return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, "[MongoDBSDK]查無資料。");
                }
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name,  ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.InsertData(T Data)
        {
            try
            {
                await _Collection.InsertOneAsync(Data);
                return Result.SetResult("[MongoDBSDK]資料新增成功。");
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.RemoveData(string ConditionData_Json)
        {
            try
            {

                BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

                 await _Collection.DeleteOneAsync(filter);
                return Result.SetResult("[MongoDBSDK]刪除資料成功。");
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.UpdateData(string ConditionData_Json, T UpdateData)
        {
            try
            {
                BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

                BsonDocument _updateData = _map.ToBsonDocument(UpdateData);

                var options = new FindOneAndUpdateOptions<T> // 如果沒強型別可以把 <YourModelClass> 拿掉
                {
                    IsUpsert = true,

                    // 強烈建議加上這行：
                    // 預設情況下會回傳「更新前」的舊資料。如果是新插入(Upsert)，舊資料會是 null。
                    // 改成 ReturnDocument.After 可以確保你拿到的是「最新更新/插入後」的完整資料。
                    ReturnDocument = ReturnDocument.After
                };

                var queryResult = await _Collection.FindOneAndUpdateAsync(filter, _updateData, options);
                
                if (queryResult != null)
                {
                    return Result.SetResult("[MongoDBSDK]資料更新成功。", queryResult.ToJson());
                }
                else
                {
                    return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name,"[MongoDBSDK]資料更新失敗。");
                }
                
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
    }
}
