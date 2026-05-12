namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 4 (GetAppDashboardOverview) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost("GetAppDashboardOverview")] GetAppDashboardOverview(GetAppDashboardOverviewModel model)`。
    /// 依 Search_4 In 樣張:cuamCid。
    /// ⚠️ PENDING_BUSINESS_LOGIC:Golden In 未含時間區間、BLL fallback 90 天規則待客戶確認;
    /// SDK 介面預留 SearchStartDate/EndDate 給後續若客戶補完規格。
    /// </summary>
    public class GetAppDashboardOverviewModel
    {
        public int? CuamCid { get; init; }

        /// <summary>
        /// 查詢起始日期 (可選、客戶端 Golden In 未強制、BLL 預設 fallback 90 天)
        /// </summary>
        public DateTime? SearchStartDate { get; init; }

        /// <summary>
        /// 查詢結束日期 (可選、客戶端 Golden In 未強制、BLL 預設 fallback UtcNow)
        /// </summary>
        public DateTime? SearchEndDate { get; init; }
    }
}
