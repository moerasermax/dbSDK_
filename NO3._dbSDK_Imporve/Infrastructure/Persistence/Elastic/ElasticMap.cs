using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using System.Text.Json;
using System.Text.Json.Nodes;

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

                // 【S23 新增】：支援 _id 查詢 (包括 _coom_no 映射到 _id)
                // 當 key 是 _coom_no (Condition 類別的屬性) 時，視為 _id 查詢
                if (key == "_id" || key.ToLower() == "id" || key == "_coom_no")
                {
                    // 使用 ids 查詢來查詢文檔 ID
                    filter.ShouldConditions.Add(q => q.Ids(i => i.Values(value.GetString()!)));
                    continue;
                }

                // 【關鍵新增】：處理 Elasticsearch 的字串分詞陷阱
                // 如果值是字串，且不是 Elasticsearch 的保留字元 (如 _id)
                if (value.ValueKind == JsonValueKind.String)
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

    /// <summary>
    /// S22 過濾：只允許 snake_case 欄位寫入 ElasticSearch
    /// 過濾掉 PascalCase 欄位 (來自父類別) 和 null 值
    /// </summary>
    public Dictionary<string, object> FilterUpdateData(string updateJson)
    {
        var filteredUpdateMap = new Dictionary<string, object>();
        
        var updateMap = JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(updateJson);
        
        if (updateMap != null)
        {
            foreach (var kv in updateMap)
            {
                // 過濾條件：
                // 1. key 必須是 snake_case (包含底線) - 如 "coom_status", "esmm_ship_no"
                // 2. 排除 PascalCase 欄位 (Id, PK, CoomStatus, EsmmShipNo, EsmmStatus 等)
                // 3. 排除 null 值
                // 4. 排除全大寫欄位 (如 ID, PK)
                bool isSnakeCase = kv.Key.Contains("_") && kv.Key.Any(char.IsLower) && !kv.Key.Any(char.IsUpper);
                
                if (isSnakeCase)
                {
                    // 取得實際值
                    var value = kv.Value;
                    if (value != null && value.GetValueKind() != JsonValueKind.Null)
                    {
                        // 轉換 JsonNode 為基本類型
                        if (value is JsonValue jv)
                        {
                            if (jv.TryGetValue<int>(out int intVal))
                                filteredUpdateMap[kv.Key] = intVal;
                            else if (jv.TryGetValue<string>(out string strVal))
                                filteredUpdateMap[kv.Key] = strVal;
                            else if (jv.TryGetValue<bool>(out bool boolVal))
                                filteredUpdateMap[kv.Key] = boolVal;
                            else
                                filteredUpdateMap[kv.Key] = value.ToString();
                        }
                        else
                        {
                            filteredUpdateMap[kv.Key] = value.ToString();
                        }
                    }
                }
                // PascalCase 欄位不允許寫入 (來自父類別 OrderSummary)
            }
        }
        
        return filteredUpdateMap;
    }

    public ElasticMap()
    {
    }
}