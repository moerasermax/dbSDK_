using NO3._dbSDK_Imporve.Core.Abstraction;
using NO3._dbSDK_Imporve.Core.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NO3._dbSDK_Imporve.Infrastructure.Driver
{
    public class RedisDriver : dbDriver
    {
        ConnectionMultiplexer _redis;
        public RedisDriver(string Service, RedisDBInfo dbInfo) : base(Service)
        {
                _Service = Service;
                _redis = ConnectionMultiplexer.Connect(getConnectInfo(dbInfo));
        }

        ConfigurationOptions getConnectInfo(RedisDBInfo dbInfo)
        {
            return new ConfigurationOptions
            {
                EndPoints = { { dbInfo.EndPoint, dbInfo.Port } },
                User = dbInfo.User,
                Password = dbInfo.Password
            };
        }

        public IDatabase GetDataBase()
        {
            return _redis.GetDatabase();
        }


        public class RedisDBInfo : dbInfo
        {
            public int Port { get; set; }
        }
    }
}
