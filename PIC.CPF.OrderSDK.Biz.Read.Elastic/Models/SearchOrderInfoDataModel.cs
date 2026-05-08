using System.Text.Json.Serialization;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Serialization;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SearchOrderInfoDataModel
    {
        [JsonPropertyName("c_Order_M")]
        public OrderMasterModel? COrderM { get; set; }

        [JsonPropertyName("c_Order_C")]
        public OrderCartModel? COrderC { get; set; }

        [JsonPropertyName("c_Order_D")]
        public OrderItemModel[]? COrderD { get; set; }

        [JsonPropertyName("c_Goods_Item")]
        public GoodsItemModel? CGoodsItem { get; set; }

        [JsonPropertyName("e_Shipment_M")]
        public ShipmentMasterModel? EShipmentM { get; set; }

        [JsonPropertyName("c_Question_M")]
        public QuestionMasterModel? CQuestionM { get; set; }

        [JsonPropertyName("c_Cancel_M")]
        public CancelMasterModel? CCancelM { get; set; }

        [JsonPropertyName("e_Shipment_L")]
        public object? EShipmentL { get; set; }

        [JsonPropertyName("e_Shipment_S")]
        public object? EShipmentS { get; set; }

        [JsonPropertyName("e_CCDHL")]
        public CCDHLModel? ECCDHL { get; set; }

        [JsonPropertyName("e_CCCS")]
        public CCCSModel? ECCCS { get; set; }

        [JsonPropertyName("e_RtnDHL_Apply")]
        public RtnDHLApplyModel? ERtnDHLApply { get; set; }
    }

    public class OrderMasterModel
    {
        [JsonPropertyName("coomNo")] public string? CoomNo { get; set; }
        [JsonPropertyName("coomOrderDate")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CoomOrderDate { get; set; }
        [JsonPropertyName("coomName")] public string? CoomName { get; set; }
        [JsonPropertyName("coomStatus")] public string? CoomStatus { get; set; }
        [JsonPropertyName("coomTempType")] public string? CoomTempType { get; set; }
        [JsonPropertyName("coomCreateDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CoomCreateDatetime { get; set; }
        [JsonPropertyName("coomCuamCid")] public int? CoomCuamCid { get; set; }
        [JsonPropertyName("coomReChoiceFlag")] public bool? CoomReChoiceFlag { get; set; }
        [JsonPropertyName("coomMergeListCoomNo")] public string? CoomMergeListCoomNo { get; set; }
        [JsonPropertyName("coomSellerMemo")] public string? CoomSellerMemo { get; set; }
        [JsonPropertyName("coomSellerGoodsTotalAmt")] public int? CoomSellerGoodsTotalAmt { get; set; }
        [JsonPropertyName("coomGoodsItemNum")] public int? CoomGoodsItemNum { get; set; }
        [JsonPropertyName("coomGoodsTotalNum")] public int? CoomGoodsTotalNum { get; set; }
        [JsonPropertyName("coomRcvTotalAmt")] public int? CoomRcvTotalAmt { get; set; }
        [JsonPropertyName("coomCgdmId")] public string? CoomCgdmId { get; set; }
        [JsonPropertyName("coomShipPrintFlag")] public bool? CoomShipPrintFlag { get; set; }
        [JsonPropertyName("coomCccmNo")] public string? CoomCccmNo { get; set; }
    }

    public class OrderCartModel
    {
        [JsonPropertyName("coocNo")] public string? CoocNo { get; set; }
        [JsonPropertyName("coocPaymentType")] public string? CoocPaymentType { get; set; }
        [JsonPropertyName("coocPaymentPayDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CoocPaymentPayDatetime { get; set; }
        [JsonPropertyName("coocDeliverMethod")] public string? CoocDeliverMethod { get; set; }
        [JsonPropertyName("coocOrdChannelKind")] public string? CoocOrdChannelKind { get; set; }
        [JsonPropertyName("coocCreateDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CoocCreateDatetime { get; set; }
        [JsonPropertyName("coocMemSid")] public int? CoocMemSid { get; set; }
        [JsonPropertyName("coocPaymentCode")] public string? CoocPaymentCode { get; set; }
        [JsonPropertyName("coocOrdNameEnc")] public string? CoocOrdNameEnc { get; set; }
        [JsonPropertyName("coocRcvNameEnc")] public string? CoocRcvNameEnc { get; set; }
        [JsonPropertyName("coocRcvMobileEnc")] public string? CoocRcvMobileEnc { get; set; }
        [JsonPropertyName("coocPaymentTradeNo")] public string? CoocPaymentTradeNo { get; set; }
        [JsonPropertyName("coocPaymentNote")] public string? CoocPaymentNote { get; set; }
        [JsonPropertyName("coocPaymentBankCode")] public string? CoocPaymentBankCode { get; set; }
        [JsonPropertyName("coocPaymentDueday")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CoocPaymentDueday { get; set; }
    }

    public class OrderItemModel
    {
        [JsonPropertyName("coodName")] public string? CoodName { get; set; }
        [JsonPropertyName("coodQty")] public int? CoodQty { get; set; }
        [JsonPropertyName("coodOriginalPrice")] public int? CoodOriginalPrice { get; set; }
        [JsonPropertyName("coodDiscountPrice")] public int? CoodDiscountPrice { get; set; }
        [JsonPropertyName("coodReceivePrice")] public int? CoodReceivePrice { get; set; }
        [JsonPropertyName("coodImagePath")] public string? CoodImagePath { get; set; }
    }

    public class GoodsItemModel
    {
        [JsonPropertyName("cgdiNumberColumn1")] public string? CgdiNumberColumn1 { get; set; }
    }

    public class ShipmentMasterModel
    {
        [JsonPropertyName("esmmNo")] public string? EsmmNo { get; set; }
        [JsonPropertyName("esmmShipNo")] public string? EsmmShipNo { get; set; }
        [JsonPropertyName("esmmStatus")] public string? EsmmStatus { get; set; }
        [JsonPropertyName("esmmShipMethod")] public string? EsmmShipMethod { get; set; }
        [JsonPropertyName("esmmShipNoAuthCode")] public string? EsmmShipNoAuthCode { get; set; }
        [JsonPropertyName("esmmShipNoA")] public string? EsmmShipNoA { get; set; }
        [JsonPropertyName("esmmLeaveStoreDateB")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? EsmmLeaveStoreDateB { get; set; }
        [JsonPropertyName("esmmIbonAppFlag")] public bool? EsmmIbonAppFlag { get; set; }
        [JsonPropertyName("esmmOddReason")] public string? EsmmOddReason { get; set; }
        [JsonPropertyName("esmmConfirmExtpayDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? EsmmConfirmExtpayDatetime { get; set; }
    }

    public class QuestionMasterModel
    {
        [JsonPropertyName("sellerQaNeverReplyCount")] public int? SellerQaNeverReplyCount { get; set; }
        [JsonPropertyName("buyerQaNeverReplyCount")] public int? BuyerQaNeverReplyCount { get; set; }
    }

    public class CancelMasterModel
    {
        [JsonPropertyName("cccmStatus")] public string? CccmStatus { get; set; }
        [JsonPropertyName("cccmCancelPeople")] public string? CccmCancelPeople { get; set; }
        [JsonPropertyName("cccmConfirmDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CccmConfirmDatetime { get; set; }
        [JsonPropertyName("cccmCreateDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? CccmCreateDatetime { get; set; }
        [JsonPropertyName("cccmRefundFlag")] public bool? CccmRefundFlag { get; set; }
        [JsonPropertyName("cccmErfmNo")] public string? CccmErfmNo { get; set; }
    }

    public class CCDHLModel
    {
        [JsonPropertyName("ecdhEsmmNo")] public string? EcdhEsmmNo { get; set; }
        [JsonPropertyName("ecdhProcessCode")] public string? EcdhProcessCode { get; set; }
    }

    public class CCCSModel
    {
        [JsonPropertyName("eccsId")] public string? EccsId { get; set; }
        [JsonPropertyName("eccsStoreType")] public string? EccsStoreType { get; set; }
        [JsonPropertyName("eccsRechoiceStoreStatus")] public string? EccsRechoiceStoreStatus { get; set; }
        [JsonPropertyName("eccsCreateDatetime")][JsonConverter(typeof(DateTimeNoZConverter))] public DateTime? EccsCreateDatetime { get; set; }
    }

    public class RtnDHLApplyModel
    {
        [JsonPropertyName("erdaApplyStatus")] public string? ErdaApplyStatus { get; set; }
    }
}
