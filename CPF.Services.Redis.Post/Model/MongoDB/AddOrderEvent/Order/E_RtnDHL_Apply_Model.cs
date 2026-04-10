using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
    /// <summary>
    /// 店到店退貨包裹宅資註記回檔
    /// </summary>
    public class E_Rtn_DHL_Apply_Model
    {
        [BsonElement("erda_apply_status")]
        [BsonIgnoreIfNull]
        public string? ErdaApplyStatus { get; set; }
    }
}