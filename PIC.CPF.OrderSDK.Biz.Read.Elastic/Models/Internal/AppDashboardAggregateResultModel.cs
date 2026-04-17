namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class AppDashboardAggregateResultModel
    {
        /// <summary>
        /// 90天(固定)內新增的訂單查出以下資訊(訂單主檔的建立時間落於該區間)
        /// </summary>
        public AppSellerOverViewAggregateResultModel[]? AppDashboard { get; set; } = null;

        /// <summary>
        /// 當下日期取得本周區間(周1到周日)為條件
        /// </summary>
        public AppSellerPerformanceAggregateResultModel[]? AppPerformance { get; set; } = null;

        /// <summary>
        /// 查詢時間
        /// </summary>
        public long Took { get; set; }
    }
}
