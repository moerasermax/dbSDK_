using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    /// <summary>
    /// 商品前五名所需資料 (Elasticsearch V9 相容版)
    /// </summary>
    public class CoodItems
    {
        /// <summary>
        /// 商品賣場編號
        /// </summary>
        [JsonPropertyName("cgdd_cgdmid")]
        public string? CgddCgdmid { get; set; }

        /// <summary>
        /// 商品編號
        /// </summary>
        [JsonPropertyName("cgdd_id")]
        public string? CgddId { get; set; }

        /// <summary>
        /// 商品規格編號
        /// </summary>
        [JsonPropertyName("cood_cgdsid")]
        public string? CoodCgdsId { get; set; }

        /// <summary>
        /// 商品名稱
        /// </summary>
        /// <remarks>
        /// 注意：若需全文檢索請改用 [Text]，若僅需精確匹配或聚合則維持 [Keyword]
        /// </remarks>
        [JsonPropertyName("cood_name")]
        public string? CoodName { get; set; }

        /// <summary>
        /// 購買數量
        /// </summary>
        [JsonPropertyName("cood_qty")]
        public int? CoodQty { get; set; }

        /// <summary>
        /// 規格圖檔路徑
        /// </summary>
        [JsonPropertyName("cood_image_path")]
        public string? CoodImagePath { get; set; }
    }
}