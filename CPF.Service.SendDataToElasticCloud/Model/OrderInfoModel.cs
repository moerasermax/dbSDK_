using System.Text.Json.Serialization; 
using Elastic.Clients.Elasticsearch;
using NO3._dbSDK_Imporve.Core.Entity;
namespace CPF.Service.SendDataToElasticCloud.Model
{
    public class OrderInfoModel : OrderSummary
    {
        public string Id => CoomNo;
        /// <summary>
        /// 訂購單編號
        /// </summary>
        [JsonPropertyName("coom_no")]
        public string? CoomNo { get; set; } = string.Empty;

        /// <summary>
        /// 賣場名稱
        /// </summary>
        [JsonPropertyName("coom_name")]
        public string? CoomName { get; set; } = null;

        /// <summary>
        /// 訂購單狀態
        /// - 00: 訂單異常
        /// - 10: 訂單成立
        /// - 11: 取消訂單
        /// - 12: 已合併
        /// - 1X: 已合併取消
        /// - 20: 備貨
        /// - 30: 配送
        /// </summary>
        [JsonPropertyName("coom_status")]
        public string? CoomStatus { get; set; } = null;

        /// <summary>
        /// 溫層代碼
        /// - 01: 常溫
        /// - 02: 冷藏
        /// - 03: 冷凍
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
        /// - 0: 會員帳戶
        /// - 1: 取貨付款
        /// - 2: ATM付款(自填帳號)【2017/2/15移除】
        /// - 3: ATM付款
        /// - 4: 信用卡
        /// - 5: 無需付款
        /// - 6: icash Pay
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
        /// - 1: 店到店
        /// - 2: 宅到宅
        /// - 3: 店到宅
        /// </summary>
        [JsonPropertyName("cooc_deliver_method")]
        public string? CoocDeliverMethod { get; set; } = null;

        /// <summary>
        /// 購物車收單類型
        /// - 1: 一般賣場
        /// - 2: 快速結帳
        /// - 3: 便利自填單
        /// - 4: 訂單匯入
        /// - 5: 租賃服務(保留)
        /// - 6: 訂單合併(2020/1/2增加)
        /// - 7: 社群賣場(2021/5/21增加)
        /// - 8: 社群匯入賣場(2022/6/22增加)
        /// </summary>
        [JsonPropertyName("cooc_ord_channel_kind")]
        public string? CoocOrdChannelKind { get; set; } = null;

        /// <summary>
        /// 訂購人會員編號
        /// </summary>
        [JsonPropertyName("cooc_mem_sid")]
        public int? CoocMemSid { get; set; } = null;

        /// <summary>
        /// 訂購子單商品名稱 (即商品名稱 - [規格])
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
        /// - 00: 處理中(for B2C)
        /// - 01: 待寄件
        /// - 05: 取消出貨
        /// - 10: 已寄件
        /// - 11: 寄件異常
        /// - 20: 已送達
        /// - 30: 已取件
        /// - 31: 逾期未取件
        /// - 32: 宅配退件
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
        public DateTime? EsmlStatusShippingDatetime { get; set; } = null;

        /// <summary>
        /// 已取件時間
        /// </summary>
        [JsonPropertyName("esml_status_finish_datetime")]
        public DateTime? EsmlStatusFinishDateTime { get; set; } = null;

        /// <summary>
        /// 商品總額總計 (ESAPP需求)
        /// </summary>
        [JsonPropertyName("esmm_rcv_total_amt")]
        public int? EsmmRcvTotalAmt { get; set; } = null;

        /// <summary>
        /// 店到店賣家取件註記
        /// </summary>
        [JsonPropertyName("esms_dlvstatus_seller_pickup")]
        public bool? EsmsDlvstatusSellerPickup { get; set; } = null;

        /// <summary>
        /// 店到宅退回註記
        /// </summary>
        [JsonPropertyName("esms_sc_status_returning")]
        public bool? EsmsSCStatusReturning { get; set; } = null;

        /// <summary>
        /// 20250401新增
        /// 退貨申請成功後為TRUE
        /// </summary>
        [JsonPropertyName("crsa_applied")]
        public bool? CrsaApplied { get; set; } = null;

        /// <summary>
        /// 20250423新增
        /// 訂單總金額(ESAPP需求)
        /// </summary>
        [JsonPropertyName("coom_rcv_totalamt")]
        public int? CoomRcvTotalAmt { get; set; } = null;

        /// <summary>
        /// 20250423新增
        /// 訂單總金額(ESAPP需求)
        /// </summary>
        [JsonPropertyName("esmm_leave_store_date_b")]
        public DateTime? EsmmLeaveStoreDateB { get; set; } = null;

        /// <summary>
        /// 20250523新增
        /// 商品前五名所需資料(ESAPP需求)
        /// </summary>
        [JsonPropertyName("cood_items")]
        public CoodItems[]? CoodItems { get; set; } = null;
    }

    public class CoodItems
    {
        /// <summary>
        /// 商品賣場編號
        /// </summary>
        [JsonPropertyName("cgdd_cgdmid")]
        public string? CgddCgdmid { get; set; } = null;

        /// <summary>
        /// 商品編號
        /// </summary>
        [JsonPropertyName("cgdd_id")]
        public string? CgddId { get; set; } = null;

        /// <summary>
        /// 商品規格編號
        /// </summary>
        [JsonPropertyName("cood_cgdsid")]
        public string? CoodCgdsId { get; set; } = null;

        /// <summary>
        /// 商品名稱(同外層CoodNames，只是這裡是單筆)
        /// </summary>
        [JsonPropertyName("cood_name")]
        public string? CoodName { get; set; } = null;

        /// <summary>
        /// 購買數量
        /// </summary>
        [JsonPropertyName("cood_qty")]
        public int? CoodQty { get; set; } = null;

        /// <summary>
        /// 規格圖檔路徑
        /// </summary>
        [JsonPropertyName("cood_image_path")]
        public string? CoodImagePath { get; set; } = null;
    }

}
