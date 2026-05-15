using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Service.SendDataToMongoDB.Model.Order;
using CPF.Services.Redis.Post.Model;
using CPF.Services.Redis.Post.Model.Elastic;
using CPF.Services.Redis.Post.Model.QueryModel.MongoDB;
using NO3._dbSDK_Imporve.Application.Sample.Redis;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using System.Text.Json;

namespace ForTest
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoDBRepository<OrderModel> _mongoRepo;
        private readonly IRepository<OrderInfoModel> _elasticRepo;
        private readonly OrderRepository_Redis _redisRepo;
        private readonly AddOrderEventRandomDataGenerator _generator;

        public Worker(ILogger<Worker> logger, IMongoDBRepository<OrderModel> mongoRepo, IRepository<OrderInfoModel> elasticRepo, OrderRepository_Redis redisRepo
            ,AddOrderEventRandomDataGenerator generator)
        {
            _logger = logger;
            _mongoRepo = mongoRepo;
            _elasticRepo = elasticRepo;
            _redisRepo = redisRepo;
            _generator = generator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var input = Console.ReadLine();
            while (input != "exit")
            {
                switch (input.ToLower())
                {
                    default:
                        break;
                }



                input = Console.ReadLine();
            }
        }

        /// ====新建訂單 WindowsService====
        async Task CreateTestOrderFlow()
        {

        }

        /// ====寄貨流程 CFP9101====
        void ToShipFlow()
        {

        }

        ///====查詢流程 WebApi-Search====
        void SearchFlow()
        {

        }


        
        }
    
}
