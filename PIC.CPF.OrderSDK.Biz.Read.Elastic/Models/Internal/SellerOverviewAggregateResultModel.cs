namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal
{
    public class SellerOverviewAggregateResultModel
    {
        public int DealWith { get; set; }

        public int Toship { get; set; }

        public int Shipping { get; set; }

        public int WaitReturn { get; set; }

        public int BuyerQaNeverReply { get; set; }

        public int SellerReturnReq { get; set; }
    }
}
