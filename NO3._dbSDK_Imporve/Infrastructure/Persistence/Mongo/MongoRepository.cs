using MongoDB.Bson;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoRepository<T> : IRepository<T>
    {

        IMongoCollection<T> _Collection { get; set; }

        public MongoRepository(MongoDBDriver driver, string collectionName) 
        {
            
            var db = driver.GetDatabase(collectionName);
            _Collection = db.GetCollection<T>(collectionName);
        }

        async Task<IResult> IRepository<T>.getData(string ConditionData)
        {
           
            BsonDocument filter = BsonDocument.Parse(ConditionData);

            try
            {
                using (var cursor = await _Collection.FindAsync(filter))
                {
                    // 2. 從游標中非同步轉換為 List
                    var data = await cursor.ToListAsync();
                    return Result.setResult("[MongoDBSDK]查詢資料成功。", data.ToJson());
                }
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.insertData(T Data)
        {
            try
            {
                await _Collection.InsertOneAsync(Data);
                return Result.setResult("[MongoDBSDK]資料新增成功。");
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.removeData(string ConditionData)
        {
            try
            {

                BsonDocument filter = BsonDocument.Parse(ConditionData);

                 await _Collection.DeleteOneAsync(filter);
                return Result.setResult("[MongoDBSDK]刪除資料成功。");
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        async Task<IResult> IRepository<T>.updateData(string ConditionData, T UpdateData)
        {
            try
            {
                BsonDocument filter = BsonDocument.Parse(ConditionData);

                BsonDocument _updateData = MongoMap.getInstance().ToBsonDocument(UpdateData);

                var queryResult =  _Collection.FindOneAndUpdateAsync(filter, _updateData);
                return Result.setResult("[MongoDBSDK]資料更新成功。", queryResult.ToJson());
            }
            catch (Exception ex)
            {
                return Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
    }
}
