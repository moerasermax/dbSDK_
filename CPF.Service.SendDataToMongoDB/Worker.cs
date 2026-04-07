using NO3._dbSDK_Imporve.Core.DTO;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.External;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System.Text.Json;

namespace CPF.Service.SendDataToElasticCloud
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRepository<Order> _mongoRepo;
        private readonly IRepository<Query> _Redis;
        private readonly IDTO _dto;
        private Result result { get; set;  }
        public Worker(ILogger<Worker> logger, IRepository<Order> repository, IRepository<Query> Redis, IDTO dTO)
        {
            _logger = logger;
            _mongoRepo = repository;
            _Redis = Redis;
            _dto = dTO;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Query query = new Query("Request_Mongo");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    string queryStr = JsonSerializer.Serialize(_dto.GetCondition(query.QueryDB));
                    result = await _Redis.GetData(query.QueryDB) as Result;

                    if (result != null) 
                    {
                        Query response = JsonSerializer.Deserialize<Query>(result.DataJson);

                        Do(response);

                    }


                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
        }

        async Task Do(Query query )
        {
            EventGiftModel data;
            Condition condition;
            switch (query.QueryType)
            {
                case "Insert":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    await _mongoRepo.InsertData(data);
                    break;
                case "Update":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id.Replace("Dev", "").ToString());
                    await _mongoRepo.UpdateData(JsonSerializer.Serialize(condition) ,data);
                    break;
                case "Read":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id);
                    result = await _mongoRepo.GetData(JsonSerializer.Serialize(condition)) as Result;

                    if(result.DataJson != "")
                    {
                        Console.WriteLine(result.DataJson);
                    }
                    break;
                case "Delete":
                    data = JsonSerializer.Deserialize<EventGiftModel>(query.OrderData);
                    condition = _dto.GetCondition(data.event_id);
                    await _mongoRepo.RemoveData(JsonSerializer.Serialize(condition));
                    break;
                default:
                    break;
            }

        }
    }
}
