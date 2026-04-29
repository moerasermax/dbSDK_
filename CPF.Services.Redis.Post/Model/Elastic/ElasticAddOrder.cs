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

        // --- 嵌套物件：coom (訂單主檔) ---
        [JsonPropertyName("coom")]
        public CoomArgs? Coom { get; set; }

        // --- 嵌套物件：esmm (物流主檔) ---
        [JsonPropertyName("esmm")]
        public EsmmArgs? Esmm { get; set; }

        // --- 嵌套物件：esml (貨態歷程) ---
        [JsonPropertyName("esml")]
        public List<EsmlArgs>? Esml { get; set; }

        // --- 嵌套物件：esms (物流狀態) ---
        [JsonPropertyName("esms")]
        public List<EsmsArgs>? Esms { get; set; }

        // --- 向下兼容：第一層欄位 (用於 AddOrderEvent) ---
        [JsonPropertyName("coom_name")]
        public string? CoomName { get; set; }

        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; }

        [JsonPropertyName("coom_temp_type")]
        public string? CoomTempType { get; set; }

        [JsonPropertyName("coom_create_datetime")]
        public DateTime? CoomCreateDatetime { get; set; }

        [JsonPropertyName("coom_cuam_cid")]
        public int? CoomCuamCid { get; set; }

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
        public int? CoocMemSid { get; set; }

        // --- 向下兼容：第一層欄位 (用於 AddOrderEvent) ---
        [JsonPropertyName("esmm_ship_no")]
        public string? EsmmShipNo { get; set; }

        [JsonPropertyName("esmm_ship_no_auth_code")]
        public string? EsmmShipNoAuthCode { get; set; }

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

        // --- 布林值欄位 ---
        [JsonPropertyName("esms_dlvstatus_seller_pickup")]
        public bool? EsmsDlvstatusSellerPickup { get; set; }

        [JsonPropertyName("esms_sc_status_returning")]
        public bool? EsmsScStatusReturning { get; set; }

        [JsonPropertyName("is_return_shipping")]
        public bool? IsReturnShipping { get; set; }

        [JsonPropertyName("crsa_applied")]
        public bool? CrsaApplied { get; set; }

        // --- 統計欄位 ---
        [JsonPropertyName("seller_qa_never_reply_count")]
        public int? SellerQaNeverReplyCount { get; set; }

        [JsonPropertyName("buyer_qa_never_reply_count")]
        public int? BuyerQaNeverReplyCount { get; set; }

        // --- 商品列表項目 ---
        [JsonPropertyName("cood_items")]
        public List<CoodItem>? CoodItems { get; set; } = new();

        public List<string>? CoodNames { get; set; }
    }

    /// <summary>
    /// 訂單主檔嵌套物件 (coom)
    /// </summary>
    public class CoomArgs
    {
        [JsonPropertyName("coom_name")]
        public string? CoomName { get; set; }

        [JsonPropertyName("coom_order_date")]
        public DateTime? CoomOrderDate { get; set; }

        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; }

        [JsonPropertyName("coom_ccc m_no")]
        public string? CoomCccmNo { get; set; }

        [JsonPropertyName("coom_temp_type")]
        public string? CoomTempType { get; set; }

        [JsonPropertyName("coom_create_datetime")]
        public DateTime? CoomCreateDatetime { get; set; }

        [JsonPropertyName("coom_cuam_cid")]
        public int? CoomCuamCid { get; set; }

        [JsonPropertyName("coom_seller_goods_total_amt")]
        public int? CoomSellerGoodsTotalAmt { get; set; }

        [JsonPropertyName("coom_goods_item_num")]
        public int? CoomGoodsItemNum { get; set; }

        [JsonPropertyName("coom_goods_total_num")]
        public int? CoomGoodsTotalNum { get; set; }

        [JsonPropertyName("coom_rcv_totalamt")]
        public int? CoomRcvTotalAmt { get; set; }

        [JsonPropertyName("coom_cgdm_id")]
        public string? CoomCgdmId { get; set; }

        [JsonPropertyName("coom_seller_memo")]
        public string? CoomSellerMemo { get; set; }

        [JsonPropertyName("coom_re_choice_flag")]
        public string? CoomReChoiceFlag { get; set; }

        [JsonPropertyName("coom_merge_list_coom_no")]
        public string? CoomMergeListCoomNo { get; set; }

        [JsonPropertyName("coom_ship_print_flag")]
        public string? CoomShipPrintFlag { get; set; }
    }

    /// <summary>
    /// 物流主檔嵌套物件 (esmm)
    /// </summary>
    public class EsmmArgs
    {
        [JsonPropertyName("esmm_no")]
        public string? EsmmNo { get; set; }

        [JsonPropertyName("esmm_ship_no")]
        public string? EsmmShipNo { get; set; }

        [JsonPropertyName("esmm_status")]
        public string? EsmmStatus { get; set; }

        [JsonPropertyName("esmm_ship_method")]
        public string? EsmmShipMethod { get; set; }

        [JsonPropertyName("esmm_ship_no_auth_code")]
        public string? EsmmShipNoAuthCode { get; set; }

        [JsonPropertyName("esmm_ship_no_a")]
        public string? EsmmShipNoA { get; set; }

        [JsonPropertyName("esmm_leave_store_date_b")]
        public DateTime? EsmmLeaveStoreDateB { get; set; }

        [JsonPropertyName("esmm_ibon_app_flag")]
        public string? EsmmIbonAppFlag { get; set; }

        [JsonPropertyName("esmm_odd_reason")]
        public string? EsmmOddReason { get; set; }

        [JsonPropertyName("esmm_confirm_ext_pay_datetime")]
        public DateTime? EsmmConfirmExtPayDatetime { get; set; }
    }

    /// <summary>
    /// 貨態歷程嵌套物件 (esml)
    /// </summary>
    public class EsmlArgs
    {
        [JsonPropertyName("esml_esmm_status")]
        public string? EsmlEsmmStatus { get; set; }

        [JsonPropertyName("esml_status_datetime")]
        public DateTime? EsmlStatusDatetime { get; set; }
    }

    /// <summary>
    /// 物流狀態嵌套物件 (esms)
    /// </summary>
    public class EsmsArgs
    {
        [JsonPropertyName("esms_dlv_status_no")]
        public string? EsmsDlvStatusNo { get; set; }

        [JsonPropertyName("esms_status_datetime")]
        public DateTime? EsmsStatusDatetime { get; set; }
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
