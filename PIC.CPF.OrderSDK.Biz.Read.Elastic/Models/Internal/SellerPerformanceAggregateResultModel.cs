namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class SellerPerformanceAggregateResultModel
    {
        /// <summary>
        /// 總訂單數
        /// </summary>
        public int OrderCount { get; set; }

        /// <summary>
        /// 總寄件數
        /// </summary>
        public int SendCount { get; set; }

        /// <summary>
        /// 總銷售額
        /// </summary>
        public int SalesAmt { get; set; }
    }
}
