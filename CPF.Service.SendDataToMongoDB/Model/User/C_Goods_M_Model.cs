using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Service.SendDataToMongoDB.Model.User
{
    public class C_Goods_M_Model
    {
        [BsonElement("cgdm_id")]
        [BsonIgnoreIfNull]
        public string? CgdmId { get; set; }

        [BsonElement("cgdm_update_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CgdmUpdateDatetime { get; set; }
    }
}