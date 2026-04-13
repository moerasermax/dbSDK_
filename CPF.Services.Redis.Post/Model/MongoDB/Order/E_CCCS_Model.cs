using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.Order
{
    /// <summary>
    /// 門市關轉店檔
    /// </summary>
    public class E_CCCS_Model
    {
        [BsonElement("eccs_id")]
        [BsonIgnoreIfNull]
        public int? EccsId { get; set; }

        [BsonElement("eccs_store_type")]
        [BsonIgnoreIfNull]
        public string? EccsStoreType { get; set; }

        [BsonElement("eccs_rechoice_store_status")]
        [BsonIgnoreIfNull]
        public string? EccsRechoiceStoreStatus { get; set; }

        [BsonElement("eccs_create_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? EccsCreateDatetime { get; set; }
    }
}