namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    /// <summary>
    /// Search 7 Terms + Reverse Nested + Max 聚合結果的內部承載模型，
    /// 每個實例對應一個去重後的 cgdmId 及其最大 _ord_modify_date。
    /// </summary>
    public class UserCgdmDataAggregateModel
    {
        /// <summary>
        /// 賣場商品明細編號（來自 Terms bucket key）
        /// </summary>
        public string CgdmId { get; set; } = string.Empty;

        /// <summary>
        /// 最大訂單更新時間（由 max_modify_date 聚合的 epoch_millis 轉換而來）
        /// </summary>
        public DateTime? MaxModifyDate { get; set; }
    }
}
