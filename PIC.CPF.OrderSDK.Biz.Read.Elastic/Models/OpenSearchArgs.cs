namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class OpenSearchArgs
    {
        public string Endpoint { get; set; } = null!;

        public string Username { get; set; } = null!;

        public string Pwd { get; set; } = null!;

        public string Index { get; set; } = null!;
    }
}
