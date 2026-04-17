namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AggregateOrderInfoResultModel
    {
        /// <summary>
        /// 買家總覽資訊-統計結果
        /// </summary>
        public BuyerOverviewAggregateResultModel[]? BuyerOverview { get; set; } = null;

        /// <summary>
        /// 買家表現資訊-統計結果
        /// </summary>
        public BuyerPerformanceAggregateResultModel[]? BuyerPerformance { get; set; } = null;

        /// <summary>
        /// 賣家總覽資訊-統計結果
        /// </summary>
        public SellerOverviewAggregateResultModel[]? SellerOverview { get; set; } = null;

        /// <summary>
        /// 賣家表現資訊-統計結果
        /// </summary>
        public SellerPerformanceAggregateResultModel[]? SellerPerformance { get; set; } = null;

        /// <summary>
        /// 查詢時間
        /// </summary>
        public long Took { get; set; }
    }
}
