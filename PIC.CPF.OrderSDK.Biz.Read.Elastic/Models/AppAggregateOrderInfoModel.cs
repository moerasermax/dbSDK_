namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppAggregateOrderInfoModel
    {
        /// <summary>
        /// 90天(固定)內新增的訂單查出以下資訊(訂單主檔的建立時間落於該區間)
        /// </summary>
        public AppSellerOverViewAggregateModel[]? AppSellerOverView { get; set; } = null;

        /// <summary>
        /// 當下日期取得本周區間(周1到周日)為條件
        /// </summary>
        public AppSellerPerformanceAggregateModel[]? AppSellerPerformance { get; set; } = null;
    }
}
