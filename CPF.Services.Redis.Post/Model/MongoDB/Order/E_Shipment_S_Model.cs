using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Service.SendDataToMongoDB.Model.Order
{
    public class E_Shipment_S_Model
    {
        [BsonElement("esms_dlv_status_no")]
        [BsonIgnoreIfNull]
        public string? EsmsDlvStatusNo { get; set; }

        [BsonElement("esms_status_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? EsmsStatusDatetime { get; set; }
    }
}