namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class BuyerOverviewAggregateResultModel
    {
        public int Unpaid { get; set; }

        public int Toship { get; set; }

        public int ToFinish { get; set; }

        public int Cancel { get; set; }

        public int BuyerQaNeverReply { get; set; }

        public int Finish { get; set; }

        public int BuyerReturnReq { get; set; }
    }
}
