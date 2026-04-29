using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using System.Linq.Expressions;
using System.Reflection;

namespace NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic
{
    /// <summary>
    /// Elastic 強型別濾鏡
    /// S16：消除硬編碼字串，改為支援運算式解析
    /// </summary>
    public class ElasticFilter
    {
        // 儲存所有的過濾條件分支
        public List<Action<QueryDescriptor<object>>> MustConditions { get; } = new();
        public List<Action<QueryDescriptor<object>>> ShouldConditions { get; } = new();

        #region 強型別方法

        /// <summary>
        /// 等於 (Equal) - 強型別版本
        /// </summary>
        public ElasticFilter Term<T>(Expression<Func<T, object>> propertyExpression, object value)
        {
            var fieldName = GetFieldName(propertyExpression);
            return Term(fieldName, value);
        }

        /// <summary>
        /// 大於等於 (Greater Than or Equal)
        /// </summary>
        public ElasticFilter Gte<T>(Expression<Func<T, object>> propertyExpression, object value)
        {
            var fieldName = GetFieldName(propertyExpression);
            MustConditions.Add(q => q.Range(r => r.NumberRange(n => n.Field(fieldName).Gte(Convert.ToDouble(value)))));
            return this;
        }

        /// <summary>
        /// 小於等於 (Less Than or Equal)
        /// </summary>
        public ElasticFilter Lte<T>(Expression<Func<T, object>> propertyExpression, object value)
        {
            var fieldName = GetFieldName(propertyExpression);
            MustConditions.Add(q => q.Range(r => r.NumberRange(n => n.Field(fieldName).Lte(Convert.ToDouble(value)))));
            return this;
        }

        /// <summary>
        /// 包含於清單 (In / Terms)
        /// </summary>
        public ElasticFilter In<T>(Expression<Func<T, object>> propertyExpression, IEnumerable<object> values)
        {
            var fieldName = GetFieldName(propertyExpression);
            // 透過隱式轉換處理字串到 FieldValue
            var terms = values.Select(v => (FieldValue)(v?.ToString() ?? "")).ToArray();
            MustConditions.Add(q => q.Terms(t => t.Field($"{fieldName}.keyword").Terms(new TermsQueryField(terms))));
            return this;
        }

        /// <summary>
        /// 模糊搜尋 (Match)
        /// </summary>
        public ElasticFilter Contains<T>(Expression<Func<T, object>> propertyExpression, string value)
        {
            var fieldName = GetFieldName(propertyExpression);
            return Contains(fieldName, value);
        }

        #endregion

        #region 字串版本（向後相容）

        // 等於 (Equal) - 自動處理 .keyword
        public ElasticFilter Eq(string field, object value)
        {
            return Term(field, value);
        }

        public ElasticFilter Term(string field, object value)
        {
            string fieldName = value is string ? $"{field}.keyword" : field;
            MustConditions.Add(q => q.Term(t => t.Field(fieldName).Value(value?.ToString() ?? "")));
            return this;
        }

        public ElasticFilter Gte(string field, double value)
        {
            MustConditions.Add(q => q.Range(r => r.NumberRange(n => n.Field(field).Gte(value))));
            return this;
        }

        public ElasticFilter Contains(string field, string value)
        {
            MustConditions.Add(q => q.Match(m => m.Field(field).Query(value)));
            return this;
        }

        public ElasticFilter Or(string field, object value)
        {
            string fieldName = value is string ? $"{field}.keyword" : field;
            ShouldConditions.Add(q => q.Term(t => t.Field(fieldName).Value(value?.ToString() ?? "")));
            return this;
        }

        #endregion

        #region 輔助方法

        /// <summary>
        /// 從運算式取得欄位名稱
        /// 支援：
        /// 1. JsonPropertyName 屬性標籤
        /// 2. 巢狀路徑：m => m.Logistics.Status -> "logistics.status"
        /// </summary>
        public static string GetFieldName<T>(Expression<Func<T, object>> expression)
        {
            Expression body = expression.Body;

            // 處理轉換 (例如 m => (object)m.Property)
            if (body is UnaryExpression unary) body = unary.Operand;

            var parts = new List<string>();
            while (body is MemberExpression member)
            {
                var fieldName = GetMemberName(member.Member);
                parts.Add(fieldName);
                body = member.Expression;
            }

            parts.Reverse();
            return string.Join(".", parts);
        }

        private static string GetMemberName(MemberInfo member)
        {
            // 優先讀取 JsonPropertyName
            var jsonAttr = member.GetCustomAttribute<System.Text.Json.Serialization.JsonPropertyNameAttribute>();
            if (jsonAttr != null) return jsonAttr.Name;

            // 次要讀取 DataMember
            var dataAttr = member.GetCustomAttribute<System.Runtime.Serialization.DataMemberAttribute>();
            if (dataAttr != null && !string.IsNullOrEmpty(dataAttr.Name)) return dataAttr.Name;

            // 預設轉小寫（符合 Elastic 慣例）
            return member.Name.ToLower();
        }

        #endregion
    }
}