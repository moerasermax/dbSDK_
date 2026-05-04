using Elastic.Clients.Elasticsearch;
// 必須加上這個，Attributes 才會生效
using Elastic.Clients.Elasticsearch.Mapping;
using System.Text.Json.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    /// <summary>
    /// 訂單文檔 - 適用於 Elasticsearch 9.3.3 (Elastic.Clients.Elasticsearch)
    /// </summary>
    public class OrderDocument
    {
        [JsonIgnore]
        public string Id => CoomNo ?? string.Empty;

        /// <summary>
        /// 訂購單編號
        /// </summary>
        [JsonPropertyName("coom_no")]
        public string CoomNo { get; set; } = string.Empty;

        /// <summary>
        /// 賣場名稱
        /// </summary>
        [JsonPropertyName("coom_name")]
        public string? CoomName { get; set; } = null;

        /// <summary>
        /// 訂購單狀態
        /// </summary>
        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; } = null;

        /// <summary>
        /// 溫層代碼
        /// </summary>
        [JsonPropertyName("coom_temp_type")]
        public string? CoomTempType { get; set; } = null;

        /// <summary>
        /// 訂購單建立時間
        /// </summary>
        [JsonPropertyName("coom_create_datetime")]
        public DateTime? CoomCreateDatetime { get; set; } = null;

        /// <summary>
        /// 訂購單所屬賣家會員編號
        /// </summary>
        [JsonPropertyName("coom_cuam_cid")]
        public int? CoomCuamCid { get; set; } = null;

        /// <summary>
        /// 購物車編號
        /// </summary>
        [JsonPropertyName("cooc_no")]
        public string? CoocNo { get; set; } = null;

        /// <summary>
        /// 付款方式
        /// </summary>
        [JsonPropertyName("cooc_payment_type")]
        public string? CoocPaymentType { get; set; } = null;

        /// <summary>
        /// 付款銷帳完成時間
        /// </summary>
        [JsonPropertyName("cooc_payment_pay_datetime")]
        public DateTime? CoocPaymentPayDatetime { get; set; } = null;

        /// <summary>
        /// 運送方式
        /// </summary>
        [JsonPropertyName("cooc_deliver_method")]
        public string? CoocDeliverMethod { get; set; } = null;

        /// <summary>
        /// 購物車收單類型
        /// </summary>
        [JsonPropertyName("cooc_ord_channel_kind")]
        public string? CoocOrdChannelKind { get; set; } = null;

        /// <summary>
        /// 訂購人會員編號
        /// </summary>
        [JsonPropertyName("cooc_mem_sid")]
        public int? CoocMemSid { get; set; } = null;

        /// <summary>
        /// 訂購子單商品名稱
        /// </summary>
        [JsonPropertyName("cood_name")]
        public string[]? CoodNames { get; set; } = null;

        /// <summary>
        /// 配送單編號
        /// </summary>
        [JsonPropertyName("esmm_ship_no")]
        public string? EsmmShipNo { get; set; } = null;

        /// <summary>
        /// 配送狀態
        /// </summary>
        [JsonPropertyName("esmm_status")]
        public string? EsmmStatus { get; set; } = null;

        /// <summary>
        /// 宅配退貨
        /// </summary>
        [JsonPropertyName("is_return_shipping")]
        public bool? IsReturnShipping { get; set; } = null;

        /// <summary>
        /// 賣家未回覆數量
        /// </summary>
        [JsonPropertyName("seller_qa_never_reply_count")]
        public int? SellerQaNeverReplyCount { get; set; }

        /// <summary>
        /// 買家未回覆數量
        /// </summary>
        [JsonPropertyName("buyer_qa_never_reply_count")]
        public int? BuyerQaNeverReplyCount { get; set; }

        /// <summary>
        /// 已寄件時間
        /// </summary>
        [JsonPropertyName("esml_status_shipping_datetime")]
        public DateTime? EsmlStatusShippingDateTime { get; set; } = null;

        /// <summary>
        /// 已取件時間
        /// </summary>
        [JsonPropertyName("esml_status_finish_datetime")]
        public DateTime? EsmlStatusFinishDateTime { get; set; } = null;

        /// <summary>
        /// 商品總額總計
        /// </summary>
        [JsonPropertyName("esmm_rcv_total_amt")]
        public int? EsmmRcvTotalAmt { get; set; } = null;

        /// <summary>
        /// 店到店賣家取件註記
        /// </summary>
        [JsonPropertyName("esms_dlv_status_seller_pickup")]
        public bool? EsmsDlvstatusSellerPickup { get; set; } = null;

        /// <summary>
        /// 店到宅退回註記
        /// </summary>
        [JsonPropertyName("esms_sc_status_returning")]
        public bool? EsmsSCStatusReturning { get; set; } = null;

        /// <summary>
        /// 退貨申請成功標記
        /// </summary>
        [JsonPropertyName("crsa_applied")]
        public bool? CrsaApplied { get; set; } = null;

        /// <summary>
        /// 商品總額總計 (ESAPP需求)
        /// </summary>
        [JsonPropertyName("coom_rcv_totalamt")]
        public int? CoomRcvTotalAmt { get; set; } = null;

        /// <summary>
        /// 商品離店日
        /// </summary>
        [JsonPropertyName("esmm_leavestoredate_b")]
        public DateTime? EsmmLeaveStoreDateB { get; set; } = null;

        /// <summary>
        /// 訂單最後更新時間 (epoch_millis；Search 7 Reverse Nested Max 聚合使用)
        /// </summary>
        [JsonPropertyName("_ord_modify_date")]
        public long? OrdModifyDate { get; set; } = null;

        /// <summary>
        /// 商品明細 (Nested)
        /// </summary>
        [JsonPropertyName("cood_items")]
        public CoodItems[]? CoodItems { get; set; } = null;
    }
}