namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum
{
    /// <summary>
    /// 訂單排序
    /// </summary>
    public enum OrderSort
    {
        /// <summary>
        /// 訂購單建立時間 (由舊到新)
        /// </summary>
        CoomCreateDatetimeAsc,

        /// <summary>
        /// 訂購單建立時間 (由新到舊)
        /// </summary>
        CoomCreateDatetimeDesc,

        /// <summary>
        /// 訂購單編號 (由小到大)
        /// </summary>
        CoomNoAsc,

        /// <summary>
        /// 訂購單編號 (由大到小)
        /// </summary>
        CoomNoDesc,

        /// <summary>
        /// 賣家未回覆問答數量 (由小到大)
        /// </summary>
        SellerQaNeverReplyCountAsc,

        /// <summary>
        /// 賣家未回覆問答數量 (由大到小)
        /// </summary>
        SellerQaNeverReplyCountDesc,

        /// <summary>
        /// 買家未回覆問答數量 (由小到大)
        /// </summary>
        BuyerQaNeverReplyCountAsc,

        /// <summary>
        /// 買家未回覆問答數量 (由大到小)
        /// </summary>
        BuyerQaNeverReplyCountDesc,
    }
}
