using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Service.SendDataToMongoDB.Model.Order
{
    public class C_Order_M_Model
    {
        [BsonElement("coom_order_date")]
        [BsonIgnoreIfNull]
        public DateTime? CoomOrderDate { get; set; }

        [BsonElement("coom_name")]
        [BsonIgnoreIfNull]
        public string? CoomName { get; set; }

        [BsonElement("coom_status")]
        [BsonIgnoreIfNull]
        public string? CoomStatus { get; set; }

        [BsonElement("coom_temp_type")]
        [BsonIgnoreIfNull]
        public string? CoomTempType { get; set; }

        [BsonElement("coom_create_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CoomCreateDatetime { get; set; }

        [BsonElement("coom_cuam_cid")]
        [BsonIgnoreIfNull]
        public int? CoomCuamCid { get; set; }

        [BsonElement("coom_rechoice_flag")]
        [BsonIgnoreIfNull]
        public bool? CoomReChoiceFlag { get; set; }

        [BsonElement("coom_merge_list_coom_no")]
        [BsonIgnoreIfNull]
        public string? CoomMergeListCoomNo { get; set; }

        [BsonElement("coom_seller_memo")]
        [BsonIgnoreIfNull]
        public string? CoomSellerMemo { get; set; }

        [BsonElement("coom_seller_goods_total_amt")]
        [BsonIgnoreIfNull]
        public int? CoomSellerGoodsTotalAmt { get; set; }

        [BsonElement("coom_goods_item_num")]
        [BsonIgnoreIfNull]
        public int? CoomGoodsItemNum { get; set; }

        [BsonElement("coom_goods_total_num")]
        [BsonIgnoreIfNull]
        public int? CoomGoodsTotalNum { get; set; }

        [BsonElement("coom_rcv_total_amt")]
        [BsonIgnoreIfNull]
        public int? CoomRcvTotalAmt { get; set; }

        [BsonElement("coom_cgdm_id")]
        [BsonIgnoreIfNull]
        public string? CoomCgdmId { get; set; }

        [BsonElement("coom_ship_print_flag")]
        [BsonIgnoreIfNull]
        public string? CoomShipPrintFlag { get; set; }

        [BsonElement("coom_cccm_no")]
        [BsonIgnoreIfNull]
        public string? CoomCccmNo { get; set; }
    }
}
