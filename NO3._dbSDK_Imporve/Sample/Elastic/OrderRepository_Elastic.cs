using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;


namespace NO3._dbSDK_Imporve.Sample.Elastic
{
    public class OrderRepository_Elastic : ElasticRepository<OrderSummary>, IOrderRepository_Elastic
    {
        public OrderRepository_Elastic(ElasticDriver driver) : base(driver, "Order")
        {
        }
    }
}
