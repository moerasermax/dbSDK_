using NO3._dbSDK_Imporve.Application;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using System.Text.Json;

namespace CPF.Services.Redis.Post
{
    public class Worker_Dev : BackgroundService
    {
        private readonly ILogger<Worker_Dev> _logger;
        private readonly IRepository<Query> _redis;
        private readonly EventGiftRandomDataGenerator _testDataEngine;
        private readonly IDTO _dto;

        public Worker_Dev(ILogger<Worker_Dev> logger, IRepository<Query> Redis, EventGiftRandomDataGenerator DtestDataEngine , IDTO dTO)
        {
            _logger = logger;
            _redis = Redis;
            _testDataEngine = DtestDataEngine;
            _dto = dTO;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            System.Console.WriteLine("[Redis_Post]按下【1】，發送【一般】【新增/修改/查詢/刪除】的Request");
            System.Console.WriteLine("[Redis_Post]按下【2】，發送【UnitTest】【新增/修改/查詢/刪除】的Request");
            var keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

            if (keyInfo.KeyChar.ToString().Equals("1"))
            {
                CURDflow(stoppingToken);
            } 
            else if (keyInfo.KeyChar.ToString().Equals("2"))
            {
                UnitTestCURDflow(stoppingToken);
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

        async void CURDflow(CancellationToken stoppingToken)
        {
            Query? queryData = null;
            while (!stoppingToken.IsCancellationRequested)
            {

                EventGiftModel Data = _testDataEngine.Generate();
                string JsonStr = JsonSerializer.Serialize(Data);

                /// Insert

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Insert",
                    OrderData = JsonStr
                };
                await _redis.InsertData(queryData); /// 先發給 Mongo


                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Insert",
                    OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                };
                await _redis.InsertData(queryData); /// 先發給 Elastic

                //// Update

                Data.event_id += "Dev";
                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Update",
                    OrderData = JsonSerializer.Serialize(Data)
                };
                await _redis.InsertData(queryData); /// 發給 Mongo

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Update",
                    OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic

                /// Read

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic

                /// Delete

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Delete",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Delete",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic

                System.Console.WriteLine("[Redis_Post]發送成功。");

            }
        }

        async void UnitTestCURDflow(CancellationToken stoppingToken)
        {
            int count = 0;
            Query? queryData = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                var keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                EventGiftModel Data = _testDataEngine.Generate();
                string JsonStr = JsonSerializer.Serialize(Data);

                /// Insert

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Insert",
                    OrderData = JsonStr
                };
                await _redis.InsertData(queryData); /// 先發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Insert",
                    OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                };
                await _redis.InsertData(queryData); /// 先發給 Elastic
                Console.WriteLine($"已發送第{count++}組");


                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");

                /// Update

                Data.event_id += "Dev";
                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Update",
                    OrderData = JsonSerializer.Serialize(Data)
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Update",
                    OrderData = JsonSerializer.Serialize(_testDataEngine.ToSummary(Data))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");


                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");


                /// Read

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");


                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");

                /// Delete

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Delete",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Delete",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Mongo")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Mongo
                Console.WriteLine($"已發送第{count++}組");

                queryData = new TestQuery("Elastic")
                {
                    CreateTime = DateTime.Now.ToString(),
                    QueryID = GenerateRandomCode(),
                    QueryType = "Read",
                    OrderData = JsonSerializer.Serialize(_dto.GetCondition(Data.event_id))
                };
                await _redis.InsertData(queryData); /// 發給 Elastic
                Console.WriteLine($"已發送第{count++}組");
                System.Console.WriteLine("[Redis_Post]發送成功。");

            }
        }
    }
}
