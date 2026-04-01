using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Models;
namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class MongoDBDriver : dbDriver
    {
        MongoClient _client;


        public MongoDBDriver(string Service, ConnectionSettings settings) : base(Service)
        {
            string mongoConnStr = $"mongodb+srv://{settings.Mongo.User}:{settings.Mongo.Password}@{settings.Mongo.Uri}";
            _client = new MongoClient(mongoConnStr);
        }
        
        public IMongoDatabase GetDatabase(string DBName)
        {
            return _client.GetDatabase(DBName);
        }

    }
}
