using CPF.Service.SendDataToMongoDB;
using CPF.Service.SendDataToMongoDB.Model.Order;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);

builder.Services.AddSingleton(new MongoMap());
builder.Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));

// OrderModel 不再繼承 Orders，直接使用 MongoRepository<OrderModel>
builder.Services.AddSingleton<IMongoDBRepository<OrderModel>>(s =>
    new MongoRepository<OrderModel>(
        s.GetRequiredService<MongoDBDriver>(),
        s.GetRequiredService<MongoMap>(),
        s.GetRequiredService<IDTO>(),
        "Order"
    )
);

builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
builder.Services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();

builder.Services.AddSingleton<IDTO, DTO>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();


