using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NO3._dbSDK_Imporve.Application;
using NO3._dbSDK_Imporve.Application.DTO;
using NO3._dbSDK_Imporve.Application.Sample;
using NO3._dbSDK_Imporve.Application.Sample.Elastic;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Application.Services;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.External;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.MAP;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis;

// 集中註冊 MongoDB 序列化器
MongoSerializationConfig.Register();

var host = CreateHostBuilder(args).Build();

await TestFlows.RunMongoFlow(
    host.Services.GetRequiredService<IRepository<Orders>>(),
    host.Services.GetRequiredService<ElasticRepository<OrderSummary>>(),
    host.Services.GetRequiredService<IDTO>(),
    host.Services.GetRequiredService<EventGiftRandomDataGenerator>(),
    host.Services.GetRequiredService<ObjectExtension>()
);

TestFlows.RunMockFlow();
TestFlows.RunShippingSyncFlow();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            // 環境變數覆蓋，前綴 DBSDK_ (例：DBSDK_ConnectionSettings__Mongo__Uri)
            config.AddEnvironmentVariables(prefix: "DBSDK_");
        })
        .ConfigureServices((context, services) =>
        {
            var settings = new ConnectionSettings();
            context.Configuration.GetSection("ConnectionSettings").Bind(settings);

            // Map 註冊
            services.AddSingleton(new MongoMap());
            services.AddSingleton(new ElasticMap());

            // Driver 註冊
            services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));
            services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", settings));
            services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));

            // Repository 註冊 (使用介面，符合 DIP)
            services.AddSingleton<IRepository<Orders>, OrderRepository_Mongo>();
            services.AddSingleton<ElasticRepository<OrderSummary>, OrderRepository_Elastic>();
            services.AddSingleton<RedisRepository<Query>, OrderRepository_Redis>();

            // 通用服務註冊
            services.AddSingleton<IDTO, DTO>();
            services.AddSingleton<IUniversalMapper, UniversalMapper>();
            services.AddSingleton<EventGiftRandomDataGenerator>();
            services.AddSingleton<ObjectExtension>();

            // 貨態同步服務
            services.AddSingleton<ShippingSyncService>();
        });
