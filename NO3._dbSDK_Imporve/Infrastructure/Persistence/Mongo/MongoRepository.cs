using MongoDB.Bson;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using System.Reflection;


namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoRepository<T> : IRepository<T>
    {

        IMongoCollection<T> _Collection { get; set; }
        private readonly MongoMap _map;

        public MongoRepository(MongoDBDriver driver, MongoMap mongoMap, string collectionName) 
        {
            
            var db = driver.GetDatabase(collectionName);
            _Collection = db.GetCollection<T>(collectionName);
            _map = mongoMap;
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

                var queryResult = await _Collection.FindOneAndUpdateAsync(filter, _updateData);

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
