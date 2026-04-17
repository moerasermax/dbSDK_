namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class BuyerPerformanceAggregateModel
    {
        /// <summary>
        /// 訂購人會員編號
        /// </summary>
        public int CoocMemSid { get; set; }

        /// <summary>
        /// 訂單開始日期
        /// </summary>
        public DateTime OrderDateStart { get; set; }

        /// <summary>
        /// 訂單結束日期
        /// </summary>
        public DateTime OrderDateEnd { get; set; }
    }
}
