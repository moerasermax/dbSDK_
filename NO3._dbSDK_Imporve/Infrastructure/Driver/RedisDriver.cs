using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Models;
using StackExchange.Redis;

namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class RedisDriver : dbDriver
    {
        ConnectionMultiplexer _redis;
        public RedisDriver(string Service, ConnectionSettings dbInfo) : base(Service)
        {
                _redis = ConnectionMultiplexer.Connect(getConnectInfo(dbInfo));
        }

        ConfigurationOptions getConnectInfo(ConnectionSettings dbInfo)
        {
            return new ConfigurationOptions
            {
                EndPoints = { { dbInfo.Redis.EndPoint, dbInfo.Redis.Port } },
                User = dbInfo.Redis.User,
                Password = dbInfo.Redis.Password
            };
        }

        public IDatabase GetDataBase()
        {
            return _redis.GetDatabase();
        }
    }
}
