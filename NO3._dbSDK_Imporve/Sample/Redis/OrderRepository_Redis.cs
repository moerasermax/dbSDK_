using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis;

namespace NO3._dbSDK_Imporve.Sample.Redis
{
    public class OrderRepository_Redis : RedisRepository<Query>, IOrderRepository_Redis
    {
        public OrderRepository_Redis(RedisDriver Driver) : base(Driver, "Query")
        {
        }

        Task<IResult> IOrderRepository_Redis.pollingData()
        {
            throw new NotImplementedException();
        }
    }
}
