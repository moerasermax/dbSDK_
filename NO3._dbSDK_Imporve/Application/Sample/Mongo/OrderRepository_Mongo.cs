using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;


namespace NO3._dbSDK_Imporve.Application.Sample.Mongo
{
    public class OrderRepository_Mongo : MongoRepository<Order>, IOrderRepository_Mongo
    {
        public OrderRepository_Mongo(MongoDBDriver driver) : base(driver, "Order")
        {
        }
    }



}
