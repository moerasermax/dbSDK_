using MongoDB.Bson.Serialization.Attributes;
namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
   

    public class C_Order_D_Model
    {
        [BsonElement("cood_name")]
        [BsonIgnoreIfNull]
        public string? CoodName { get; set; }

        [BsonElement("cood_qty")]
        [BsonIgnoreIfNull]
        public int? CoodQty { get; set; }

        [BsonElement("cood_price")]
        [BsonIgnoreIfNull]
        public int? CoodOriginalPrice { get; set; }

        [BsonElement("cood_discount_price")]
        [BsonIgnoreIfNull]
        public int? CoodDiscountPrice { get; set; }

        [BsonElement("cood_receive_price")]
        [BsonIgnoreIfNull]
        public int? CoodReceivePrice { get; set; }

        [BsonElement("cood_image_path")]
        [BsonIgnoreIfNull]
        public string? CoodImagePath { get; set; }
    }
}
