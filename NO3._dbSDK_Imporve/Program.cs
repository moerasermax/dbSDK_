using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NO3._dbSDK_Imporve.Application;
using NO3._dbSDK_Imporve.Application.Sample.Elastic;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.External;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.MAP;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis;
using System.Text.Json;



var Services = new ServiceCollection();
init();
var provider = Services.BuildServiceProvider();

await TestFlow_Mongo(provider.GetRequiredService<MongoRepository<Orders>>(), provider.GetRequiredService<ElasticRepository<OrderSummary>>(),
    provider.GetRequiredService<IDTO>()
    , provider.GetRequiredService<EventGiftRandomDataGenerator>(), provider.GetRequiredService<ObjectExtension>());

void init()
{
    IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

    var settings = new ConnectionSettings();

    Services.AddSingleton(new MongoMap());
    Services.AddSingleton(new ElasticMap());

    
    configuration.GetSection("ConnectionSettings").Bind(settings);

    Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));
    Services.AddSingleton<MongoRepository<Orders>, OrderRepository_Mongo>();

    Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", settings));
    Services.AddSingleton<ElasticRepository<OrderSummary>, OrderRepository_Elastic>();

    Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
    Services.AddSingleton<RedisRepository<Query>, OrderRepository_Redis>();

    Services.AddSingleton<IDTO, DTO>();
    Services.AddSingleton<IUniversalMapper, UniversalMapper>();

    Services.AddSingleton<EventGiftRandomDataGenerator>();

    Services.AddSingleton<ObjectExtension>();

}

async Task TestFlow_Mongo(MongoRepository<Orders> MongoRepo, ElasticRepository<OrderSummary> ElasticRepo,
    IDTO dto, 
    EventGiftRandomDataGenerator TestDataEngine, ObjectExtension objectExtension)
{
    var _mongoEngine = new DbSDKEngine<Orders>(MongoRepo);

    

    Result response;

    EventGiftModel Data = TestDataEngine.Generate();
    string condition = JsonSerializer.Serialize(dto.GetCondition(Data.event_id));

    EventGiftModel dev_DATA = objectExtension.Copy(Data);
    dev_DATA.event_id += "Dev";

    string NewData = JsonSerializer.Serialize(dev_DATA);


    /// MongoDB測試流程：Insert -> Update -> Read -> Remove
    response = (Result)await _mongoEngine.Insert(Data);
    Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{Data.event_id}"); Console.ReadKey();
    response = (Result)await _mongoEngine.Update(condition, dev_DATA);
    Console.WriteLine($"{response.Msg}......條件{condition} 更新為{NewData}"); Console.ReadKey();

    condition = JsonSerializer.Serialize(dto.GetCondition(dev_DATA.event_id));
    response = (Result)await _mongoEngine.Read(condition);
    Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{response.DataJson}"); Console.ReadKey();
    response = (Result)await _mongoEngine.Remove(condition);
    Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {condition}資料"); Console.ReadKey();



    /// ElasticSearch測試流程：Insert -> Update -> Read -> Remove
    var _elasticEngine = new DbSDKEngine<OrderSummary>(ElasticRepo);
    EventGiftSummaryModel Data_Summry = TestDataEngine.ToSummary(Data);
    string condition_Summry = JsonSerializer.Serialize(dto.GetCondition(Data_Summry.event_id));

    EventGiftSummaryModel dev_DATA_Summry = objectExtension.Copy(Data_Summry);
    dev_DATA_Summry.event_id += "Dev";

    string NewData_Summry = JsonSerializer.Serialize(dev_DATA_Summry);

    response = (Result)await _elasticEngine.Insert(Data_Summry);
    Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{Data.event_id}"); Console.ReadKey();
    response = (Result)await _elasticEngine.Update(condition_Summry, dev_DATA_Summry);
    Console.WriteLine($"{response.Msg}......條件{condition_Summry} 更新為{NewData_Summry}"); Console.ReadKey();

    condition = JsonSerializer.Serialize(dto.GetCondition(dev_DATA_Summry.event_id));
    response = (Result)await _elasticEngine.Read(condition);
    Console.WriteLine($"{response.Msg}。請按一下繼續下一步......{response.DataJson}"); Console.ReadKey();
    response = (Result)await _elasticEngine.Remove(condition_Summry);
    Console.WriteLine($"已完成資料移除。請按一下結束測試流程......已刪除 {condition}資料"); Console.ReadKey();

}



