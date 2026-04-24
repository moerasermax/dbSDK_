using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.Order
{
    /// <summary>
    /// 訂單主結構 (聚合根)
    /// 注意：此類別不再繼承 Orders，以避免 BsonElement 名稱衝突。
    /// 所有屬性均在此類別中獨立定義。
    /// </summary>
    [BsonIgnoreExtraElements]
    public class OrderModel
    {
        /// <summary>
        /// coom_no - 業務主鍵
        /// </summary>
        [BsonElement("coom_no")]
        [BsonIgnoreIfNull]
        public string? PK { get; set; }

        /// <summary>
        /// cooc_no (GSI)
        /// </summary>
        [BsonElement("cooc_no")]
        [BsonIgnoreIfNull]
        public string? CoocNo { get; set; }

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
