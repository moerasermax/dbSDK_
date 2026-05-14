using Microsoft.Extensions.Options;
using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Models;
using StackExchange.Redis;

namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class RedisDriver : DbDriver
    {
        ConnectionMultiplexer _redis;
        public RedisDriver(string Service, ConnectionSettings dbInfo) : base(Service)
        {
                _redis = ConnectionMultiplexer.Connect(GetConnectInfo(dbInfo));
        }

        public RedisDriver(string Service, IOptions<ConnectionSettings> options)
            : this(Service, options.Value) { }

        ConfigurationOptions GetConnectInfo(ConnectionSettings dbInfo)
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
