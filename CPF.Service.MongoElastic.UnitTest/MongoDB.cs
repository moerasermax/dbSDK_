using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NO3._dbSDK_Imporve;
using NO3._dbSDK_Imporve.Application.Sample.Mongo;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using System.Text.Json;
using Xunit;
// 記得引入你主程式的命名空間

namespace CPF.Service.MongoElastic.UnitTest
{
    public class MongoDBTests
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRepository<Order> _Repo;
        private readonly IRepository<Query> _Redis;
        private readonly IDTO _dto;
        private Result result;
        private Result Request;
        private string exceptResult;
        private string RealResult;

        void init()
        {
            var builder = Host.CreateApplicationBuilder();

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var settings = new ConnectionSettings();
            configuration.GetSection("ConnectionSettings").Bind(settings);

            builder.Services.AddSingleton(new MongoMap());
            builder.Services.AddSingleton<MongoDBDriver>(s => new MongoDBDriver("MongoDB", settings));

            builder.Services.AddSingleton<IRepository<Order>, OrderRepository_Mongo>();

            builder.Services.AddSingleton<RedisDriver>(s => new RedisDriver("Redis", settings));
            builder.Services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();

            builder.Services.AddSingleton<IDTO, DTO>();
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
        }
        public MongoDBTests()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var settings = new ConnectionSettings();
            configuration.GetSection("ConnectionSettings").Bind(settings);


            _Repo = new OrderRepository_Mongo(new MongoDBDriver("Mongo", settings),new MongoMap());
            _Redis = new OrderRepository_Redis(new RedisDriver("Redis", settings));
            _dto = new DTO();
        }


        [Fact]
        public async Task Test_Mongo_Insert_Logic()
        {
            Query query = new Query("Request_Mongo");


            string queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                exceptResult = await Do(response);
            }

            queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                RealResult = await Do(response);
            }
            Assert.Equal(exceptResult, RealResult);
        }

        [Fact]
        public async Task Test_Mongo_Update_Logic()
        {
            Query query = new Query("Request_Mongo");


            string queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                exceptResult = await Do(response);
            }

            queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                RealResult = await Do(response);
            }
            Assert.Equal(exceptResult, RealResult);
        }

        [Fact]
        public async Task Test_Mongo_Read_Logic()
        {
            Query query = new Query("Request_Mongo");


            string queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                RealResult = await Do(response);
            }
            exceptResult = "";

            Assert.NotEqual(RealResult, exceptResult);
        }

        [Fact]
        public async Task Test_Mongo_Delete_Logic()
        {
            Query query = new Query("Request_Mongo");


            string queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                exceptResult = await Do(response);
            }

            queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
            Request = await _Redis.GetData(query.QueryDB) as Result;

            if (Request != null)
            {
                /// Insert
                Query response = JsonSerializer.Deserialize<Query>(Request.DataJson);

                RealResult = await Do(response);
            }
            Assert.NotEqual(exceptResult, RealResult);
        }


        async Task<string> Do(Query query)
        {
            EventGiftModel data;
            Condition condition;
            switch (query.QueryType)
            {
                case "Insert":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    await _Repo.InsertData(data);
                    return query.QueryDB;
                case "Update":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id.Replace("Dev", "").ToString());
                    await _Repo.UpdateData(JsonSerializer.Serialize(condition), data);
                    return query.OrderData;
                case "Read":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id);
                    result = await _Repo.GetData(JsonSerializer.Serialize(condition)) as Result;
                    return result.DataJson;
                case "Delete":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id);
                    await _Repo.RemoveData(JsonSerializer.Serialize(condition));
                    return query.OrderData;
                default:
                    return "";
            }

        }
    }
}