using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Service.SendDataToMongoDB.Model.Order
{
    public class C_Order_C_Model
    {
        [BsonElement("cooc_payment_type")]
        [BsonIgnoreIfNull]
        public string? CoocPaymentType { get; set; }

        [BsonElement("cooc_payment_pay_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CoocPaymentPayDatetime { get; set; }

        [BsonElement("cooc_deliver_method")]
        [BsonIgnoreIfNull]
        public string? CoocDeliverMethod { get; set; }

        [BsonElement("cooc_ord_channel_kind")]
        [BsonIgnoreIfNull]
        public string? CoocOrdChannelKind { get; set; }

        [BsonElement("cooc_create_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CoocCreateDatetime { get; set; }

        [BsonElement("cooc_mem_sid")]
        [BsonIgnoreIfNull]
        public int? CoocMemSid { get; set; }

        [BsonElement("cooc_payment_code")]
        [BsonIgnoreIfNull]
        public string? CoocPaymentCode { get; set; }

        [BsonElement("cooc_ord_name_enc")]
        [BsonIgnoreIfNull]
        public string? CoocOrdNameEnc { get; set; }

        [BsonElement("cooc_rcv_name_enc")]
        [BsonIgnoreIfNull]
        public string? CoocRcvNameEnc { get; set; }

        [BsonElement("cooc_rcv_mobile_enc")]
        [BsonIgnoreIfNull]
        public string? CoocRcvMobileEnc { get; set; }

        [BsonElement("cooc_payment_trade_no")]
        [BsonIgnoreIfNull]
        public string? CoocPaymentTradeNo { get; set; }

        [BsonElement("cooc_payment_note")]
        [BsonIgnoreIfNull]
        public string? CoocPaymentNote { get; set; }

        [BsonElement("cooc_payment_bank_code")]
        [BsonIgnoreIfNull]
        public string? CoocPaymentBankCode { get; set; }

        [BsonElement("cooc_payment_dueday")]
        [BsonIgnoreIfNull]
        public DateTime? CoocPaymentDueday { get; set; }
    }
}
