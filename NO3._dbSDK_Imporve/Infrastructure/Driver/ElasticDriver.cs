using Elastic.Clients.Elasticsearch;
using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Models;

namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class ElasticDriver : DbDriver
    {
        protected ElasticsearchClient _client;
        public ElasticDriver(string Service, ConnectionSettings _dbInfo) : base(Service)
        {
            var settings = new ElasticsearchClientSettings(new Uri(_dbInfo.Elastic.EndPoint))
                .Authentication(new Elastic.Transport.ApiKey(_dbInfo.Elastic.ApiKey)).ServerCertificateValidationCallback((sender, certificate, chain, errors) => true);

            _client = new ElasticsearchClient(settings);
        }
        public ElasticsearchClient GetClient()
        {
            return this._client;
        }

    }


}
