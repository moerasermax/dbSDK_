namespace NO3._dbSDK_Imporve.Core.Entity
{
    /// <summary>
    /// 賣家取號事件 (Redis Event)
    /// 對應資料流：取號_資料流
    /// </summary>
    public class UpdateSellerGetNumberEvent
    {
        public string CoomNo { get; set; } = "";
        
        public CoomData? coom { get; set; }
        public EsmmData? esmm { get; set; }
        public List<EsmlData>? esml { get; set; }
        public List<EsmsData>? esms { get; set; }
    }

    public class CoomData
    {
        public string? CoomName { get; set; }
        public string? CoomOrderDate { get; set; }
        public string? CoomStatus { get; set; }
        public string? CoomCccmNo { get; set; }
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

    public class EsmmData
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
    }

    public class EsmlData
    {
        public string? EsmlEsmmStatus { get; set; }
        public string? EsmlStatusDatetime { get; set; }
    }

    public class EsmsData
    {
        public string? EsmsDlvStatusNo { get; set; }
        public string? EsmsStatusDatetime { get; set; }
    }

    /// <summary>
    /// 外部資料庫 Model (客戶端)
    /// 對應資料流：寄貨_資料流
    /// </summary>
    public class DBModel
    {
        public string Coom_No { get; set; } = "";
        public string Coom_Status { get; set; } = "";
        public string? Coom_MergeList_CoomNo { get; set; }
        public string Esmm_No { get; set; } = "";
        public string Esmm_Ship_No { get; set; } = "";
        public string Esmm_Status { get; set; } = "";
        public string Esmm_Ship_Method { get; set; } = "";
        public string Esmm_ShipNo_AuthCode { get; set; } = "";
        public string Esmm_Ship_No_A { get; set; } = "";
        public string Esmm_IbonApp_Flag { get; set; } = "";
        public string? Cccm_Status { get; set; }
        public string Esml_EsmmNo_List { get; set; } = "";
        public string Esms_EsmmNo_List { get; set; } = "";
    }
}