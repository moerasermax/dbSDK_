
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NO3._dbSDK_Imporve.Core.Configurations;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Sample;
using NO3._dbSDK_Imporve.Sample.Elastic;
using NO3._dbSDK_Imporve.Sample.Mongo;
using NO3._dbSDK_Imporve.Sample.Redis;
using static NO3._dbSDK_Imporve.Infrastructure.Driver.RedisDriver;

var Services = new ServiceCollection();

await init();


async Task init()
{
    IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

    var settings = new ConnectionSettings();
    configuration.GetSection("ConnectionSettings").Bind(settings);

    // Mongo
    string mongoConnStr = $"mongodb+srv://{settings.Mongo.User}:{settings.Mongo.Password}@{settings.Mongo.Uri}";
    Console.WriteLine($"準備連線至 Mongo: {settings.Mongo.Uri}");

    // Redis
    string redisConnStr = $"{settings.Redis.EndPoint}:{settings.Redis.Port},password={settings.Redis.Password}";
    Console.WriteLine($"準備連線至 Redis: {settings.Redis.EndPoint}");

    Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));
    Services.AddSingleton<IOrderRepository_Mongo, OrderRepository_Mongo>();

    Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", settings));
    Services.AddSingleton<IOrderRepository_Elastic, OrderRepository_Elastic>();

    Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
    Services.AddSingleton<IOrderRepository_Redis, OrderRepository_Redis>();

    var provider = Services.BuildServiceProvider();

    var _mongoEngine = new dbSDKEngine<Order>(provider.GetRequiredService<IOrderRepository_Mongo>());
    await _mongoEngine.Insert(RandomDataGenerator.EventGiftGenerator.Generate());

    Console.WriteLine("執行結束");
    Console.ReadKey();
}


#region 開發區_後面要刪除
async Task Mongo()
{
    //Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", _dbInfo));
    Services.AddSingleton<IOrderRepository_Mongo, OrderRepository_Mongo>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Mongo>();

    await OrderRepo.insertData(RandomDataGenerator.EventGiftGenerator.Generate());
}

async Task Elastic()
{
    //Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", _dbInfo));
    Services.AddSingleton<IOrderRepository_Elastic, OrderRepository_Elastic>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Elastic>();
    var engine = new dbSDKEngine<OrderSummary>(provider.GetRequiredService<IOrderRepository_Elastic>());

    OrderRepo.changeTable("order");
    await OrderRepo.insertData(RandomDataGenerator.EventGiftGenerator.ToSummary(RandomDataGenerator.EventGiftGenerator.Generate()));

}

void Redis()
{
    //Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", _redisDBInfo));
    Services.AddSingleton<IOrderRepository_Redis, OrderRepository_Redis>();

    var provider = Services.BuildServiceProvider();
    var OrderRepo = provider.GetRequiredService<IOrderRepository_Redis>();

    OrderRepo.pollingData();
}
#endregion
