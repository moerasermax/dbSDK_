using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;


namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class AppSalesMetricsModel
    {
        public int CuamCid { get; set; }
        public DateTime? SearchStartDate { get; set; } = null;
        public DateTime? SearchEndDate { get; set; } = null;
        public DateTime? StartDatePoP { get; set; } = null;
        public DateTime? EndDatePoP { get; set; } = null;
        public DateRangeType? DateRangeType { get; set; } = null;
    }
}
