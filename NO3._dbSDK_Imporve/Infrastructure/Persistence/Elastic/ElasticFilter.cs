using Elastic.Clients.Elasticsearch.QueryDsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    public class ElasticFilter
    {
        // 儲存所有的過濾條件分支
        public List<Action<QueryDescriptor<object>>> MustConditions { get; } = new();
        public List<Action<QueryDescriptor<object>>> ShouldConditions { get; } = new();

        // 等於 (Equal) - 自動處理 .keyword
        public ElasticFilter Eq(string field, object value)
        {
            string fieldName = value is string ? $"{field}.keyword" : field;
            MustConditions.Add(q => q.Term(t => t.Field(fieldName).Value(value.ToString())));
            return this; // 支援鏈式寫法
        }

        // 範圍 (Greater Than or Equal)
        public ElasticFilter Gte(string field, double value)
        {
            MustConditions.Add(q => q.Range(r => r.NumberRange(n => n.Field(field).Gte(value))));
            return this;
        }

        // 模糊搜尋 (Match)
        public ElasticFilter Contains(string field, string value)
        {
            MustConditions.Add(q => q.Match(m => m.Field(field).Query(value)));
            return this;
        }

        // 聯集 (OR 邏輯)
        public ElasticFilter Or(string field, object value)
        {
            string fieldName = value is string ? $"{field}.keyword" : field;
            ShouldConditions.Add(q => q.Term(t => t.Field(fieldName).Value(value.ToString())));
            return this;
        }
    }
}
