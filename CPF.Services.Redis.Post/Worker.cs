using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Infrastructure.External;
using System.Text.Json;

namespace CPF.Services.Redis.Post
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRepository<Query> _redis;
        private readonly EventGiftRandomDataGenerator _testDataEngine;
        private readonly IDTO _dto;

        public Worker(ILogger<Worker> logger, IRepository<Query> Redis, EventGiftRandomDataGenerator TestDataEngine,IDTO dTO)
        {
            _logger = logger;
            _redis = Redis;
            _testDataEngine = TestDataEngine;
            _dto = dTO;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Query? queryData = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                System.Console.WriteLine("[Redis_Post]按下【1】，發送新增的Request");
                var keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                if (keyInfo.KeyChar.ToString().Equals("1"))
                {

                    EventGiftModel Data = _testDataEngine.Generate();
                    string JsonStr = JsonSerializer.Serialize(Data);
                    queryData = new Query("Mongo")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Insert",
                        OrderData = JsonStr
                    };
                    await _redis.InsertData(queryData); /// 先發給 Mongo


                    queryData = new Query("Elastic")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Insert",
                        OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Elastic

                    System.Console.WriteLine("[Redis_Post]發送 新增 成功。");



                    System.Console.WriteLine("[Redis_Post]按下【2】，發送修改的Request");
                    keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                    Data.event_id += "Dev";
                    queryData = new Query("Mongo")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Update",
                        OrderData = JsonSerializer.Serialize(Data)
                    };
                    await _redis.InsertData(queryData); /// 先發給 Mongo

                    queryData = new Query("Elastic")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Update",
                        OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Elastic

                    System.Console.WriteLine("[Redis_Post]發送 更新 成功。");


                    System.Console.WriteLine("[Redis_Post]按下【3】，發送查詢的Request");
                    keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                    queryData = new Query("Mongo")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Read",
                        OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Mongo

                    queryData = new Query("Elastic")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Read",
                        OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Elastic

                    System.Console.WriteLine("[Redis_Post]發送 更新 成功。");

                    System.Console.WriteLine("[Redis_Post]按下【4】，發送刪除的Request");
                    keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                    queryData = new Query("Mongo")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Delete",
                        OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Mongo

                    queryData = new Query("Elastic")
                    {
                        CreateTime = DateTime.Now.ToString(),
                        QueryID = GenerateRandomCode(),
                        QueryType = "Delete",
                        OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                    };
                    await _redis.InsertData(queryData); /// 先發給 Elastic

                    System.Console.WriteLine("[Redis_Post]發送 更新 成功。");

                }

                

            }


            await Task.Delay(1000, stoppingToken);
        }
        public string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
