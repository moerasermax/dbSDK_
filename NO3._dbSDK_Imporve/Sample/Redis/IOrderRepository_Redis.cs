using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;

namespace NO3._dbSDK_Imporve.Sample.Redis
{
    public interface IOrderRepository_Redis : IRepository<Query>
    {
        public Task<IResult> pollingData();
    }

    public class Query
    {
        public string QueryID {  get; set; }
        public string OrderData { get; set; }
        public string CreateTime { get; set; }
    }
}
