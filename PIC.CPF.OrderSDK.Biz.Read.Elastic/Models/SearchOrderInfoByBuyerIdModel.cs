using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 3 (SearchOrderInfoByBuyerId) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost("order/buyer")] SearchOrderInfoByBuyerId(SearchOrderInfoByBuyerIdModel model)`。
    /// 依 Search_3 In 樣張:PageIndex/PageSize + MemSid + OrderDateStart/End + CoomNo/CoocNo + OrderState + OrderSorts + IsQaList + BindMembersArray。
    /// 注意:買家視角用 MemSid 不是 CuamCid。
    /// </summary>
    public class SearchOrderInfoByBuyerIdModel
    {
        public OrderSearchPageInfo? PageInfo { get; init; }
        public int? MemSid { get; init; }
        public DateTime? OrderDateStart { get; init; }
        public DateTime? OrderDateEnd { get; init; }
        public string? CoomNo { get; init; }
        public string? CoocNo { get; init; }
        public OrderState? OrderState { get; init; }
        public OrderSort[]? OrderSorts { get; init; }
        public bool? IsQaList { get; init; }
        public int[]? BindMembersArray { get; init; }
    }
}
