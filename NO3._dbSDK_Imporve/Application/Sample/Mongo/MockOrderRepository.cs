using MongoDB.Bson;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using System.Collections.Concurrent;

namespace NO3._dbSDK_Imporve.Application.Sample.Mongo
{
    /// <summary>
    /// Mock 資料類別 (離線測試用)
    /// </summary>
    public class MockOrderData
    {
        public string? CoomNo { get; set; }
        public string? CoomStatus { get; set; }
        public string? EsmmStatus { get; set; }
    }

    /// <summary>
    /// Mock 更新選項 (離線測試用)
    /// </summary>
    public class MockUpdateOptions
    {
        public List<string>? PushFields { get; set; }
    }

    /// <summary>
    /// 具備內存存儲的 Mock 倉儲
    /// S15.1：Update 與 Insert 直接呼叫 MongoRepository&lt;BsonDocument&gt;.FlattenBsonDocument()
    /// 確保 Mock 驗證的每一行指令都是 SDK 的正式邏輯
    /// </summary>
    public class MockOrderRepository
    {
        // ─── In-Memory Store：key = coom_no ───────────────────────
        private readonly ConcurrentDictionary<string, BsonDocument> _store = new();

        // ─────────────────────────────────────────────────────────
        // Insert：將 BsonDocument 存入字典，以 coom_no 為 key
        // ─────────────────────────────────────────────────────────
        public Task<IResult> Insert(BsonDocument doc)
        {
            try
            {
                if (!doc.TryGetValue("coom_no", out var keyVal) || keyVal.IsBsonNull)
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Insert", "文件缺少 coom_no 欄位，無法存入 Mock Store。"));

                string key = keyVal.AsString;
                _store[key] = doc.DeepClone().AsBsonDocument;
                return Task.FromResult<IResult>(
                    Result.SetResult($"[Mock] Insert 成功。coom_no={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Insert", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // GetData：解析 JSON Filter，以 coom_no 查詢 Mock Store
        // ─────────────────────────────────────────────────────────
        public Task<IResult> GetData(string filterJson)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);

                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("GetData", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (_store.TryGetValue(key, out var doc))
                    return Task.FromResult<IResult>(
                        Result.SetResult("[Mock] 查詢成功。", doc.ToJson()));

                return Task.FromResult<IResult>(
                    Result.SetErrorResult("GetData", $"[Mock] 查無資料。coom_no={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("GetData", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // Update：模擬 MongoDB $set 點符號路徑局部更新
        //   - filterJson : { "coom_no": "CM..." }
        //   - rawDoc     : 原始 BsonDocument（可以是巢狀結構）
        //
        //   S15.1：直接呼叫 MongoRepository<BsonDocument>.FlattenBsonDocument()
        //   將原始文件扁平化（含 null 排除、日期轉換），再套用至 Store
        // ─────────────────────────────────────────────────────────
        public Task<IResult> Update(string filterJson, BsonDocument rawDoc)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);

                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Update", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (!_store.TryGetValue(key, out var existing))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Update", $"[Mock] 查無資料，無法更新。coom_no={key}"));

                // ★ 直接呼叫 MongoRepository 的原生靜態方法進行扁平化
                //   與生產環境 UpdateData 的處理路徑完全一致
                BsonDocument flatDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(rawDoc);

                // 套用點符號路徑更新至 in-memory store
                ApplyDotNotationSet(existing, flatDoc);

                return Task.FromResult<IResult>(
                    Result.SetResult("[Mock] Update 成功。", existing.ToJson()));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Update", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // Push：模擬 MongoDB $push 行為，將元素追加至陣列欄位
        //   - filterJson  : { "coom_no": "CM..." }
        //   - pushDoc     : { "e_shipment_l": {BsonDocument}, "e_shipment_s": {BsonDocument} }
        //   每個 value 可以是單一 BsonDocument 或 BsonArray（$each 語意）
        // ─────────────────────────────────────────────────────────
        public Task<IResult> Push(string filterJson, BsonDocument pushDoc)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);

                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Push", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (!_store.TryGetValue(key, out var existing))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Push", $"[Mock] 查無資料，無法 Push。coom_no={key}"));

                foreach (var el in pushDoc)
                {
                    // 確保目標欄位是陣列，若不存在則建立
                    if (!existing.Contains(el.Name) || !existing[el.Name].IsBsonArray)
                        existing[el.Name] = new BsonArray();

                    var arr = existing[el.Name].AsBsonArray;

                    // 支援單一元素或 $each 陣列
                    if (el.Value.IsBsonArray)
                    {
                        foreach (var item in el.Value.AsBsonArray)
                            arr.Add(item);
                    }
                    else if (el.Value.IsBsonDocument)
                    {
                        arr.Add(el.Value);
                    }
                }

                return Task.FromResult<IResult>(
                    Result.SetResult("[Mock] Push 成功。", existing.ToJson()));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Push", ex.ToString()));
            }
        }
        // ─────────────────────────────────────────────────────────
        public Task<IResult> UpdateInit(string condition, MockOrderData data, MockUpdateOptions options)
        {
            try
            {
                var updateDef = new BsonDocument();

                var setDoc = new BsonDocument
                {
                    { "c_order_m.coom_status", data.CoomStatus ?? "" },
                    { "e_shipment_m.esmm_status", data.EsmmStatus ?? "" }
                };
                updateDef.Add("$set", setDoc);

                if (options.PushFields != null && options.PushFields.Count > 0)
                {
                    var pushDoc = new BsonDocument();
                    foreach (var field in options.PushFields)
                    {
                        pushDoc.Add(field, new BsonDocument("$each",
                            new BsonArray
                            {
                                new BsonDocument
                                {
                                    { "status", "10" },
                                    { "datetime", DateTime.UtcNow.ToString("o") }
                                }
                            }));
                    }
                    updateDef.Add("$push", pushDoc);
                }

                var result = new
                {
                    Filter = BsonDocument.Parse(condition),
                    UpdateDefinition = updateDef,
                    Options = new { IsUpsert = true }
                };

                return Task.FromResult<IResult>(Result.SetResult(
                    "[Mock] 更新指令初始化完成。",
                    System.Text.Json.JsonSerializer.Serialize(result)));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("UpdateInit", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // 輔助：套用點符號路徑 $set 到 BsonDocument
        //   例：path = "c_order_m.coom_seller_memo", value = "新備註"
        //   → 找到 doc["c_order_m"]["coom_seller_memo"] 並更新
        //   null 值跳過，不覆蓋舊資料
        // ─────────────────────────────────────────────────────────
        private static void ApplyDotNotationSet(BsonDocument doc, BsonDocument setDoc)
        {
            foreach (var el in setDoc)
            {
                if (el.Value.IsBsonNull) continue; // null 不覆蓋

                var parts = el.Name.Split('.');
                BsonDocument current = doc;

                // 逐層建立或取得子文件
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (!current.Contains(part) || !current[part].IsBsonDocument)
                        current[part] = new BsonDocument();
                    current = current[part].AsBsonDocument;
                }

                // 設定最終欄位值
                current[parts[^1]] = el.Value;
            }
        }

        /// <summary>
        /// 取得目前 Store 中的所有 key（供測試用）
        /// </summary>
        public IEnumerable<string> GetAllKeys() => _store.Keys;

        /// <summary>
        /// 清空 Store（供測試重置用）
        /// </summary>
        public void Clear() => _store.Clear();
    }
}
