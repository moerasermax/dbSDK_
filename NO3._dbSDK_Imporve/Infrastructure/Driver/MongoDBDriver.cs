using MongoDB.Driver;
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
    public class MongoDBDriver : dbDriver
    {
        MongoClient _client;


        public MongoDBDriver(string Service,dbInfo connectionData) : base(Service)
        {
            _Service = Service;
            string ConnectionString = string.Format("mongodb+srv://{0}:{1}@{2}", connectionData.User, connectionData.Password, connectionData.Uri);
            _client = new MongoClient(ConnectionString);
        }
        
        public IMongoDatabase GetDatabase(string DBName)
        {
            return _client.GetDatabase(DBName);
        }

    }
}
