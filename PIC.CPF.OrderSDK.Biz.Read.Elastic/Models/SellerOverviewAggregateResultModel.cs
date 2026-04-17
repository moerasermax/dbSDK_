namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SellerOverviewAggregateResultModel
    {
        public int DealWith { get; set; }

        public int Toship { get; set; }

        public int Shipping { get; set; }

        public int NoShowToDHL { get; set; }

        public int SellerQaNeverReply { get; set; }

        public int SellerReturnReq { get; set; }
    }
}
