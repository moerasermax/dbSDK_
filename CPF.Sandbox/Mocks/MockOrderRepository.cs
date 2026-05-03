using MongoDB.Bson;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Utils;
using System.Collections.Concurrent;

namespace CPF.Sandbox.Mocks
{
    public class MockOrderData
    {
        public string? CoomNo { get; set; }
        public string? CoomStatus { get; set; }
        public string? EsmmStatus { get; set; }
    }

    public class MockUpdateOptions
    {
        public List<string>? PushFields { get; set; }
    }

    /// <summary>
    /// 具備內存存儲的 Mock 倉儲，用於離線驗證 MongoDB 指令邏輯。
    /// S15.1：直接呼叫 MongoRepository&lt;BsonDocument&gt;.FlattenBsonDocument()
    /// 確保每一行指令都走 SDK 的正式邏輯路徑。
    /// </summary>
    public class MockOrderRepository
    {
        private readonly ConcurrentDictionary<string, BsonDocument> _store = new();

        public Task<IResult> Insert(BsonDocument doc)
        {
            try
            {
                if (!doc.TryGetValue("coom_no", out var keyVal) || keyVal.IsBsonNull)
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Insert", "文件缺少 coom_no 欄位，無法存入 Mock Store。"));

                string key = keyVal.AsString;
                var normalized = MongoCommandBuilder.Normalize(doc);
                _store[key] = normalized.AsBsonDocument;
                return Task.FromResult<IResult>(Result.SetResult($"[Mock] Insert 成功。coom_no={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Insert", ex.ToString()));
            }
        }

        public Task<IResult> GetData(string filterJson)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);
                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(Result.SetErrorResult("GetData", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (_store.TryGetValue(key, out var doc))
                    return Task.FromResult<IResult>(Result.SetResult("[Mock] 查詢成功。", doc.ToJson()));

                return Task.FromResult<IResult>(Result.SetErrorResult("GetData", $"[Mock] 查無資料。coom_no={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("GetData", ex.ToString()));
            }
        }

        public Task<IResult> Update(string filterJson, BsonDocument rawDoc)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);
                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(Result.SetErrorResult("Update", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (!_store.TryGetValue(key, out var existing))
                    return Task.FromResult<IResult>(Result.SetErrorResult("Update", $"[Mock] 查無資料，無法更新。coom_no={key}"));

                BsonDocument flatDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(rawDoc);
                ApplyDotNotationSet(existing, flatDoc);
                return Task.FromResult<IResult>(Result.SetResult("[Mock] Update 成功。", existing.ToJson()));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Update", ex.ToString()));
            }
        }

        public Task<IResult> Push(string filterJson, BsonDocument pushDoc)
        {
            try
            {
                var filter = BsonDocument.Parse(filterJson);
                if (!filter.TryGetValue("coom_no", out var keyVal))
                    return Task.FromResult<IResult>(Result.SetErrorResult("Push", "Filter 缺少 coom_no 欄位。"));

                string key = keyVal.AsString;
                if (!_store.TryGetValue(key, out var existing))
                    return Task.FromResult<IResult>(Result.SetErrorResult("Push", $"[Mock] 查無資料，無法 Push。coom_no={key}"));

                var normalizedPushDoc = MongoCommandBuilder.Normalize(pushDoc).AsBsonDocument;
                foreach (var el in normalizedPushDoc)
                {
                    if (!existing.Contains(el.Name) || !existing[el.Name].IsBsonArray)
                        existing[el.Name] = new BsonArray();
                    var arr = existing[el.Name].AsBsonArray;
                    if (el.Value.IsBsonArray)
                        foreach (var item in el.Value.AsBsonArray) arr.Add(item);
                    else if (el.Value.IsBsonDocument)
                        arr.Add(el.Value);
                }
                return Task.FromResult<IResult>(Result.SetResult("[Mock] Push 成功。", existing.ToJson()));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Push", ex.ToString()));
            }
        }

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

                if (options.PushFields?.Count > 0)
                {
                    var pushDoc = new BsonDocument();
                    foreach (var field in options.PushFields)
                    {
                        pushDoc.Add(field, new BsonDocument("$each",
                            new BsonArray { new BsonDocument { { "status", "10" }, { "datetime", DateTime.UtcNow.ToString("o") } } }));
                    }
                    updateDef.Add("$push", pushDoc);
                }

                var result = new { Filter = BsonDocument.Parse(condition), UpdateDefinition = updateDef, Options = new { IsUpsert = true } };
                return Task.FromResult<IResult>(Result.SetResult("[Mock] 更新指令初始化完成。", System.Text.Json.JsonSerializer.Serialize(result)));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("UpdateInit", ex.ToString()));
            }
        }

        public IEnumerable<string> GetAllKeys() => _store.Keys;
        public void Clear() => _store.Clear();

        private static void ApplyDotNotationSet(BsonDocument doc, BsonDocument setDoc)
        {
            foreach (var el in setDoc)
            {
                if (el.Value.IsBsonNull) continue;
                var parts = el.Name.Split('.');
                BsonDocument current = doc;
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    string part = parts[i];
                    if (!current.Contains(part) || !current[part].IsBsonDocument)
                        current[part] = new BsonDocument();
                    current = current[part].AsBsonDocument;
                }
                current[parts[^1]] = el.Value;
            }
        }
    }
}
