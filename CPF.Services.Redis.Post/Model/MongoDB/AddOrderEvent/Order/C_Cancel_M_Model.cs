using System;
using MongoDB.Bson.Serialization.Attributes;

namespace CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order
{
    /// <summary>
    /// 取消單主檔
    /// </summary>
    public class C_Cancel_M_Model
    {
        [BsonElement("cccm_status")]
        [BsonIgnoreIfNull]
        public string? CccmStatus { get; set; }

        [BsonElement("cccm_cancel_people")]
        [BsonIgnoreIfNull]
        public string? CccmCancelPeople { get; set; }

        [BsonElement("cccm_confirm_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CccmConfirmDatetime { get; set; }

        [BsonElement("cccm_create_datetime")]
        [BsonIgnoreIfNull]
        public DateTime? CccmCreateDatetime { get; set; }

        [BsonElement("cccm_refund_flag")]
        [BsonIgnoreIfNull]
        public string? CccmRefundFlag { get; set; }

        [BsonElement("cccm_erfm_no")]
        [BsonIgnoreIfNull]
        public string? CccmErfmNo { get; set; }
    }
}