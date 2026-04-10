using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Service.SendDataToMongoDB.Model.Order
{
    /// <summary>
    /// 賣場項目設定檔
    /// </summary>
    public class C_Goods_Item_Model
    {
        [BsonElement("cgdi_number_column1")]
        [BsonIgnoreIfNull]
        public int? CgdiNumberColumn1 { get; set; } = null!;
    }
}
