using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
    /// <summary>
    /// 出貨配送狀態歷程檔
    /// </summary>
    public class E_Shipment_L_Model
    {
        [BsonElement("esml_esmm_status")]
        [BsonIgnoreIfNull]
        public string? EsmlEsmmStatus { get; set; }

        [BsonElement("esml_status_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? EsmlStatusDatetime { get; set; }
    }
}