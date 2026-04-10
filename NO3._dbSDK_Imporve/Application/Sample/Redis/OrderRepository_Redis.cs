using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis;

namespace NO3._dbSDK_Imporve.Application.Sample.Redis
{
    public class OrderRepository_Redis : RedisRepository<Query>
    {
        public OrderRepository_Redis(RedisDriver Driver) : base(Driver, "Query")
        {
        }
        public string QueryDB { get; set; }
        public override string GetKey()
        {
            return QueryDB;
        }

    }
}
