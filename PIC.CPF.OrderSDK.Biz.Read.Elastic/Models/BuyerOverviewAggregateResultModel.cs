using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class BuyerOverviewAggregateResultModel
    {
        [JsonPropertyName("unpaid")]
        public int Unpaid { get; set; }

        [JsonPropertyName("toship")]
        public int Toship { get; set; }

        [JsonPropertyName("toFinish")]
        public int ToFinish { get; set; }

        [JsonPropertyName("cancel")]
        public int Cancel { get; set; }

        [JsonPropertyName("buyerQaNeverReply")]
        public int BuyerQaNeverReply { get; set; }

        [JsonPropertyName("finish")]
        public int Finish { get; set; }

        [JsonPropertyName("buyerReturnReq")]
        public int BuyerReturnReq { get; set; }
    }
}
