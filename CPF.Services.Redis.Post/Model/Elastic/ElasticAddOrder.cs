using NO3._dbSDK_Imporve.Core.Entity;
using System.Text.Json.Serialization;

namespace CPF.Services.Redis.Post.Model.Elastic
{
    public class ElasticAddOrder : Query
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; } = null!;

        [JsonPropertyName("args")]
        public OrderArgs? Args { get; set; } = null!;
    }

    public class OrderArgs
    {
        // --- 第一層欄位 ---
        [JsonPropertyName("coom_no")]
        public string? CoomNo { get; set; }

        [JsonPropertyName("coom_name")]
        public string? CoomName { get; set; }

        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; }

        [JsonPropertyName("coom_temp_type")]
        public string? CoomTempType { get; set; }

        [JsonPropertyName("coom_create_datetime")]
        public DateTime? CoomCreateDatetime { get; set; } // 修正：依圖片改為 Date 型別

        [JsonPropertyName("coom_cuam_cid")]
        public int? CoomCuamCid { get; set; }        // 修正：依圖片改為 Keyword (string)

        [JsonPropertyName("coom_rcv_totalamt")]
        public int? CoomRcvTotalAmt { get; set; }

        [JsonPropertyName("cooc_no")]
        public string? CoocNo { get; set; }

        [JsonPropertyName("cooc_payment_type")]
        public string? CoocPaymentType { get; set; }

        [JsonPropertyName("cooc_payment_pay_datetime")]
        public DateTime? CoocPaymentPayDatetime { get; set; }

        [JsonPropertyName("cooc_deliver_method")]
        public string? CoocDeliverMethod { get; set; }

        [JsonPropertyName("cooc_ord_channel_kind")]
        public string? CoocOrdChannelKind { get; set; }

        [JsonPropertyName("cooc_mem_sid")]
        public int? CoocMemSid { get; set; }         // 修正：依圖片改為 Keyword (string)

        // --- 補足圖片中遺漏的出貨與狀態欄位 (esmm / esml 系列) ---
        [JsonPropertyName("esmm_ship_no")]
        public string? EsmmShipNo { get; set; }

        [JsonPropertyName("esmm_status")]
        public string? EsmmStatus { get; set; }

        [JsonPropertyName("esmm_rcv_totalamt")]
        public int? EsmmRcvTotalAmt { get; set; }

        [JsonPropertyName("esmm_leavestoredate_b")]
        public DateTime? EsmmLeavestoredateB { get; set; }

        [JsonPropertyName("esml_status_shipping_datetime")]
        public DateTime? EsmlStatusShippingDatetime { get; set; }

        [JsonPropertyName("esml_status_finish_datetime")]
        public DateTime? EsmlStatusFinishDatetime { get; set; }

        // --- 補足布林值欄位 (Boolean) ---
        [JsonPropertyName("esms_dlvstatus_seller_pickup")]
        public bool? EsmsDlvstatusSellerPickup { get; set; }

        [JsonPropertyName("esms_sc_status_returning")]
        public bool? EsmsScStatusReturning { get; set; }

        [JsonPropertyName("is_return_shipping")]
        public bool? IsReturnShipping { get; set; }

        [JsonPropertyName("crsa_applied")]
        public bool? CrsaApplied { get; set; }

        // --- 補足統計欄位 (Integer) ---
        [JsonPropertyName("seller_qa_never_reply_count")]
        public int? SellerQaNeverReplyCount { get; set; }

        [JsonPropertyName("buyer_qa_never_reply_count")]
        public int? BuyerQaNeverReplyCount { get; set; }

        // --- 商品列表項目 ---
        [JsonPropertyName("cood_items")]
        public List<CoodItem>? CoodItems { get; set; } = new();

        // 額外保留：你原有的清單 (視需求決定是否移除)
        public List<string>? CoodNames { get; set; }
    }

    public class CoodItem
    {
        [JsonPropertyName("cgdd_cgdmid")]
        public string? CgddCgdmid { get; set; }

        [JsonPropertyName("cgdd_id")]
        public string? CgddId { get; set; }

        [JsonPropertyName("cood_cgdsid")]
        public string? CoodCgdsId { get; set; }

        [JsonPropertyName("cood_name")]
        public string? CoodName { get; set; }

        [JsonPropertyName("cood_qty")]
        public int? CoodQty { get; set; }

        [JsonPropertyName("cood_image_path")]
        public string? CoodImagePath { get; set; }
    }
}
