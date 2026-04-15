using MongoDB.Bson.Serialization.Attributes;
using NO3._dbSDK_Imporve.Core.Entity;
namespace CPF.Service.SendDataToMongoDB.Model.Order
{
    /// <summary>
    /// 訂單主結構 (聚合根)
    /// </summary>
    public class OrderModel : Orders
    {
        /// <summary>
        /// coom_no
        /// </summary>
        /// <value></value>
        [BsonId] // 標示為 MongoDB 的主鍵 (_id)，取代原 DynamoDBHashKey
        public string _id { get; set; }

        [BsonElement("coom_no")]
        public string? PK { get; set; } = null!;

        /// <summary>
        /// cooc_no (GSI)
        /// </summary>
        /// <value></value>
        [BsonElement("cooc_no")] // GSI 在 MongoDB 是透過建置 Collection 索引來處理，Model 層保留欄位對應即可
        [BsonIgnoreIfNull]
        public string? CoocNo { get; set; } = null!;

        [BsonElement("c_order_m")]
        [BsonIgnoreIfNull]
        public C_Order_M_Model? C_Order_M { get; set; }

        [BsonElement("c_order_c")]
        [BsonIgnoreIfNull]
        public C_Order_C_Model? C_Order_C { get; set; }

        [BsonElement("c_order_d")]
        [BsonIgnoreIfNull]
        public List<C_Order_D_Model>? C_Order_D { get; set; }

        [BsonElement("c_goods_item")]
        [BsonIgnoreIfNull]
        public C_Goods_Item_Model? C_Goods_Item { get; set; }

        [BsonElement("e_shipment_m")]
        [BsonIgnoreIfNull]
        public E_Shipment_M_Model? E_Shipment_M { get; set; }

        [BsonElement("c_question_m")]
        [BsonIgnoreIfNull]
        public C_Question_M_Model? C_Question_M { get; set; }

        [BsonElement("c_cancel_m")]
        [BsonIgnoreIfNull]
        public C_Cancel_M_Model? C_Cancel_M { get; set; }

        [BsonElement("e_shipment_l")]
        [BsonIgnoreIfNull]
        public List<E_Shipment_L_Model>? E_Shipment_L { get; set; }

        [BsonElement("e_shipment_s")]
        [BsonIgnoreIfNull]
        public List<E_Shipment_S_Model>? E_Shipment_S { get; set; }

        [BsonElement("e_cccs")]
        [BsonIgnoreIfNull]
        public E_CCCS_Model? E_CCCS { get; set; }

        [BsonElement("e_ccdhl")]
        [BsonIgnoreIfNull]
        public E_CCDHL_Model? E_CCDHL { get; set; }

        [BsonElement("e_rtn_dhl_apply")]
        [BsonIgnoreIfNull]
        public E_Rtn_DHL_Apply_Model? E_RtnDHL_Apply { get; set; }
    }
}