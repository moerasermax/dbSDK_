namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum
{
    /// <summary>
    /// 訂單狀態
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// 待處理
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 處理中
        /// </summary>
        Processing = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// 已退款
        /// </summary>
        Refunded = 4
    }
}
