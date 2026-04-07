using CPF.Services.Redis.Post;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.External;
using NO3._dbSDK_Imporve.Infrastructure.MAP;


var builder = Host.CreateApplicationBuilder(args);



IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);

builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
builder.Services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();


builder.Services.AddSingleton<IUniversalMapper, UniversalMapper>();
builder.Services.AddSingleton<EventGiftRandomDataGenerator>();
builder.Services.AddSingleton<IDTO, DTO>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
