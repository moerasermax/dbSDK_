using System.Text.Json;
using System.Text.Json.Serialization;

namespace CPF.Sandbox.Mocks
{
    /// <summary>
    /// ES 資料植入用的簡易 Document。
    /// [JsonExtensionData] 將所有未宣告的 JSON 欄位原樣保留，
    /// 序列化時再展開，無需複製完整 OrderDocument 結構。
    /// </summary>
    public class TestOrderDocument
    {
        [JsonIgnore]
        public string Id => CoomNo ?? string.Empty;

        [JsonPropertyName("coom_no")]
        public string CoomNo { get; set; } = string.Empty;

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraData { get; set; }
    }
}
