using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppSellerOverViewAggregateResultModel
    {
        /// <summary>
        /// 新訂單：同賣家總覽下的新訂單數量
        /// </summary>
        [JsonPropertyName("newOrderCnt")]
        public int NewOrderCnt { get; set; }

        /// <summary>
        /// 待出貨訂單：同賣家總覽下的待出貨訂單數量
        /// </summary>
        [JsonPropertyName("shippedCnt")]
        public int ShippedCnt { get; set; }

        /// <summary>
        /// 待回覆訂單：同賣家總覽下的待回覆訂單數量
        /// </summary>
        [JsonPropertyName("repliedCnt")]
        public int RepliedCnt { get; set; }

        /// <summary>
        /// 取貨倒數：區間內的訂單，並距離退貨剩3天之包裹數量
        /// </summary>
        [JsonPropertyName("pickupCnt")]
        public int PickupCnt { get; set; }
    }
}
