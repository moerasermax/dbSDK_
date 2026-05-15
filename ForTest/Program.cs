using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Service.SendDataToMongoDB.Model.Order;
using CPF.Services.Redis.Post.Model;
using ForTest;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Serialization;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();


var builder = Host.CreateApplicationBuilder(args);

/// ====更新貨態寄貨====
builder.Services.AddDbSdk(configuration);
builder.Services.AddDbSdkMongoRepository<OrderModel>("Orders-YCTest");
builder.Services.AddDbSdkElasticRepository<OrderInfoModel>("orders-yctest");
/// ====創建訂單====
builder.Services.AddSingleton<OrderRepository_Redis>();
builder.Services.AddSingleton<AddOrderEventRandomDataGenerator>();
/// ====WebAPI====
/// XXX

builder.Services.AddHostedService(provider => new Worker(
    provider.GetRequiredService<ILogger<Worker>>(), 
    provider.GetRequiredService<IMongoDBRepository<OrderModel>>(), 
    provider.GetRequiredService<IRepository<OrderInfoModel>>(),
    provider.GetRequiredService<OrderRepository_Redis>(),
    provider.GetRequiredService<AddOrderEventRandomDataGenerator>()
));

var host = builder.Build();
host.Run();
