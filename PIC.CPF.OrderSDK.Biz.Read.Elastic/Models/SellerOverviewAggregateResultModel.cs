using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SellerOverviewAggregateResultModel
    {
        [JsonPropertyName("dealWith")]
        public int DealWith { get; set; }

        [JsonPropertyName("toship")]
        public int Toship { get; set; }

        [JsonPropertyName("shipping")]
        public int Shipping { get; set; }

        [JsonPropertyName("noShowToDHL")]
        public int NoShowToDHL { get; set; }

        [JsonPropertyName("sellerQaNeverReply")]
        public int SellerQaNeverReply { get; set; }

        [JsonPropertyName("sellerReturnReq")]
        public int SellerReturnReq { get; set; }
    }
}
