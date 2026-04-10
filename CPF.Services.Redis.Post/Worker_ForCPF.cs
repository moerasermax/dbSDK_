using CPF.Services.Redis.Post.Model;
using CPF.Services.Redis.Post.Model.Elastic;
using CPF.Services.Redis.Post.Model.MongoDB;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using System.Text.Json;
namespace CPF.Services.Redis.Post
{
    public class Worker_ForCPF : BackgroundService
    {
        private readonly OrderRepository_Redis _redis;
        private readonly AddOrderEventRandomDataGenerator _CPF_TestDataEngine;

        public Worker_ForCPF(OrderRepository_Redis Redis
            , AddOrderEventRandomDataGenerator MongoDBCPF_TestDataEngine
            )
        {
            _redis = Redis;
            _CPF_TestDataEngine = MongoDBCPF_TestDataEngine;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)

            {
                System.Console.WriteLine("[Redis_Post]按下【1】，發送【一般】【新增/修改】的Request");
                var keyInfo = Console.ReadKey(intercept: true); // true 代表不把按下的字顯示在螢幕上

                if (keyInfo.KeyChar.ToString().Equals("1"))
                {
                    Createflow();
                    Createflow();
                    Createflow();
                    Createflow();
                    Createflow();
                }
                else if (keyInfo.KeyChar.ToString().Equals("2"))
                {
                    UpdateFolw();
                }


                await Task.Delay(1000, stoppingToken);

            }
        }
        public string GenerateRandomCode(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        async void Createflow()
        {


            var TestData = _CPF_TestDataEngine.Generate();

            _redis.QueryDB = "Request_MongoDB";

            await _redis.InsertData(_CPF_TestDataEngine.GetMongoDataObject(TestData));


            /// Elastic

            _redis.QueryDB = "Request_Elastic";
            await _redis.InsertData(_CPF_TestDataEngine.GetElasticDataObject(TestData));

        }

        async void UpdateFolw()
        {
            var UpdateData = _CPF_TestDataEngine.GetMongoUpDataObject("Test");

            await _redis.InsertData(UpdateData);

        }


    }
}
