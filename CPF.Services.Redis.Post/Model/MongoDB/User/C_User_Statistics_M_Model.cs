using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.User
{
    public class C_User_Statistics_M_Model
    {
        [BsonElement("cusm_rrcb_qty")]
        [BsonIgnoreIfNull]
        public int? CusmRrcbQty { get; set; }

        [BsonElement("cusm_rcvd_qty")]
        [BsonIgnoreIfNull]
        public int? CusmRcvdQty { get; set; }
    }
}