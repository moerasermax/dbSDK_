using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Security;
using Elastic.Transport;
using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class ElasticDriver : dbDriver
    {
        protected ElasticsearchClient _client;
        public ElasticDriver(string Service, dbInfo _dbInfo) : base(Service)
        {
            _Service = Service;

            var settings = new ElasticsearchClientSettings(new Uri(_dbInfo.EndPoint))
                .Authentication(new Elastic.Transport.ApiKey(_dbInfo.ApiKey));

            _client = new ElasticsearchClient(settings);
        }
        public ElasticsearchClient getClient()
        {
            return this._client;
        }

    }


}
