using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppDashboardAggregateResultModel
    {
        /// <summary>
        /// 90天(固定)內新增的訂單查出以下資訊(訂單主檔的建立時間落於該區間)
        /// </summary>
        [JsonPropertyName("appSellerOverView")]
        public AppSellerOverViewAggregateResultModel? AppSellerOverView { get; set; }

        /// <summary>
        /// 當下日期取得本周區間(周1到周日)為條件
        /// </summary>
        [JsonPropertyName("appSellerPerformance")]
        public AppSellerPerformanceAggregateResultModel? AppSellerPerformance { get; set; }
    }
}
