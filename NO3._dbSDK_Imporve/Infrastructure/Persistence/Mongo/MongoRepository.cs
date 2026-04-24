using MongoDB.Bson;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Utils;
using System.Globalization;
using System.Reflection;


namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo
{
    public class MongoRepository<T> : IMongoDBRepository<T>
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.ToString());
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.ToString());
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.ToString());
            }
        }

        /// <summary>
        /// Upsert的方式實作，利用扁平化欄位技術，將可完成類別底下的類別指定的欄位更新。
        /// </summary>
        /// <param name="ConditionData_Json">條件資料</param>
        /// <param name="UpdateData">欲更新之資料</param>
        /// <returns></returns>
        async Task<IResult> IRepository<T>.UpdateData(string ConditionData_Json, T UpdateData)
        {
            try
            {
                BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

                // 1. 將泛型 T 轉成原始的 BsonDocument
                BsonDocument rawUpdateData = UpdateData.ToBsonDocument();

                // 2. 呼叫扁平化工具，轉換成點符號格式
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.ToString()}");
            }
        }

        /// <summary>
        /// 將 BsonDocument 扁平化為點符號路徑格式（public static，供 Mock 與沙盒直接呼叫）
        /// 規則：忽略 _id、忽略 null、日期字串轉 BsonDateTime、陣列保留整體
        /// </summary>
        public static BsonDocument FlattenBsonDocument(BsonDocument doc, string prefix = "")
        {
            // 委派給 SDK 統一工具，確保生產邏輯與沙盒邏輯一致
            return MongoCommandBuilder.Flatten(doc, prefix);
        }

        /// <summary>
        /// 嘗試將日期字串轉換為 BsonDateTime（public static，供 Mock 與沙盒直接呼叫）
        /// </summary>
        public static BsonValue TryConvertToBsonDateTime(BsonValue value)
        {
            return MongoCommandBuilder.TryConvertToBsonDateTime(value);
        }

        /// <summary>
        /// 進階更新：支援 $set、$unset 與 $push 合併操作
        /// </summary>
        /// <param name="ConditionData_Json">查詢條件 (JSON 格式)</param>
        /// <param name="UpdateData">欲更新之資料</param>
        /// <param name="options">更新選項 (支援 UnsetFields, PushFields)</param>
        /// <returns></returns>
        public async Task<IResult> UpdateData(string ConditionData_Json, T UpdateData, MongoUpdateOptions options)
        {
            options ??= new MongoUpdateOptions();

            try
            {
                BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

                // 1. 將泛型 T 轉成原始的 BsonDocument
                BsonDocument rawUpdateData = UpdateData.ToBsonDocument();

                // 2. 呼叫扁平化工具，轉換成點符號格式
                BsonDocument flattenedUpdateData = FlattenBsonDocument(rawUpdateData);

                // 3. 建立更新指令 (合併 $set、$unset 與 $push)
                var updateDefinition = new BsonDocument();

                // 3.1 加入 $set (如果有資料)
                if (flattenedUpdateData.ElementCount > 0)
                {
                    updateDefinition.Add("$set", flattenedUpdateData);
                }

                // 3.2 加入 $unset (如果有指定欄位)
                if (options.UnsetFields != null && options.UnsetFields.Count > 0)
                {
                    var unsetDoc = new BsonDocument();
                    foreach (var field in options.UnsetFields)
                    {
                        // $unset 格式: { "欄位路徑": "" }
                        unsetDoc.Add(field, "");
                    }
                    updateDefinition.Add("$unset", unsetDoc);
                }

                // 3.3 加入 $push (如果有指定陣列追加欄位)
                if (options.PushFields != null && options.PushFields.Count > 0)
                {
                    var pushDoc = new BsonDocument();
                    foreach (var pushField in options.PushFields)
                    {
                        // $push 格式: { "陣列路徑": { "$each": [元素] } }
                        // 支援單一元素或多個元素
                        if (pushField.Value.IsBsonArray)
                        {
                            pushDoc.Add(pushField.Key, new BsonDocument("$each", pushField.Value));
                        }
                        else
                        {
                            // 單一元素包裝成 $each 陣列
                            pushDoc.Add(pushField.Key, new BsonDocument("$each", new BsonArray { pushField.Value }));
                        }
                    }
                    updateDefinition.Add("$push", pushDoc);
                }

                // 防呆機制：如果 $set、$unset 和 $push 都沒有
                if (updateDefinition.ElementCount == 0)
                {
                    return Result.SetResult("[MongoDBSDK]沒有需要更新的有效欄位。");
                }

                // 4. 轉換 Options
                var mongoOptions = new FindOneAndUpdateOptions<T>
                {
                    IsUpsert = options.IsUpsert,
                    ReturnDocument = options.ReturnNewDocument ? ReturnDocument.After : ReturnDocument.Before
                };

                // 5. 執行更新
                var queryResult = await _Collection.FindOneAndUpdateAsync(filter, updateDefinition, mongoOptions);

                if (queryResult != null)
                {
                    return Result.SetResult("[MongoDBSDK]資料更新成功。", queryResult.ToJson());
                }
                else
                {
                    // 如果 IsUpsert=true 且查無資料，會建立新文件，此時 queryResult 可能為 null
                    // 需要重新查詢確認
                    if (options.IsUpsert)
                    {
                        var checkResult = await ((IRepository<T>)this).GetData(ConditionData_Json);
                        if (checkResult.IsSuccess)
                        {
                            return Result.SetResult("[MongoDBSDK]資料新增成功 (Upsert)。", checkResult.DataJson);
                        }
                    }
                    return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, "[MongoDBSDK]資料更新失敗。");
                }
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.ToString()}");
            }
        }

        /// <summary>
        /// 初始化更新指令 (供進階使用者直接取得 BsonDocument)
        /// </summary>
        /// <param name="ConditionData_Json">查詢條件 (JSON 格式)</param>
        /// <param name="UpdateData">欲更新之資料</param>
        /// <param name="options">更新選項</param>
        /// <returns>回傳更新指令的 JSON 字串供檢查</returns>
        public Task<IResult> UpdateInit(string ConditionData_Json, T UpdateData, MongoUpdateOptions options)
        {
            options ??= new MongoUpdateOptions();

            try
            {
                BsonDocument filter = BsonDocument.Parse(ConditionData_Json);

                // 1. 將泛型 T 轉成原始的 BsonDocument
                BsonDocument rawUpdateData = UpdateData.ToBsonDocument();

                // 2. 呼叫扁平化工具，轉換成點符號格式
                BsonDocument flattenedUpdateData = FlattenBsonDocument(rawUpdateData);

                // 3. 建立更新指令 (合併 $set、$unset 與 $push)
                var updateDefinition = new BsonDocument();

                // 3.1 加入 $set (如果有資料)
                if (flattenedUpdateData.ElementCount > 0)
                {
                    updateDefinition.Add("$set", flattenedUpdateData);
                }

                // 3.2 加入 $unset (如果有指定欄位)
                if (options.UnsetFields != null && options.UnsetFields.Count > 0)
                {
                    var unsetDoc = new BsonDocument();
                    foreach (var field in options.UnsetFields)
                    {
                        unsetDoc.Add(field, "");
                    }
                    updateDefinition.Add("$unset", unsetDoc);
                }

                // 3.3 加入 $push (如果有指定陣列追加欄位)
                if (options.PushFields != null && options.PushFields.Count > 0)
                {
                    var pushDoc = new BsonDocument();
                    foreach (var pushField in options.PushFields)
                    {
                        // $push 格式: { "陣列路徑": { "$each": [元素] } }
                        if (pushField.Value.IsBsonArray)
                        {
                            pushDoc.Add(pushField.Key, new BsonDocument("$each", pushField.Value));
                        }
                        else
                        {
                            pushDoc.Add(pushField.Key, new BsonDocument("$each", new BsonArray { pushField.Value }));
                        }
                    }
                    updateDefinition.Add("$push", pushDoc);
                }

                // 4. 回傳完整指令供檢查
                var result = new
                {
                    Filter = filter,
                    UpdateDefinition = updateDefinition,
                    Options = new
                    {
                        IsUpsert = options.IsUpsert,
                        ReturnNewDocument = options.ReturnNewDocument
                    }
                };

                return Task.FromResult<IResult>(Result.SetResult("[MongoDBSDK]更新指令初始化完成。", System.Text.Json.JsonSerializer.Serialize(result)));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.ToString()}"));
            }
        }
    }
}
