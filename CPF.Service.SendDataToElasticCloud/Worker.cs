using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Services.Redis.Post.Model.Elastic;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System.Text.Json;

namespace CPF.Service.SendDataToElasticCloud
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRepository<OrderSummary> _elasticRepo;
        private readonly IRepository<Query> _Redis;
        private readonly IDTO _dto;
        private Result result { get; set; }
        public Worker(ILogger<Worker> logger, IRepository<OrderSummary> repository, IRepository<Query> Redis, IDTO dTO)
        {
            _logger = logger;
            _elasticRepo = repository;
            _Redis = Redis;
            _dto = dTO;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ElasticAddOrder query = new ElasticAddOrder();
            string Key = "Request_Elastic";
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    result = await _Redis.GetData(Key) as Result;

                    if (result != null)
                    {
                        ElasticAddOrder response = JsonSerializer.Deserialize<ElasticAddOrder>(result.DataJson);

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

        async Task Do(ElasticAddOrder query)
        {
            
            Condition condition;
            switch (query.Name)
            {
                case "AddOrderEvent":

                    OrderInfoModel data = new OrderInfoModel();

                    data.CoomNo = query.Args.CoomNo;
                    data.CoomName = query.Args.CoomName;
                    data.CoomStatus = query.Args.CoomStatus;
                    data.CoomTempType = query.Args.CoomTempType;
                    data.CoomCreateDatetime = query.Args.CoomCreateDatetime;
                    data.CoomCuamCid = query.Args.CoomCuamCid;
                    data.CoocNo = query.Args.CoocNo;
                    data.CoocPaymentType = query.Args.CoocPaymentType;
                    data.CoocDeliverMethod = query.Args.CoocDeliverMethod;
                    data.CoocOrdChannelKind = query.Args.CoocOrdChannelKind;
                    data.CoocMemSid = query.Args.CoocMemSid;
                    data.CoocPaymentPayDatetime = query.Args.CoocPaymentPayDatetime;
                    data.CoodNames = query.Args.CoodNames.ToArray();
                    data.CoomRcvTotalAmt = query.Args.CoomRcvTotalAmt;
                    data.CoodItems = query.Args.CoodItems.Select(x => new CoodItems()
                    {
                        CgddCgdmid = x.CgddCgdmid,
                        CgddId = x.CgddId,
                        CoodCgdsId = x.CoodCgdsId,
                        CoodName = x.CoodName,
                        CoodQty = x.CoodQty,
                        CoodImagePath = x.CgddId,
                    }).ToArray();

                    await _elasticRepo.InsertData(data);
                    break;
                //case "Update":
                //    data = JsonSerializer.Deserialize<EventGiftSummaryModel>(query.OrderData);
                //    condition = _dto.GetCondition(data.event_id.Replace("Dev", "").ToString());
                //    await _elasticRepo.UpdateData(JsonSerializer.Serialize(condition), data);
                //    break;
                //case "Read":
                //    data = JsonSerializer.Deserialize<EventGiftSummaryModel>(query.OrderData);
                //    condition = _dto.GetCondition(data.event_id);
                //    result = await _elasticRepo.GetData(JsonSerializer.Serialize(condition)) as Result;

                //    if (result.DataJson != "")
                //    {
                //        Console.WriteLine(result.DataJson);
                //    }
                //    break;
                //case "Delete":
                //    data = JsonSerializer.Deserialize<EventGiftSummaryModel>(query.OrderData);
                //    condition = _dto.GetCondition(data.event_id);
                //    await _elasticRepo.RemoveData(JsonSerializer.Serialize(condition));
                //    break;
                default:
                    break;
            }

        }
    }
}
