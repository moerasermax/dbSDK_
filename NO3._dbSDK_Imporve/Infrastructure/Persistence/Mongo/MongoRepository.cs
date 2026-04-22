using MongoDB.Bson;
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.Message}");
            }
        }

        private BsonDocument FlattenBsonDocument(BsonDocument doc, string prefix = "")
        {
            var flatDoc = new BsonDocument();

            foreach (var element in doc.Elements)
            {
                // 1：忽略最外層的 _id，防止 Duplicate Key 報錯
                if (prefix == "" && element.Name == "_id")
                    continue;

                // 2：忽略 null 值。這樣前端傳 null 來時，就不會把資料庫裡的舊資料洗空
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
                    // 3：日期字串自動轉換為 BsonDateTime (開發憲章第 10 點)
                    var processedValue = TryConvertToBsonDateTime(element.Value);
                    
                    // 如果是一般值（字串、數字、陣列等），直接加入
                    flatDoc.Add(prefix + element.Name, processedValue);
                }
            }

            return flatDoc;
        }

        /// <summary>
        /// 嘗試將 BsonValue 轉換為 BsonDateTime
        /// 支援多語系日期字串格式（包含「下午」格式）
        /// </summary>
        /// <param name="value">原始 BsonValue</param>
        /// <returns>轉換後的 BsonValue (成功則為 BsonDateTime，失敗則回傳原值)</returns>
        private BsonValue TryConvertToBsonDateTime(BsonValue value)
        {
            // 已經是 DateTime，直接回傳
            if (value.BsonType == BsonType.DateTime)
                return value;

            // 不是字串，不處理
            if (value.BsonType != BsonType.String)
                return value;

            var stringValue = value.AsString;
            
            // 空字串不處理
            if (string.IsNullOrWhiteSpace(stringValue))
                return value;

            // 嘗試解析日期字串
            if (TryParseMultiCultureDateTime(stringValue, out var dateTime))
            {
                // 轉換為 UTC milliseconds since epoch
                var utcTicks = dateTime.Kind == DateTimeKind.Local 
                    ? dateTime.ToUniversalTime().Ticks 
                    : DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).Ticks;
                
                // MongoDB 使用 milliseconds since Unix epoch
                var milliseconds = utcTicks / 10000 - 62135596800000;
                return new BsonDateTime(milliseconds);
            }

            // 無法解析，回傳原值
            return value;
        }

        /// <summary>
        /// 嘗試使用多種文化特性解析日期字串
        /// </summary>
        private static bool TryParseMultiCultureDateTime(string dateString, out DateTime result)
        {
            result = default;

            // 支援的日期格式
            var formats = new[]
            {
                "yyyy-MM-ddTHH:mm:ss.fffZ",
                "yyyy-MM-ddTHH:mm:ssZ",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy/MM/dd",
                "yyyy年MM月dd日 tt hh:mm:ss",
                "yyyy年MM月dd日",
                "yyyy/MM/dd tt hh:mm:ss",
                "yyyy/MM/dd 下午 hh:mm:ss",
                "yyyy/MM/dd 上午 hh:mm:ss"
            };

            // 支援的文化特性
            var cultures = new[]
            {
                CultureInfo.InvariantCulture,
                new CultureInfo("zh-TW"),
                new CultureInfo("zh-CN"),
                new CultureInfo("en-US")
            };

            // 先嘗試標準解析
            foreach (var culture in cultures)
            {
                if (DateTime.TryParse(dateString, culture, DateTimeStyles.None, out result))
                    return true;
            }

            // 嘗試精確格式匹配
            foreach (var format in formats)
            {
                foreach (var culture in cultures)
                {
                    if (DateTime.TryParseExact(dateString, format, culture, DateTimeStyles.None, out result))
                        return true;
                }
            }

            // 特殊處理：替換中文上午/下午為 AM/PM
            var normalizedString = dateString
                .Replace("上午", "AM")
                .Replace("下午", "PM");

            if (DateTime.TryParse(normalizedString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return true;

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(normalizedString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 進階更新：支援 $set 與 $unset 合併操作
        /// </summary>
        /// <param name="ConditionData_Json">查詢條件 (JSON 格式)</param>
        /// <param name="UpdateData">欲更新之資料</param>
        /// <param name="options">更新選項 (支援 UnsetFields)</param>
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

                // 3. 建立更新指令 (合併 $set 與 $unset)
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

                // 防呆機制：如果 $set 和 $unset 都沒有
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
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.Message}");
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

                // 3. 建立更新指令 (合併 $set 與 $unset)
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

                return Task.FromResult<IResult>(Result.SetResult("[MongoDBSDK]更新指令初始化完成。", result.ToJson()));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, $"[MongoDBSDK] 例外錯誤: {ex.Message}"));
            }
        }
    }
}
