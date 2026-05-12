using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    // Search 7 路徑用 — mirror 客戶原 SDK `_searchDal.GetUserByCuamCidFromDDB(cuamCid)` 單筆 UserModel
    // 客戶端 DDB/DynamoDB → dbSDK 改 MongoDB Users collection 直讀
    // 測資 cuam_cid 為 string、SDK 介面 int — DAL 層轉接
    [BsonIgnoreExtraElements]
    public class MongoUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("cuam_cid")] public string? CuamCid { get; set; }
        [BsonElement("c_user_account_m")] public BsonDocument? CUserAccountM { get; set; }
        [BsonElement("c_user_statistics_m")] public BsonDocument? CUserStatisticsM { get; set; }
        [BsonElement("c_goods_m")] public List<CGoodsM>? CGoodsM { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class CGoodsM
    {
        [BsonElement("cgdm_id")] public string? CgdmId { get; set; }
        [BsonElement("cgdm_update_datetime")] public DateTime? CgdmUpdateDatetime { get; set; }
    }
}
