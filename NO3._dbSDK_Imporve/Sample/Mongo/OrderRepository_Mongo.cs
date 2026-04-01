using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Sample.Mongo
{
    public class OrderRepository_Mongo : MongoRepository<Order>, IOrderRepository_Mongo
    {
        public OrderRepository_Mongo(MongoDBDriver driver) : base(driver, "Order")
        {
        }
    }



}
