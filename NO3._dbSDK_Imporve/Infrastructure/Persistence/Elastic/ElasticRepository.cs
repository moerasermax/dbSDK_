using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Result = NO3._dbSDK_Imporve.Core.Models.Result;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    public class ElasticRepository<T> : IRepository<T>
    {
        public string _indexName { get; private set; } ///TableName
        protected ElasticsearchClient _client; ///Elastic入口點只有 client
        protected ElasticMap _map;

        public ElasticRepository(ElasticDriver driver, ElasticMap elasticMap, string indexName)
        {
            _client = driver.GetClient();
            _indexName = indexName;
            _map = elasticMap;

        }

        public IResult ChangeTable(string TableName)
        {
            _indexName = TableName;
            return Result.SetResult("[ElasticSDK]已設置 indexName。");
        }

        public async Task<IResult> GetData(string ConditionData_Json)
        {
            ElasticFilter filter = _map.ToFilter(ConditionData_Json);

            try
            {
                // 🚨 這裡改回 <object>，配合 ElasticFilter 裡面的 QueryDescriptor<object>
                var response = await _client.SearchAsync<object>(s => s
                    .Index(_indexName) // 確保有指定 Index
                    .Query(q => q
                        .Bool(b => b
                            .Filter(filter.MustConditions.ToArray())   // AND 邏輯
                            .Should(filter.ShouldConditions.ToArray()) // OR 邏輯
                        )
                    )
                );

                if (response.IsValidResponse)
                {
                    // response.Documents 會是一組 JsonElement 集合，
                    // 序列化後一樣能得到漂亮的 JSON 陣列，例如 [{"event_id":"EVT6289", ...}]
                    return Result.SetResult("[ElasticSDK]已成功取得資料。", JsonSerializer.Serialize(response.Documents));
                }
                else
                {
                    return Result.SetErrorResult("GetData", response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        // --- 新增資料 (Insert) ---
        public async Task<IResult> InsertData(T Data)
        {
            try
            {
                // 1. 檢查索引，不存在則建立並開啟動態映射
                var exists = await _client.Indices.ExistsAsync(_indexName);
                if (!exists.Exists)
                {
                    await _client.Indices.CreateAsync(_indexName, c => c
                        .Mappings(m => m.Dynamic(DynamicMapping.True))
                    );
                }

                // 2. 取得 ID (假設 Model 有 Id 屬性)
                var actualData = Data as dynamic;
                string documentId = actualData?.Id?.ToString();

                // 3. 執行 Indexing (指定 <T> 解決 {} 問題)
                var response = await _client.IndexAsync<object>(Data, i => i
                    .Index(_indexName)
                    .Id(((dynamic)Data).Id.ToString()) // 確保 ID 有傳
                );

                if (response.IsSuccess())
                    return Result.SetResult($"[Elastic] 新增成功: {response.Id}");

                return Result.SetErrorResult("InsertData", response.DebugInformation);
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult("InsertException", ex.Message);
            }
        }

        public async Task<IResult> RemoveData(string ConditionData_Json)
        {
            try
            {
                // 1. 解析過濾條件
                ElasticFilter filter = _map.ToFilter(ConditionData_Json);

                // 2. 執行刪除：將 Index 名稱放在第一個參數
                var response = await _client.DeleteByQueryAsync<object>(
                    _indexName, // 🚨 這裡請務必換成你實際的 Index 名稱 (例如 "query_logs")
                    dq => dq
                        .Query(q => q
                            .Bool(b => b
                                .Filter(filter.MustConditions.ToArray())   // AND: 必須符合的條件
                                .Should(filter.ShouldConditions.ToArray()) // OR: 符合其中之一即可
                                .MinimumShouldMatch(filter.ShouldConditions.Any() ? 1 : 0)
                            )
                        )
                );


                // 3. 驗證 Elasticsearch 是否回傳成功狀態
                if (!response.IsSuccess())
                {
                    // 如果語法錯誤或 ES 報錯，這行能幫你抓出真正的原因
                    return Result.SetErrorResult("ElasticDeleteError", response.DebugInformation);
                }

                // 4. 整理回傳結果 (包含實際刪除的筆數)
                var resultData = new
                {
                    DeletedCount = response.Deleted,
                    TotalMatched = response.Total
                };

                return Result.SetResult("[ElasticSDK]已成功刪除資料。", JsonSerializer.Serialize(resultData));
            }
            catch (Exception ex)
            {
                // 捕捉未預期的系統例外
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }

        public async Task<IResult> UpdateData(string ConditionData_Json, T UpdateData)
        {
            try
            {

                // 先反序列化成 Dictionary，這會迫使內容脫離原始 JsonDocument 緩衝區
                var tempDict = JsonSerializer.Deserialize<Dictionary<string, object>>(ConditionData_Json);
                // 重新序列化成一個全新的、乾淨的字串再傳給 ToFilter
                string safeJson = JsonSerializer.Serialize(tempDict);

                ElasticFilter filter = _map.ToFilter(safeJson);

                // 2. 固化條件清單 (避免延遲載入)
                var materializedConditions = filter.MustConditions.ToList();

                // 3. 處理更新內容 (解決 {} 空值問題)
                string updateJson = JsonSerializer.Serialize(UpdateData, UpdateData.GetType());

                // 4. 執行更新
                // S22 修復：透過 Map 層過濾，只允許 snake_case 欄位寫入
                var filteredUpdateMap = _map.FilterUpdateData(updateJson);

                var response = await _client.UpdateByQueryAsync<object>(_indexName, u => u
                    .Query(q => q.Bool(b => b.Filter(materializedConditions.ToArray())))
                    .Script(s => s
                        .Source("for (e in params.entrySet()) { if (e.key.toLowerCase() != 'id') { ctx._source[e.key] = e.value; } }")
                        .Params(p =>
                        {
                            foreach (var kv in filteredUpdateMap) p.Add(kv.Key, kv.Value);
                        })
                    )
                    .Refresh(true)
                );

                if (response.IsValidResponse)
                {
                    return Result.SetResult($"[ElasticSDK] 更新完成，共影響 {response.Updated} 筆資料。");
                }

                return Result.SetErrorResult("updateDataByFilter", response.DebugInformation);
            }
            catch (Exception ex)
            {
                return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
        }
        public async Task<SearchResponse<T>> AdvancedSearchAsync(Action<SearchRequestDescriptor<T>> configureQuery)
        {
            try
            {
                var response = await _client.SearchAsync<T>(s =>
                {
                    s.Indices(_indexName); // 確保套用 SDK 管控的 Index
                    configureQuery(s);   // 執行客戶自訂的複雜查詢與聚合
                });

                if (!response.IsValidResponse)
                {
                    return null; // 或是依據你 SDK 的 Result 模式回傳錯誤，這裡先簡單處理
                }

                return response;
            }
            catch (Exception ex)
            {
                // 拋出例外或用 Result.SetErrorResult 紀錄
                throw new Exception($"[ElasticSDK] AdvancedSearch Exception: {ex.Message}", ex);
            }
        }
    }
}
