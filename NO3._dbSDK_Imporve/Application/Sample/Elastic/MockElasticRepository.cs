using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System.Collections.Concurrent;

namespace NO3._dbSDK_Imporve.Application.Sample.Elastic
{
    /// <summary>
    /// Mock Elastic Repository
    /// 使用 ConcurrentDictionary 模擬內存存儲
    /// S16：將 Elasticsearch 納入沙盒自動化檢核體系
    /// </summary>
    public class MockElasticRepository
    {
        // ─── In-Memory Store：key = coom_no ───────────────────────
        private readonly ConcurrentDictionary<string, OrderSummary> _store = new();

        // ─────────────────────────────────────────────────────────
        // Insert：將 OrderSummary 存入字典，以 coom_no 為 key
        // ─────────────────────────────────────────────────────────
        public Task<IResult> Insert(OrderSummary model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.PK))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Insert", "文件缺少 PK 欄位，無法存入 Mock Store。"));

                _store[model.PK] = model;
                return Task.FromResult<IResult>(
                    Result.SetResult($"[MockElastic] Insert 成功。PK={model.PK}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Insert", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // Upsert：更新或插入（Elastic 的常見操作）
        //   - filterJson : { "pk": "CM..." }
        //   - updateDoc  : 要更新的欄位（Partial Document）
        // ─────────────────────────────────────────────────────────
        public Task<IResult> Upsert(string filterJson, OrderSummary updateDoc)
        {
            try
            {
                var filter = System.Text.Json.JsonDocument.Parse(filterJson);
                if (!filter.RootElement.TryGetProperty("pk", out var keyElement))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("Upsert", "Filter 缺少 pk 欄位。"));

                string key = keyElement.GetString() ?? "";

                // 如果不存在，則建立新文件
                if (!_store.TryGetValue(key, out var existing))
                {
                    _store[key] = updateDoc;
                    return Task.FromResult<IResult>(
                        Result.SetResult($"[MockElastic] Upsert（新增）成功。PK={key}"));
                }

                // 存在則進行部分更新
                var updated = ElasticCommandBuilder.MergeUpdate(existing, updateDoc);
                _store[key] = updated;

                return Task.FromResult<IResult>(
                    Result.SetResult($"[MockElastic] Upsert（更新）成功。PK={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("Upsert", ex.ToString()));
            }
        }

        // ─────────────────────────────────────────────────────────
        // GetData：以 pk 查詢 Mock Store
        // ─────────────────────────────────────────────────────────
        public Task<IResult> GetData(string filterJson)
        {
            try
            {
                var filter = System.Text.Json.JsonDocument.Parse(filterJson);
                if (!filter.RootElement.TryGetProperty("pk", out var keyElement))
                    return Task.FromResult<IResult>(
                        Result.SetErrorResult("GetData", "Filter 缺少 pk 欄位。"));

                string key = keyElement.GetString() ?? "";
                if (_store.TryGetValue(key, out var doc))
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(doc);
                    return Task.FromResult<IResult>(
                        Result.SetResult("[MockElastic] 查詢成功。", json));
                }

                return Task.FromResult<IResult>(
                    Result.SetErrorResult("GetData", $"[MockElastic] 查無資料。PK={key}"));
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(Result.SetErrorResult("GetData", ex.ToString()));
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