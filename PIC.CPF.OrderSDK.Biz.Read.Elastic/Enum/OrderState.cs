namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum
{
    /// <summary>
    /// 訂單狀態（對應 OrderSearchDal 各 Query 方法）
    /// </summary>
    public enum OrderState
    {
        /// <summary>
        /// 未付款：coomStatus=10 且未完成付款
        /// </summary>
        Unpaid = 0,

        /// <summary>
        /// 處理中：coomStatus=10 且已付款（有付款時間或取貨付款）
        /// </summary>
        DealWith = 1,

        /// <summary>
        /// 待出貨（賣家視角）：esmmStatus=01 且已付款
        /// </summary>
        ToshipForSeller = 2,

        /// <summary>
        /// 待出貨（買家視角）：coomStatus in [10,20] 且已付款
        /// </summary>
        ToshipForBuyer = 3,

        /// <summary>
        /// 已寄件：esmmStatus=10
        /// </summary>
        Shipping = 4,

        /// <summary>
        /// 待取件（已送達）：esmmStatus=20
        /// </summary>
        ToFinish = 5,

        /// <summary>
        /// 已取件：esmmStatus=30
        /// </summary>
        Finish = 6,

        /// <summary>
        /// 退貨申請：crsaApplied=true
        /// </summary>
        RtnShipping = 7,

        /// <summary>
        /// 未取退宅：esmmStatus=31 OR isReturnShipping=true
        /// </summary>
        NoShowToDHL = 8,

        /// <summary>
        /// 異常訂單：esmmStatus in [11, 32]
        /// </summary>
        Exception = 9,

        /// <summary>
        /// 取消訂單：coomStatus in [11, 12, 1X]
        /// </summary>
        Cancel = 10,

        /// <summary>
        /// 賣家未回覆問答：sellerQaNeverReplyCount > 0
        /// </summary>
        SellerQaNeverReply = 11,

        /// <summary>
        /// 買家未回覆問答：buyerQaNeverReplyCount > 0
        /// </summary>
        BuyerQaNeverReply = 12,
    }
}
