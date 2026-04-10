using NO3._dbSDK_Imporve.Core.Entity;

namespace CPF.Services.Redis.Post.Model.Elastic
{
    public class ElasticAddOrder : Query
    {
        public string? Name { get; set; } = null!;
        public OrderArgs? Args { get; set; } = null!;
    }

    public class OrderArgs
    {
        public string? CoomNo { get; set; } = null!;
        public string? CoomName { get; set; } = null!;
        public string? CoomStatus { get; set; } = null!;
        public string? CoomTempType { get; set; } = null!;
        public string? CoomCreateDatetime { get; set; }
        public int? CoomCuamCid { get; set; }
        public string? CoocNo { get; set; } = null!;
        public string? CoocPaymentType { get; set; } = null!;
        public string? CoocDeliverMethod { get; set; } = null!;
        public string? CoocOrdChannelKind { get; set; } = null!;
        public int? CoocMemSid { get; set; }
        public DateTime? CoocPaymentPayDatetime { get; set; }
        public List<string>? CoodNames { get; set; }
        public int? CoomRcvTotalAmt { get; set; }
        public List<CoodItem>? CoodItems { get; set; } = new();
    }

    public class CoodItem
    {
        public string? CgddCgdmid { get; set; } = null!;
        public string? CgddId { get; set; } = null!;
        public string? CoodCgdsId { get; set; } = null!;
        public string? CoodName { get; set; } = null!;
        public int? CoodQty { get; set; }
        public string? CoodImagePath { get; set; } = null!;
    }
}
