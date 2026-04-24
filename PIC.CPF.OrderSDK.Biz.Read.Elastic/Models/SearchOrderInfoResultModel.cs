namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SearchOrderInfoResultModel
    {
        public OrderInfoModel[]? OrderInfos { get; set; }
        public long Total { get; set; }
        public long Took { get; set; }
    }
}
