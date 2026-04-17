using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class OrderInfoQueryModel
    {
        /// <summary>
        /// 訂購人會員編號
        /// </summary>
        public int? CoocMemSid { get; set; } = null;

        /// <summary>
        /// 商品名稱
        /// </summary>
        public string? CoodName { get; set; } = null;

        /// <summary>
        /// 購物車編號
        /// </summary>
        public string? CoocNo { get; set; } = null;

        /// <summary>
        /// 訂單編號
        /// </summary>
        public string? CoomNo { get; set; } = null;

        /// <summary>
        /// 賣場名稱
        /// </summary>
        public string? CoomName { get; set; } = null;

        /// <summary>
        /// 配送方式
        /// </summary>
        public string? DeliverMethodSearchKind { get; set; } = null;

        /// <summary>
        /// 配送編號
        /// </summary>
        public string? EsmmShipNo { get; set; } = null;

        /// <summary>
        /// 訂單結束日期
        /// </summary>
        public DateTime? OrderDateEnd { get; set; } = null;

        /// <summary>
        /// 訂單開始日期
        /// </summary>
        public DateTime? OrderDateStart { get; set; } = null;

        /// <summary>
        /// 訂單狀態
        /// </summary>
        public OrderState? OrderState { get; set; } = null;

        /// <summary>
        /// 訂購單所屬賣家會員編號
        /// </summary>
        public int? CoomCuamCid { get; set; } = null;

        /// <summary>
        /// 溫層代碼
        /// </summary>
        public string? TempTypeSearchKind { get; set; } = null;

        /// <summary>
        /// 賣場類型
        /// </summary>
        public string? OrdChannelKindSearchKind { get; set; } = null;

        /// <summary>
        /// 訂購人會員編號 或 訂購單所屬賣家會員編號
        /// </summary>
        public string? CoomCuamCidOrCoocMemSid { get; set; } = null;

        /// <summary>
        /// 是否只顯示有問答的資料
        /// </summary>
        public bool? IsQaList { get; set; } = null;

        /// <summary>
        /// 社群會員編號資料
        /// </summary>
        public int[]? BindMembersArray { get; set; } = null;
    }
}
