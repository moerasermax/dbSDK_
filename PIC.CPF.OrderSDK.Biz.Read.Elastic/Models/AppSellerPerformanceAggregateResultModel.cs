using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppSellerPerformanceAggregateResultModel
    {
        /// <summary>
        /// 本週銷售額：區間內未取消的訂單，取訂單金額(含運費)加總
        /// </summary>
        [JsonPropertyName("salesAmount")]
        public int SalesAmount { get; set; }

        /// <summary>
        /// 區間內未取消的訂單，取訂單筆數
        /// </summary>
        [JsonPropertyName("totalOrderQty")]
        public int TotalOrderQty { get; set; }
    }
}
