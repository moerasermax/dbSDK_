using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    /// <summary>
    /// Search 7 GetUserCgdmData 回傳結果（對應 data 節點）
    /// </summary>
    public class UserCgdmDataResultModel
    {
        /// <summary>
        /// 賣家會員編號
        /// </summary>
        [JsonPropertyName("cuamCid")]
        public int CuamCid { get; set; }

        /// <summary>
        /// 賣場商品明細資料清單
        /// </summary>
        [JsonPropertyName("cgdm")]
        public CgdmDataModel[]? Cgdm { get; set; }
    }

    /// <summary>
    /// 單筆 cgdm 資料
    /// </summary>
    public class CgdmDataModel
    {
        /// <summary>
        /// 賣場商品明細編號
        /// </summary>
        [JsonPropertyName("cgdmId")]
        public string CgdmId { get; set; } = string.Empty;

        /// <summary>
        /// 賣場商品明細最後更新時間（ISO 8601 字串）
        /// </summary>
        [JsonPropertyName("cgdmUpdateDatetime")]
        public string CgdmUpdateDatetime { get; set; } = string.Empty;
    }
}
