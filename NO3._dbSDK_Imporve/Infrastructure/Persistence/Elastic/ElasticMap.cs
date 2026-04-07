using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using System.Text.Json;

public class ElasticMap
{
    public ElasticFilter ToFilter(string ConditionData_Json)
    {
        ElasticFilter filter = new ElasticFilter();

        using (JsonDocument doc = JsonDocument.Parse(ConditionData_Json))
        {
            JsonElement root = doc.RootElement;

            // 遍歷所有的屬性
            foreach (JsonProperty property in root.EnumerateObject())
            {
                string key = property.Name;
                JsonElement value = property.Value;

                // 【關鍵新增】：處理 Elasticsearch 的字串分詞陷阱
                // 如果值是字串，且不是 Elasticsearch 的保留字元 (如 _id)
                if (value.ValueKind == JsonValueKind.String && key != "_id" && key.ToLower() != "id")
                {
                    // 防呆機制：避免別人已經手動加了 .keyword 我們又重複加，Elastic自動映射一定要加
                    if (!key.EndsWith(".keyword"))
                    {
                        key = $"{key}.keyword";
                    }
                }

                // 使用 .Clone() 解決 ObjectDisposedException
                filter.Eq(key, value.Clone());
            }
        }
        return filter;
    }

    public ElasticMap()
    {
    }
}