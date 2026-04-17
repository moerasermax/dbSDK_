namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AggregateOrderInfoModel
    {
        /// <summary>
        /// 買家總覽資訊-查詢條件
        /// </summary>
        public BuyerOverviewAggregateModel[]? BuyerOverview { get; set; }

        /// <summary>
        /// 買家表現資訊-查詢條件
        /// </summary>
        public BuyerPerformanceAggregateModel[]? BuyerPerformance { get; set; }

        /// <summary>
        /// 賣家總覽資訊-查詢條件
        /// </summary>
        public SellerOverviewAggregateModel[]? SellerOverview { get; set; }

        /// <summary>
        /// 賣家表現資訊-查詢條件
        /// </summary>
        public SellerPerformanceAggregateModel[]? SellerPerformance { get; set; }
    }
}
