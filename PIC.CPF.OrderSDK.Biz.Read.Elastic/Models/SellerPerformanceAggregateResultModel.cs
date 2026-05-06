using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SellerPerformanceAggregateResultModel
    {
        /// <summary>
        /// 總訂單數
        /// </summary>
        [JsonPropertyName("orderCount")]
        public int OrderCount { get; set; }

        /// <summary>
        /// 總寄件數
        /// </summary>
        [JsonPropertyName("sendCount")]
        public int SendCount { get; set; }

        /// <summary>
        /// 總銷售額
        /// </summary>
        [JsonPropertyName("salesAmt")]
        public int SalesAmt { get; set; }
    }
}
