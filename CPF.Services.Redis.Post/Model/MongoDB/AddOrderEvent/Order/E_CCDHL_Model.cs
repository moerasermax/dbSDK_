using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
    /// <summary>
    /// 店到店退貨包裹待宅配檔
    /// </summary>
    public class E_CCDHL_Model
    {
        [BsonElement("ecdh_esmm_no")]
        [BsonIgnoreIfNull]
        public string? EcdhEsmmNo { get; set; }

        [BsonElement("ecdh_process_code")]
        [BsonIgnoreIfNull]
        public string? EcdhProcessCode { get; set; }
    }
}