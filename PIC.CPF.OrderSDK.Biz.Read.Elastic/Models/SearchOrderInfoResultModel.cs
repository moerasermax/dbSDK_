using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SearchOrderInfoResultModel
    {
        [JsonPropertyName("data")]
        public SearchOrderInfoDataModel[]? OrderInfos { get; set; }

        [JsonPropertyName("total")]
        public long Total { get; set; }
    }
}
