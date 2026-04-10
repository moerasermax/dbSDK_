using CPF.Service.SendDataToElasticCloud;
using NO3._dbSDK_Imporve.Application.Sample.Elastic;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Redis;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();





IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);

builder.Services.AddSingleton(new ElasticMap());
builder.Services.AddSingleton<ElasticDriver>(s => new ElasticDriver("Elastic", settings));

builder.Services.AddSingleton<IRepository<OrderSummary>, OrderRepository_Elastic>();

builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
builder.Services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();

builder.Services.AddSingleton<IDTO, DTO>();
builder.Services.AddHostedService<Worker>();



var host = builder.Build();
host.Run();
