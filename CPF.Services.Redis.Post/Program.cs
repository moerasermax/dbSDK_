using CPF.Services.Redis.Post;
using CPF.Services.Redis.Post.Model;
using NO3._dbSDK_Imporve.Application;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.MAP;


var builder = Host.CreateApplicationBuilder(args);



IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);

builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
builder.Services.AddSingleton<IdbDriver>(s => s.GetRequiredService<RedisDriver>());
builder.Services.AddSingleton<OrderRepository_Redis>(); ///為了使同樣的Json不改動原先的json格式，因此直接用類別去加入到Services的容器，去達到分流目的。


builder.Services.AddSingleton<IUniversalMapper, UniversalMapper>();
builder.Services.AddSingleton<AddOrderEventRandomDataGenerator>();
builder.Services.AddSingleton<EventGiftRandomDataGenerator>();
builder.Services.AddHostedService<Worker_ForCPF>();

var host = builder.Build();

host.Run();
