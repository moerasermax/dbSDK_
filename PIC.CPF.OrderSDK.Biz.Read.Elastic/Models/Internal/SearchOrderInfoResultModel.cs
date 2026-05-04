using System.Text.Json;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class SearchOrderInfoResultModel
    {
        public JsonElement[] Documents { get; set; } = [];

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
