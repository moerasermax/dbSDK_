namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 4 (GetAppDashboardOverview) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost("GetAppDashboardOverview")] GetAppDashboardOverview(GetAppDashboardOverviewModel model)`。
    /// 依 Search_4 In 樣張只含 cuamCid。
    /// 時間區間由 BLL 內部依 IClock 算出 (Overview=過去 90 天、Performance=本週一~today+1)、
    /// 對齊客戶原 SDK BLL 邏輯;測試環境可注入 FixedClock 固定 today。
    /// </summary>
    public class GetAppDashboardOverviewModel
    {
        public int? CuamCid { get; init; }
    }
}
