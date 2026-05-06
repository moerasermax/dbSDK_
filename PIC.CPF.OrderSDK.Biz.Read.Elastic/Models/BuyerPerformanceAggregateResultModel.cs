using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class BuyerPerformanceAggregateResultModel
    {
        /// <summary>
        /// 總購買訂單數
        /// </summary>
        [JsonPropertyName("orderCount")]
        public int OrderCount { get; set; }

        /// <summary>
        /// 總取貨數
        /// </summary>
        [JsonPropertyName("pickupCount")]
        public int PickupCount { get; set; }
    }
}
