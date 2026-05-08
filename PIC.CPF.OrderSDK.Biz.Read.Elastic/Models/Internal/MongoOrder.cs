using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    // Dual Engine 流程的 Mongo 端 Internal Model — 對齊客戶原 SDK 的 OrderData 結構
    // 路徑：ES 取 CoomNo[] → MongoSearchDal.SearchByDDB(keyList) → IEnumerable<MongoOrder>
    // → ConvertToOrderData → SearchOrderInfoDataModel
    [BsonIgnoreExtraElements]
    public class MongoOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("coom_no")] public string? CoomNo { get; set; }
        [BsonElement("cooc_no")] public string? CoocNo { get; set; }

        [BsonElement("c_order_m")] public COrderM? COrderM { get; set; }
        [BsonElement("c_order_c")] public COrderC? COrderC { get; set; }
        [BsonElement("c_order_d")] public List<COrderD>? COrderD { get; set; }
        [BsonElement("c_goods_item")] public CGoodsItem? CGoodsItem { get; set; }
        [BsonElement("e_shipment_m")] public EShipmentM? EShipmentM { get; set; }
        [BsonElement("c_question_m")] public CQuestionM? CQuestionM { get; set; }
        [BsonElement("c_cancel_m")] public CCancelM? CCancelM { get; set; }
        [BsonElement("e_shipment_l")] public List<EShipmentL>? EShipmentL { get; set; }
        [BsonElement("e_shipment_s")] public List<EShipmentS>? EShipmentS { get; set; }
        [BsonElement("e_ccdhl")] public ECCDHL? ECCDHL { get; set; }
        [BsonElement("e_cccs")] public ECCCS? ECCCS { get; set; }
        [BsonElement("e_rtn_dhl_apply")] public ERtnDHLApply? ERtnDHLApply { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class COrderM
    {
        [BsonElement("coom_order_date")]public DateTime? CoomOrderDate { get; set; }
        [BsonElement("coom_name")] public string? CoomName { get; set; }
        [BsonElement("coom_status")] public string? CoomStatus { get; set; }
        [BsonElement("coom_temp_type")] public string? CoomTempType { get; set; }
        [BsonElement("coom_create_datetime")]public DateTime? CoomCreateDatetime { get; set; }
        [BsonElement("coom_cuam_cid")] public int? CoomCuamCid { get; set; }
        [BsonElement("coom_rechoice_flag")] public bool? CoomRechoiceFlag { get; set; }
        [BsonElement("coom_merge_list_coom_no")] public string? CoomMergeListCoomNo { get; set; }
        [BsonElement("coom_seller_memo")] public string? CoomSellerMemo { get; set; }
        [BsonElement("coom_seller_goods_total_amt")] public int? CoomSellerGoodsTotalAmt { get; set; }
        [BsonElement("coom_goods_item_num")] public int? CoomGoodsItemNum { get; set; }
        [BsonElement("coom_goods_total_num")] public int? CoomGoodsTotalNum { get; set; }
        [BsonElement("coom_rcv_total_amt")] public int? CoomRcvTotalAmt { get; set; }
        [BsonElement("coom_cgdm_id")] public string? CoomCgdmId { get; set; }
        [BsonElement("coom_ship_print_flag")] public bool? CoomShipPrintFlag { get; set; }
        [BsonElement("coom_cccm_no")] public string? CoomCccmNo { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class COrderC
    {
        [BsonElement("cooc_payment_type")] public string? CoocPaymentType { get; set; }
        [BsonElement("cooc_payment_pay_datetime")]public DateTime? CoocPaymentPayDatetime { get; set; }
        [BsonElement("cooc_deliver_method")] public string? CoocDeliverMethod { get; set; }
        [BsonElement("cooc_ord_channel_kind")] public string? CoocOrdChannelKind { get; set; }
        [BsonElement("cooc_create_datetime")]public DateTime? CoocCreateDatetime { get; set; }
        [BsonElement("cooc_mem_sid")] public int? CoocMemSid { get; set; }
        [BsonElement("cooc_payment_code")] public string? CoocPaymentCode { get; set; }
        [BsonElement("cooc_ord_name_enc")] public string? CoocOrdNameEnc { get; set; }
        [BsonElement("cooc_rcv_name_enc")] public string? CoocRcvNameEnc { get; set; }
        [BsonElement("cooc_rcv_mobile_enc")] public string? CoocRcvMobileEnc { get; set; }
        [BsonElement("cooc_payment_trade_no")] public string? CoocPaymentTradeNo { get; set; }
        [BsonElement("cooc_payment_note")] public string? CoocPaymentNote { get; set; }
        [BsonElement("cooc_payment_bank_code")] public string? CoocPaymentBankCode { get; set; }
        [BsonElement("cooc_payment_dueday")]public DateTime? CoocPaymentDueday { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class COrderD
    {
        [BsonElement("cood_name")] public string? CoodName { get; set; }
        [BsonElement("cood_qty")] public int? CoodQty { get; set; }
        [BsonElement("cood_price")] public int? CoodPrice { get; set; }
        [BsonElement("cood_discount_price")] public int? CoodDiscountPrice { get; set; }
        [BsonElement("cood_receive_price")] public int? CoodReceivePrice { get; set; }
        [BsonElement("cood_image_path")] public string? CoodImagePath { get; set; }
    }

    // 測試資料 c_goods_item 全 null,欄位依 Public Model GoodsItemModel 反推
    [BsonIgnoreExtraElements]
    public class CGoodsItem
    {
        [BsonElement("cgdi_number_column1")] public string? CgdiNumberColumn1 { get; set; }
    }

    // 測試資料 e_shipment_m 全 null,欄位依 Public Model ShipmentMasterModel 反推
    [BsonIgnoreExtraElements]
    public class EShipmentM
    {
        [BsonElement("esmm_no")] public string? EsmmNo { get; set; }
        [BsonElement("esmm_ship_no")] public string? EsmmShipNo { get; set; }
        [BsonElement("esmm_status")] public string? EsmmStatus { get; set; }
        [BsonElement("esmm_ship_method")] public string? EsmmShipMethod { get; set; }
        [BsonElement("esmm_ship_no_auth_code")] public string? EsmmShipNoAuthCode { get; set; }
        [BsonElement("esmm_ship_no_a")] public string? EsmmShipNoA { get; set; }
        [BsonElement("esmm_leave_store_date_b")]public DateTime? EsmmLeaveStoreDateB { get; set; }
        [BsonElement("esmm_ibon_app_flag")] public bool? EsmmIbonAppFlag { get; set; }
        [BsonElement("esmm_odd_reason")] public string? EsmmOddReason { get; set; }
        [BsonElement("esmm_confirm_extpay_datetime")]public DateTime? EsmmConfirmExtpayDatetime { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CQuestionM
    {
        [BsonElement("seller_qa_never_reply_count")] public int? SellerQaNeverReplyCount { get; set; }
        [BsonElement("buyer_qa_never_reply_count")] public int? BuyerQaNeverReplyCount { get; set; }
    }

    // 測試資料 c_cancel_m 全 null,欄位依 Public Model CancelMasterModel 反推
    [BsonIgnoreExtraElements]
    public class CCancelM
    {
        [BsonElement("cccm_status")] public string? CccmStatus { get; set; }
        [BsonElement("cccm_cancel_people")] public string? CccmCancelPeople { get; set; }
        [BsonElement("cccm_confirm_datetime")]public DateTime? CccmConfirmDatetime { get; set; }
        [BsonElement("cccm_create_datetime")]public DateTime? CccmCreateDatetime { get; set; }
        [BsonElement("cccm_refund_flag")] public bool? CccmRefundFlag { get; set; }
        [BsonElement("cccm_erfm_no")] public string? CccmErfmNo { get; set; }
    }

    // 測試資料 e_shipment_l 全 null,schema 未知,留空 placeholder + IgnoreExtraElements
    [BsonIgnoreExtraElements]
    public class EShipmentL
    {
    }

    // 測試資料 e_shipment_s 全 null,schema 未知,留空 placeholder + IgnoreExtraElements
    [BsonIgnoreExtraElements]
    public class EShipmentS
    {
    }

    [BsonIgnoreExtraElements]
    public class ECCDHL
    {
        [BsonElement("ecdh_esmm_no")] public string? EcdhEsmmNo { get; set; }
        [BsonElement("ecdh_process_code")] public string? EcdhProcessCode { get; set; }
    }

    // 測試資料 e_cccs 全 null,欄位依 Public Model CCCSModel 反推
    [BsonIgnoreExtraElements]
    public class ECCCS
    {
        [BsonElement("eccs_id")] public string? EccsId { get; set; }
        [BsonElement("eccs_store_type")] public string? EccsStoreType { get; set; }
        [BsonElement("eccs_rechoice_store_status")] public string? EccsRechoiceStoreStatus { get; set; }
        [BsonElement("eccs_create_datetime")]public DateTime? EccsCreateDatetime { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ERtnDHLApply
    {
        [BsonElement("erda_apply_status")] public string? ErdaApplyStatus { get; set; }
    }
}
