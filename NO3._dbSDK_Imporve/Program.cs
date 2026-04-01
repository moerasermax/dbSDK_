
using Microsoft.Extensions.DependencyInjection;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Sample;
using NO3._dbSDK_Imporve.Sample.Elastic;
using NO3._dbSDK_Imporve.Sample.Mongo;
using NO3._dbSDK_Imporve.Sample.Redis;
using static NO3._dbSDK_Imporve.Infrastructure.Driver.RedisDriver;

IResult _result = new Result();

var Services = new ServiceCollection();

await init();

Console.WriteLine(_result.Msg);


async Task init()
{
    dbInfo _MongodbInfo = new dbInfo()
    {
        Uri = "cluster0.txxkmtb.mongodb.net/?appName=Cluster0",
        User = "yclin_db_user",
        Password = "Aa123456"
    };
    Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", _MongodbInfo));
    Services.AddSingleton<IOrderRepository_Mongo, OrderRepository_Mongo>();

    dbInfo _dbInfo = new dbInfo() { EndPoint = "https://my-elasticsearch-project-e96f60.es.us-central1.gcp.elastic.cloud:443", ApiKey = "NGhBTEg1MEI0VWYzdktkNjJKUVM6ZmZER0RIWTA4NWxmUFl0MW82S29YUQ==" };
    Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", _dbInfo));
    Services.AddSingleton<IOrderRepository_Elastic, OrderRepository_Elastic>();

    RedisDBInfo _redisDBInfo = new RedisDBInfo()
    {
        EndPoint = "redis-17927.c259.us-central1-2.gce.cloud.redislabs.com",
        Port = 17927,
        User = "default",
        Password = "MO9yfi9LPXKF891XoOayuBikj6IUWRD6"
    };
    Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", _redisDBInfo));
    Services.AddSingleton<IOrderRepository_Redis, OrderRepository_Redis>();

    var provider = Services.BuildServiceProvider();


    var _mongoEngine = new dbSDKEngine<Order>(provider.GetRequiredService<IOrderRepository_Mongo>());
     await _mongoEngine.Insert(RandomDataGenerator.EventGiftGenerator.Generate());


    Console.WriteLine("執行結束");
    Console.ReadKey();
}


async Task Mongo()
{
    dbInfo _dbInfo = new dbInfo()
    {
        Uri = "cluster0.txxkmtb.mongodb.net/?appName=Cluster0",
        User = "yclin_db_user",
        Password = "Aa123456"
    };
    Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", _dbInfo));
    Services.AddSingleton<IOrderRepository_Mongo, OrderRepository_Mongo>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Mongo>();
  
    _result = await OrderRepo.insertData(RandomDataGenerator.EventGiftGenerator.Generate());
}

async Task Elastic()
{
    dbInfo _dbInfo = new dbInfo() { EndPoint = "https://my-elasticsearch-project-e96f60.es.us-central1.gcp.elastic.cloud:443", ApiKey = "NGhBTEg1MEI0VWYzdktkNjJKUVM6ZmZER0RIWTA4NWxmUFl0MW82S29YUQ==" };
    Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", _dbInfo));
    Services.AddSingleton<IOrderRepository_Elastic, OrderRepository_Elastic>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Elastic>();
    var engine = new dbSDKEngine<OrderSummary>(provider.GetRequiredService<IOrderRepository_Elastic>());

    OrderRepo.changeTable("order");
    _result = await OrderRepo.insertData(RandomDataGenerator.EventGiftGenerator.ToSummary(RandomDataGenerator.EventGiftGenerator.Generate()));

}

void Redis()
{
    RedisDBInfo _redisDBInfo = new RedisDBInfo()
    {
        EndPoint = "redis-17927.c259.us-central1-2.gce.cloud.redislabs.com",
        Port = 17927,
        User = "default",
        Password = "MO9yfi9LPXKF891XoOayuBikj6IUWRD6"
    };

    Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", _redisDBInfo));
    Services.AddSingleton<IOrderRepository_Redis, OrderRepository_Redis>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Redis>();

    OrderRepo.pollingData();

}