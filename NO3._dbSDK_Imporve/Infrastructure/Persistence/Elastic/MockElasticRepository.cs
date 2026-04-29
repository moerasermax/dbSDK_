using Elastic.Clients.Elasticsearch;
using NO3._dbSDK_Imporve.Core.Interface;
using System.Text.Json;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    /// <summary>
    /// Mock Elastic 倉儲
    /// 用於攔截指令並產出 DSL JSON，不依賴實體 Elastic 服務
    /// </summary>
    public class MockElasticRepository<T> : IRepository<T>
    {
        public string _indexName { get; private set; }
        private readonly ElasticsearchClient _client;

        public MockElasticRepository(string indexName = "mock_index")
        {
            _indexName = indexName;
            // 使用空的設定初始化 Client，僅用於序列化
            _client = new ElasticsearchClient();
        }

        public IResult ChangeTable(string TableName)
        {
            _indexName = TableName;
            return Core.Models.Result.SetResult($"[MockElastic] 已切換 Index 為: {TableName}");
        }

        public Task<Core.Interface.IResult> GetData(string ConditionData_Json)
        {
            // 這裡暫不實作從 JSON 轉 Filter 的邏輯，因為沙盒主要驗證強型別 Filter
            return Task.FromResult<Core.Interface.IResult>(Core.Models.Result.SetErrorResult("GetData", "Mock 模式下請直接使用強型別 Filter 驗證。"));
        }

        /// <summary>
        /// 核心驗證方法：攔截 ElasticFilter 並轉為 DSL JSON
        /// </summary>
        public string GetSearchDsl(ElasticFilter filter)
        {
            var request = new SearchRequestDescriptor<object>();
            request.Index(_indexName)
                   .Query(q => q.Bool(b => b
                       .Filter(filter.MustConditions.ToArray())
                       .Should(filter.ShouldConditions.ToArray())
                   ));

            // 使用 Elastic SDK 內建的序列化器取得原始 DSL
            using var ms = new MemoryStream();
            _client.ElasticsearchClientSettings.SourceSerializer.Serialize(request, ms);
            ms.Position = 0;
            using var sr = new StreamReader(ms);
            return sr.ReadToEnd();
        }

        public Task<IResult> InsertData(T Data) => throw new NotImplementedException();
        public Task<IResult> RemoveData(string ConditionData_Json) => throw new NotImplementedException();
        public Task<IResult> UpdateData(string ConditionData_Json, T UpdateData) => throw new NotImplementedException();
    }
}
