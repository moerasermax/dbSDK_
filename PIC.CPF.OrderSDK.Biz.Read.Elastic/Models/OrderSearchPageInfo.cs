namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// 分頁資訊 (Search 2/3 共用)
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
