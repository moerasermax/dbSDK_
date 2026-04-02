using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NO3._dbSDK_Imporve.Application;
using NO3._dbSDK_Imporve.Application.Sample;
using NO3._dbSDK_Imporve.Application.Sample.Elastic;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.MAP;
using System.Text.Json;



var Services = new ServiceCollection();
await init();
var provider = Services.BuildServiceProvider();







EventGiftRandomDataGenerator TestDataEngine = new EventGiftRandomDataGenerator(provider.GetRequiredService<IUniversalMapper>());


await TestFlow(provider.GetRequiredService<IOrderRepository_Mongo>(), provider.GetRequiredService<IDTO>());

async Task init()
{
    IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

    var settings = new ConnectionSettings();
    configuration.GetSection("ConnectionSettings").Bind(settings);

    Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));
    Services.AddSingleton<IOrderRepository_Mongo, OrderRepository_Mongo>();

    Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", settings));
    Services.AddSingleton<IOrderRepository_Elastic, OrderRepository_Elastic>();

    Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
    Services.AddSingleton<IOrderRepository_Redis, OrderRepository_Redis>();

    Services.AddSingleton<IDTO, DTO>();
    Services.AddSingleton<IUniversalMapper, UniversalMapper>();
}

async Task TestFlow(IOrderRepository_Mongo MongoRepo, IDTO dto)
{
    var _mongoEngine = new dbSDKEngine<Order>(MongoRepo);

    string condition = JsonSerializer.Serialize(dto.getCondition("EVT2569_GFT98142"));

    EventGiftModel Data = TestDataEngine.Generate();
    EventGiftModel dev_DATA = Data;
    dev_DATA.Id += "Dev";

    EventGiftSummaryModel Data_Summry = TestDataEngine.ToSummary(Data);

    string NewData = JsonSerializer.Serialize(dev_DATA);


    await _mongoEngine.Insert(Data);
    Console.WriteLine($"已完成資料新增。請按一下繼續下一步......{Data.event_id}"); Console.ReadKey();
    await _mongoEngine.Update(condition, dev_DATA);
    Console.WriteLine($"已完成資料更新。請按一下繼續下一步......條件{condition} 更新為{NewData}"); Console.ReadKey();
    string result = (await _mongoEngine.Read(condition)).DataJson;
    Console.WriteLine($"已完成資料查詢。請按一下繼續下一步......{result}"); Console.ReadKey();
    await _mongoEngine.Remove(condition);
    Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {condition}資料"); Console.ReadKey();
}



