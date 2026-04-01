using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    public class ElasticRepository<T> : IRepository<T>
    {
        public IResult Result { get; set; } = new Core.Models.Result();
        public string _indexName { get; set;  } ///TableName
        protected ElasticsearchClient _client; ///Elastic入口點只有 client
        protected ElasticMap _map = ElasticMap.getInstance();

        public ElasticRepository(ElasticDriver driver, string indexName) 
        {
            _client = driver.getClient();
            _indexName = indexName;

        }

        public IResult changeTable(string TableName)
        {
            _indexName = TableName;
            this.Result.setResult(true, "[ElasticSDK]已設置 indexName。");
            return Result;
        }

        public async Task<IResult> getData(string ConditionData)
        {
            ElasticFilter filter = _map.toFilter(ConditionData);

            try
            {
                var response = await _client.SearchAsync<object>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.MustConditions.ToArray()) // AND 邏輯
                        .Should(filter.ShouldConditions.ToArray()) // OR 邏輯
                    )
                ));

                this.Result.setResult(true, "[ElasticSDK]已成功取得資料。", JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                this.Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
            return Result;
        }

        public async Task<IResult> insertData(T Data)
        {
            var autoId = Guid.NewGuid().ToString();
            try
            {
                var response = await _client.CreateAsync(Data, q => q.Index(_indexName));
                this.Result.setResult(true, "[ElasticSDK]{0} 新增完成。", response.Id);
            }
            catch (Exception ex)
            {
                this.Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
            return Result;
        }

        public async Task<IResult> removeData(string ConditionData)
        {
            try
            {
                ElasticFilter filter = _map.toFilter(ConditionData);

                var response = await _client.DeleteByQueryAsync<object>(s => s
                .Query(q => q
                    .Bool(b => b
                        .Filter(filter.MustConditions.ToArray()) // AND 邏輯
                        .Should(filter.ShouldConditions.ToArray()) // OR 邏輯
                    )
                ));

                this.Result.setResult(true, "[ElasticSDK]已成功取得資料。", JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                this.Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
            return Result;
        }

        public async Task<IResult> updateData(string ConditionData, T UpdateData)
        {
            var json = JsonSerializer.Serialize(UpdateData);
            var map = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            ElasticFilter filter = new ElasticFilter().Eq("Name", ConditionData);

            try
            {
                var response = await _client.UpdateByQueryAsync<object>(_indexName, u => u
                .Query(q => q.Bool(b => b.Filter(filter.MustConditions.ToArray())))
                .Script(s => s
                    .Source("foreach (e in params.f.entrySet()) { ctx._source[e.key] = e.value; }")
                    .Params(p =>
                    {
                        foreach (var kv in map) p.Add(kv.Key, kv.Value);
                    })
                )
                );

                if (response.IsValidResponse)
                {
                    this.Result.setResult(true, "[ElasticSDK] 更新完成，共影響 {0} 筆資料。", JsonSerializer.Serialize(response));
                }
                else
                {
                    this.Result.setErrorResult("updateDataByFilter", response.DebugInformation);
                }
            }
            catch (Exception ex)
            {
                this.Result.setErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.Message);
            }
            return Result;
        }
    }
}
