using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// 對外 API 回傳之標準信封結構，對齊客戶 GoldenRecipe Out 形狀
    /// （data / code / message / errorMsg / total）。
    /// </summary>
    /// <typeparam name="T">data 節點之業務型別</typeparam>
    public class ApiResponseWrapper<T>
    {
        /// <summary>
        /// 業務資料節點
        /// </summary>
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        /// <summary>
        /// 回傳碼，"00" 為成功
        /// </summary>
        [JsonPropertyName("code")]
        public string Code { get; set; } = "00";

        /// <summary>
        /// 回傳訊息，預設「成功」
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = "成功";

        /// <summary>
        /// 錯誤訊息，成功時為空字串
        /// </summary>
        [JsonPropertyName("errorMsg")]
        public string ErrorMsg { get; set; } = string.Empty;

        /// <summary>
        /// Response 筆數標記（金標固定為 1，表本次 response 為單一 envelope）
        /// </summary>
        [JsonPropertyName("total")]
        public long Total { get; set; } = 1;
    }
}
