using CPF.Service.SendDataToElasticCloud;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;

var builder = Host.CreateApplicationBuilder(args);




IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);

builder.Services.AddSingleton(new MongoMap());
builder.Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));

builder.Services.AddSingleton<IRepository<Order>, OrderRepository_Mongo>();

builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis",settings));
builder.Services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();

builder.Services.AddSingleton<IDTO, DTO>();
builder.Services.AddHostedService<Worker>();


var host = builder.Build();
host.Run();


