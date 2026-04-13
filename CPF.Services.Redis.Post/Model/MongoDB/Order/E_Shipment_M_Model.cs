using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.Order // 命名空間已同步修改為 MongoDB
{
    /// <summary>
    /// 貨態主檔
    /// </summary>
    public class E_Shipment_M_Model
    {
        [BsonElement("esmm_no")]
        [BsonIgnoreIfNull]
        public string? EsmmNo { get; set; }

        [BsonElement("esmm_ship_no")]
        [BsonIgnoreIfNull]
        public string? EsmmShipNo { get; set; }

        [BsonElement("esmm_status")]
        [BsonIgnoreIfNull]
        public string? EsmmStatus { get; set; }

        [BsonElement("esmm_ship_method")]
        [BsonIgnoreIfNull]
        public string? EsmmShipMethod { get; set; }

        [BsonElement("esmm_ship_no_auth_code")]
        [BsonIgnoreIfNull]
        public string? EsmmShipNoAuthCode { get; set; }

        [BsonElement("esmm_ship_no_a")]
        [BsonIgnoreIfNull]
        public string? EsmmShipNoA { get; set; }

        [BsonElement("esmm_leave_store_date_b")]
        [BsonIgnoreIfNull]
        public DateTime? EsmmLeaveStoreDateB { get; set; }

        [BsonElement("esmm_ibon_app_flag")]
        [BsonIgnoreIfNull]
        public string? EsmmIbonAppFlag { get; set; }

        [BsonElement("esmm_odd_reason")]
        [BsonIgnoreIfNull]
        public string? EsmmOddReason { get; set; }

        [BsonElement("esmm_confirm_extpay_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? EsmmConfirmExtpayDatetime { get; set; }
    }
}
