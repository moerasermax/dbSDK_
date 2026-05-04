using System.Text.Json;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Models
{
    public class SearchOrderInfoResultModel
    {
        public JsonElement[]? OrderInfos { get; set; }
        public long Total { get; set; }
    }
}
