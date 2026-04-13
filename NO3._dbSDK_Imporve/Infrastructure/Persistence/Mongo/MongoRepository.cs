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

                // 1. 將泛型 T 轉成原始的 BsonDocument
                BsonDocument rawUpdateData = UpdateData.ToBsonDocument();

                // 2. 呼叫高手的扁平化工具，轉換成點符號格式
                BsonDocument flattenedUpdateData = FlattenBsonDocument(rawUpdateData);

                // 防呆機制：如果傳進來的資料全都是 null 或只有 _id，攤平後會是空的
                if (flattenedUpdateData.ElementCount == 0)
                {
                    return Result.SetResult("沒有需要更新的有效欄位。", null);
                }

                // 3. 使用 $set 包裝攤平後的資料
                var updateDefinition = new BsonDocument("$set", flattenedUpdateData);

                var options = new FindOneAndUpdateOptions<T>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                };

                // 4. 執行更新
                var queryResult = await _Collection.FindOneAndUpdateAsync(filter, updateDefinition, options);

                if (queryResult != null)
                {
                    return Result.SetResult("[MongoDBSDK]資料更新成功。", queryResult.ToJson());
                }
                else
                {
                    return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, "[MongoDBSDK]資料更新失敗。");
                }
            }
            catch (Exception ex)
            {
                // 記得捕捉並印出錯誤，除錯會快很多
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.Message}");
            }
        }

        // 寫在同一個 class 裡面
        private BsonDocument FlattenBsonDocument(BsonDocument doc, string prefix = "")
        {
            var flatDoc = new BsonDocument();

            foreach (var element in doc.Elements)
            {
                // 高手防禦 1：忽略最外層的 _id，防止 Duplicate Key 報錯
                if (prefix == "" && element.Name == "_id")
                    continue;

                // 高手防禦 2：忽略 null 值。這樣前端傳 null 來時，就不會把資料庫裡的舊資料洗空
                if (element.Value.IsBsonNull)
                    continue;

                // 如果該屬性是一個巢狀的 Document，進行遞迴處理
                if (element.Value.IsBsonDocument)
                {
                    // 將前綴加上當前屬性名與點號，例如 "c_order_m."
                    var nestedDoc = FlattenBsonDocument(element.Value.AsBsonDocument, prefix + element.Name + ".");
                    flatDoc.Merge(nestedDoc); // 把遞迴攤平的結果合併進來
                }
                else
                {
                    // 如果是一般值（字串、數字、陣列等），直接加入
                    flatDoc.Add(prefix + element.Name, element.Value);
                }
            }

            return flatDoc;
        }
    }
}
