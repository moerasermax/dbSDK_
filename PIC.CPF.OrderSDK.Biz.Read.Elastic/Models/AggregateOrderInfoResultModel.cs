using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AggregateOrderInfoResultModel
    {
        /// <summary>
        /// 買家總覽資訊-統計結果
        /// </summary>
        [JsonPropertyName("buyerOverView")]
        public BuyerOverviewAggregateResultModel? BuyerOverview { get; set; }

        /// <summary>
        /// 買家表現資訊-統計結果
        /// </summary>
        [JsonPropertyName("buyerPerformance")]
        public BuyerPerformanceAggregateResultModel? BuyerPerformance { get; set; }

        /// <summary>
        /// 賣家總覽資訊-統計結果
        /// </summary>
        [JsonPropertyName("sellerOverView")]
        public SellerOverviewAggregateResultModel? SellerOverview { get; set; }

        /// <summary>
        /// 賣家表現資訊-統計結果
        /// </summary>
        [JsonPropertyName("sellerPerformance")]
        public SellerPerformanceAggregateResultModel? SellerPerformance { get; set; }
    }
}
