using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;

namespace CPF.Sandbox.Scenarios
{
    internal static class SearchSdkSetup
    {
        internal static IElasticOrderSearchService Build(
            string esEndpoint = "http://localhost:9200",
            string mongoUri = "mongodb://root:example@localhost:27017",
            string mongoDb = "CpfOrderDb",
            string mongoCollection = "Orders")
        {
            // ES 端 (OPS)
            var connSettings = new ConnectionSettings
            {
                Elastic = new DbDetail { EndPoint = esEndpoint }
            };
            var driver = new ElasticDriver("Elastic", connSettings);
            var map = new ElasticMap();
            var repo = new ElasticRepository<OrderDocument>(driver, map, "orders-*");
            var dal = new OrderSearchDal(repo);

            // Mongo 端 (DDB) — Dual Engine hydration 用
            var mongoMap = new MongoMap();
            var mongoClient = new MongoClient(mongoUri);
            var mongoDatabase = mongoClient.GetDatabase(mongoDb);
            var mongoColl = mongoDatabase.GetCollection<MongoOrder>(mongoCollection);
            var mongoSearchDal = new MongoSearchDal(mongoColl, mongoMap);

            var bll = new ElasticOrderSearchBll(dal, mongoSearchDal, null);
            return new ElasticOrderSearchService(bll, null);
        }
    }
}
