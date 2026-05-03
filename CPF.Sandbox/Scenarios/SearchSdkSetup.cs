using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;

namespace CPF.Sandbox.Scenarios
{
    internal static class SearchSdkSetup
    {
        internal static IElasticOrderSearchService Build(string esEndpoint = "http://localhost:9200")
        {
            var connSettings = new ConnectionSettings
            {
                Elastic = new DbDetail { EndPoint = esEndpoint }
            };
            var driver = new ElasticDriver("Elastic", connSettings);
            var map = new ElasticMap();
            var repo = new ElasticRepository<OrderDocument>(driver, map, "orders-*");
            var dal = new OrderSearchDal(repo);
            var bll = new ElasticOrderSearchBll(dal, null);
            return new ElasticOrderSearchService(bll, null);
        }
    }
}
