using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 2 (SearchOrderInfoBySellerId) Public Input Model。
    /// 對齊客戶 Controller `[HttpPost("order/seller")] SearchOrderInfoBySellerId(SearchOrderInfoBySellerIdModel model)`。
    /// 依 Search_2 In 樣張:PageIndex/PageSize + CuamCid + OrderDateStart/End + 多 filter + OrderState + OrderSorts + IsQaList。
    /// </summary>
    public class SearchOrderInfoBySellerIdModel
    {
        // S41-M: flat 化 PageIndex/PageSize、對齊 Golden In JSON 形狀 + 客戶 Model pass-through 整合
        public int? PageIndex { get; init; }
        public int? PageSize { get; init; }
        public int? CuamCid { get; init; }
        public DateTime? OrderDateStart { get; init; }
        public DateTime? OrderDateEnd { get; init; }
        public string? CoomNo { get; init; }
        public string? CoomName { get; init; }
        public string? EsmmShipNo { get; init; }
        public string? CoodName { get; init; }
        public string? CoocNo { get; init; }
        public OrderState? OrderState { get; init; }
        public OrderSort[]? OrderSorts { get; init; }
        public bool? IsQaList { get; init; }
        public string? DeliverMethodSearchKind { get; init; }
        public string? OrdChannelKindSearchKind { get; init; }
        public string? TempTypeSearchKind { get; init; }
    }
}
