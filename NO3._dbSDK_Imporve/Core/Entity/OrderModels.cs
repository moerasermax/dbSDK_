namespace NO3._dbSDK_Imporve.Core.Entity
{
    /// <summary>
    /// 訂單主檔 Model
    /// </summary>
    public class C_Order_M_Model
    {
        public string? CoomOrderDate { get; set; }
        public string? CoomName { get; set; }
        public string? CoomStatus { get; set; }
        public string? CoomTempType { get; set; }
        public string? CoomCreateDatetime { get; set; }
        public int? CoomCuamCid { get; set; }
        public int? CoomSellerGoodsTotalAmt { get; set; }
        public int? CoomGoodsItemNum { get; set; }
        public int? CoomGoodsTotalNum { get; set; }
        public int? CoomRcvTotalAmt { get; set; }
        public string? CoomCgdmId { get; set; }
        public string? CoomSellerMemo { get; set; }
        public string? CoomReChoiceFlag { get; set; }
        public string? CoomMergeListCoomNo { get; set; }
        public string? CoomShipPrintFlag { get; set; }
    }

    /// <summary>
    /// 訂單聯絡資訊 Model
    /// </summary>
    public class C_Order_C_Model
    {
        public string? CoocDeliverMethod { get; set; }
        public string? CoocPaymentType { get; set; }
        public string? CoocOrdChannelKind { get; set; }
        public int? CoocMemSid { get; set; }
        public string? CoocRcvNameEnc { get; set; }
        public string? CoocOrdNameEnc { get; set; }
        public string? CoocRcvMobileEnc { get; set; }
        public string? CoocCreateDatetime { get; set; }
    }

    /// <summary>
    /// 訂單明細 Model
    /// </summary>
    public class C_Order_D_Model
    {
        public string? CoodName { get; set; }
        public int? CoodQty { get; set; }
        public int? CoodPrice { get; set; }
        public int? CoodDiscountPrice { get; set; }
        public int? CoodReceivePrice { get; set; }
        public string? CoodImagePath { get; set; }
    }

    /// <summary>
    /// 物流主檔 Model
    /// </summary>
    public class E_Shipment_M_Model
    {
        public string? EsmmNo { get; set; }
        public string? EsmmShipNo { get; set; }
        public string? EsmmStatus { get; set; }
        public string? EsmmShipMethod { get; set; }
        public string? EsmmShipNoAuthCode { get; set; }
        public string? EsmmShipNoA { get; set; }
        public string? EsmmLeaveStoreDateB { get; set; }
        public string? EsmmIbonAppFlag { get; set; }
        public string? EsmmOddReason { get; set; }
        public string? EsmmConfirmExtPayDatetime { get; set; }
        public int? EsmmRcvTotalAmt { get; set; }
    }

    /// <summary>
    /// 貨態歷程 Model
    /// </summary>
    public class E_Shipment_L_Model
    {
        public string? EsmlEsmmStatus { get; set; }
        public DateTime? EsmlStatusDatetime { get; set; }
    }

    /// <summary>
    /// 物流狀態 Model
    /// </summary>
    public class E_Shipment_S_Model
    {
        public string? EsmsDlvStatusNo { get; set; }
        public DateTime? EsmsStatusDatetime { get; set; }
    }

    /// <summary>
    /// 商品項目 Model
    /// </summary>
    public class C_Goods_Item_Model
    {
        public int? CgdiNumberColumn1 { get; set; }
    }
}
