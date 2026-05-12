namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 1 (GetHomeToDoOverview) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost] GetHomeToDoOverview(GetHomeToDoOverviewModel model)`、
    /// 客戶 wrapper 可直接 pass-through 我們 SDK。
    /// 依 Search_1 In 樣張:cuamCid + searchStartDate + searchEndDate。
    /// CuamCid 同時用作賣家視角 (CoomCuamCid) 與買家視角 (CoocMemSid)、BLL 內部分發。
    /// </summary>
    public class GetHomeToDoOverviewModel
    {
        public int? CuamCid { get; init; }
        public DateTime? SearchStartDate { get; init; }
        public DateTime? SearchEndDate { get; init; }
    }
}
