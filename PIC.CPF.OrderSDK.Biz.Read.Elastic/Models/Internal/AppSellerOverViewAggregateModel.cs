namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class AppSellerOverViewAggregateResultModel
    {
        /// <summary>
        /// 新訂單：同賣家總覽下的新訂單數量
        /// </summary>
        public int NewOrderCnt { get; set; }

        /// <summary>
        /// 待出貨訂單：同賣家總覽下的待出貨訂單數量
        /// </summary>
        public int ShippedCnt { get; set; }

        /// <summary>
        /// 待回覆訂單：同賣家總覽下的待回覆訂單數量
        /// </summary>
        public int RepliedCnt { get; set; }

        /// <summary>
        /// 取貨倒數：區間內的訂單，並距離退貨剩3天之包裹數量
        /// </summary>
        public int PickupCnt { get; set; }
    }
}
