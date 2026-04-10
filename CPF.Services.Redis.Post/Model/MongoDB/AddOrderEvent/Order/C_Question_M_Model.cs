using System;
using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
    /// <summary>
    /// 問答主檔
    /// </summary>
    public class C_Question_M_Model
    {
        [BsonElement("seller_qa_never_reply_count")]
        [BsonIgnoreIfNull]
        public int? SellerQaNeverReplyCount { get; set; }

        [BsonElement("buyer_qa_never_reply_count")]
        [BsonIgnoreIfNull]
        public int? BuyerQaNeverReplyCount { get; set; }
    }
}