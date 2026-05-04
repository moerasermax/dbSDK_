using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// 統一入參：覆蓋所有 7 個 Search 方法的輸入欄位集合。
    /// 各方法只讀取與自身業務相關的欄位，其餘欄位保持 null/預設值。
    /// </summary>
    public class OrderSearchRequest
    {
        // ── 共通識別欄位 ──────────────────────────────────────

        /// <summary>
        /// 賣家會員編號 (Search 1, 2, 4, 5, 6, 7)
        /// </summary>
        public int? CuamCid { get; init; }

        /// <summary>
        /// 買家會員編號 (Search 3)
        /// </summary>
        public int? MemSid { get; init; }

        // ── 主日期區間 ─────────────────────────────────────────

        /// <summary>
        /// 查詢起始日期 (Search 2, 3, 4, 5, 6, 7)
        /// </summary>
        public DateTime? SearchStartDate { get; init; }

        /// <summary>
        /// 查詢結束日期 (Search 2, 3, 4, 5, 6, 7)
        /// </summary>
        public DateTime? SearchEndDate { get; init; }

        // ── PoP 環比日期 (Search 5, 6) ────────────────────────

        /// <summary>
        /// 環比查詢起始日期
        /// </summary>
        public DateTime? DateStartPoP { get; init; }

        /// <summary>
        /// 環比查詢結束日期
        /// </summary>
        public DateTime? DateEndPoP { get; init; }

        /// <summary>
        /// 日期區間類型 (本日 / 本週 / 本月 / 過去30天 / 按週 / 按月)
        /// </summary>
        public DateRangeType? DateRangeType { get; init; }

        // ── 分頁與排序 (Search 2, 3) ──────────────────────────

        /// <summary>
        /// 分頁資訊
        /// </summary>
        public OrderSearchPageInfo? PageInfo { get; init; }

        /// <summary>
        /// 排序條件
        /// </summary>
        public OrderSort[]? Sorts { get; init; }

        // ── 搜尋篩選條件 (Search 2, 3) ───────────────────────

        /// <summary>
        /// 訂購單編號 (完整 15 碼或部分關鍵字)
        /// </summary>
        public string? CoomNo { get; init; }

        /// <summary>
        /// 賣場名稱 (關鍵字)
        /// </summary>
        public string? CoomName { get; init; }

        /// <summary>
        /// 配送單編號 (關鍵字)
        /// </summary>
        public string? EsmmShipNo { get; init; }

        /// <summary>
        /// 商品名稱 (關鍵字)
        /// </summary>
        public string? CoodName { get; init; }

        /// <summary>
        /// 購物車編號
        /// </summary>
        public string? CoocNo { get; init; }

        /// <summary>
        /// 訂單狀態
        /// </summary>
        public OrderState? OrderState { get; init; }

        /// <summary>
        /// 配送方式代碼
        /// </summary>
        public string? DeliverMethodSearchKind { get; init; }

        /// <summary>
        /// 賣場類型代碼
        /// </summary>
        public string? OrdChannelKindSearchKind { get; init; }

        /// <summary>
        /// 溫層代碼
        /// </summary>
        public string? TempTypeSearchKind { get; init; }

        /// <summary>
        /// 只顯示有問答記錄的訂單
        /// </summary>
        public bool? IsQaList { get; init; }

        /// <summary>
        /// 社群帳號綁定之其他會員編號陣列
        /// </summary>
        public int[]? BindMembersArray { get; init; }
    }

    /// <summary>
    /// 分頁資訊
    /// </summary>
    public class OrderSearchPageInfo
    {
        /// <summary>
        /// 分頁索引 (從 0 開始)
        /// </summary>
        public int? PageIndex { get; init; }

        /// <summary>
        /// 每頁筆數
        /// </summary>
        public int? PageSize { get; init; }
    }
}
