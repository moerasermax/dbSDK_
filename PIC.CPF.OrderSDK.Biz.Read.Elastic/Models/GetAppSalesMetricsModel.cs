using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 5/6 (GetAppSalesMetrics) Public Input Model — 共用 Today / SetWeek。
    /// 對齊客戶 Controller `[HttpPost("GetAppSalesMetrics")] GetAppSalesMetrics(GetAppSalesMetricsModel model)`。
    /// 依 Search_5/6 In 樣張:cuamCid + searchStartDate/EndDate + startDatePoP/endDatePoP + dateRangeType。
    /// dateRangeType=Today → Search 5;dateRangeType=SetWeek → Search 6 (本 SDK 暴露兩個 endpoint、客戶端透過 model.DateRangeType 切換)。
    /// PoP 區間:caller 可不傳、BLL 預設 = main range (依 S41-H 業務規則)。
    /// </summary>
    public class GetAppSalesMetricsModel
    {
        public int? CuamCid { get; init; }
        public DateTime? SearchStartDate { get; init; }
        public DateTime? SearchEndDate { get; init; }
        public DateTime? DateStartPoP { get; init; }
        public DateTime? DateEndPoP { get; init; }
        public DateRangeType? DateRangeType { get; init; }
    }
}
