using System;
using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.User
{
    public class C_User_Account_M_Model
    {
        [BsonElement("cuam_email")]
        [BsonIgnoreIfNull]
        public string? CuamEmail { get; set; }

        [BsonElement("cuam_mobile")]
        [BsonIgnoreIfNull]
        public string? CuamMobile { get; set; }

        [BsonElement("order_daytotalcount")]
        [BsonIgnoreIfNull]
        public int? OrderDayTotalCount { get; set; }
    }
}