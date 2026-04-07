using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;


namespace NO3._dbSDK_Imporve.Application.Sample.Elastic
{
    public class OrderRepository_Elastic : ElasticRepository<OrderSummary>
    {
        public OrderRepository_Elastic(ElasticDriver driver, ElasticMap elasticMap) : base(driver, elasticMap, "order")
        {
        }
    }
}
