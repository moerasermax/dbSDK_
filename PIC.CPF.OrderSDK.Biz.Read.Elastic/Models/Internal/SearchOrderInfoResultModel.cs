namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class SearchOrderInfoResultModel
    {
        /// <summary>
        /// 訂單資訊
        /// </summary>
        public OrderDocument[] Documents { get; set; } = null!;

        /// <summary>
        /// 符合條件的訂單數量
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// 查詢時間
        /// </summary>
        public long Took { get; set; }
    }
}
