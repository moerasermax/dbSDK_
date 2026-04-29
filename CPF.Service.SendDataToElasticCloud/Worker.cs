using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Services.Redis.Post.Model.Elastic;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using System.Linq;
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

                    if (result.DataJson != null)
                    {
                        ElasticAddOrder response = JsonSerializer.Deserialize<ElasticAddOrder>(result.DataJson);

                        Do(response);

                    }
                    else
                    {
                        Console.WriteLine("Redis目前尚無【Elastic】Request資料");

                    }


                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        async Task Do(ElasticAddOrder query)
        {
            
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

                #region S18 貨態更新事件

                case "UpdateSellerGetNumberEvent":
                    // 取號事件 - 更新 Elastic 貨態欄位
                    if (query.Args != null)
                    {
                        // 從嵌套的 esmm 物件取得資料
                        var esmm = query.Args.Esmm;
                        var coom = query.Args.Coom;
                        
                        // 組合 EsmmShipNo + EsmmShipNoAuthCode (如 D8803212 + 0964 = D88032120964)
                        var shipNo = esmm?.EsmmShipNo ?? query.Args.EsmmShipNo ?? "";
                        var authCode = esmm?.EsmmShipNoAuthCode ?? query.Args.EsmmShipNoAuthCode ?? "";
                        var fullShipNo = shipNo + authCode;

                        // 從嵌套的 coom 物件取得 CoomStatus
                        var coomStatus = coom?.CoomStatus ?? query.Args.CoomStatus;

                        var updateData = new OrderInfoModel
                        {
                            CoomNo = query.Args.CoomNo,
                            CoomStatus = coomStatus,
                            EsmmShipNo = fullShipNo,
                            EsmmStatus = "01", // 取號狀態為待寄件
                            EsmmRcvTotalAmt = coom?.CoomRcvTotalAmt ?? query.Args.CoomRcvTotalAmt
                        };

                        var updateCondition = new Condition(query.Args.CoomNo);
                        await _elasticRepo.UpdateData(JsonSerializer.Serialize(updateCondition), updateData);
                        _logger.LogInformation($"[UpdateSellerGetNumberEvent] CoomNo: {query.Args.CoomNo}, CoomStatus: {coomStatus}, EsmmStatus: 01, EsmmShipNo: {fullShipNo}");
                    }
                    break;

                case "Delivery_CargoDynamics_02":
                    // 寄貨事件 - 更新 Elastic 貨態欄位
                    if (query.Args != null)
                    {
                        // 從嵌套的 esmm 物件取得資料
                        var esmm = query.Args.Esmm;
                        var coom = query.Args.Coom;
                        
                        // 組合 EsmmShipNo + EsmmShipNoAuthCode
                        var shipNo = esmm?.EsmmShipNo ?? query.Args.EsmmShipNo ?? "";
                        var authCode = esmm?.EsmmShipNoAuthCode ?? query.Args.EsmmShipNoAuthCode ?? "";
                        var fullShipNo = shipNo + authCode;

                        // 取得寄貨時間 - 從 esml 陣列取得最新的時間
                        DateTime? shippingDateTime = null;
                        if (query.Args.Esml != null && query.Args.Esml.Count > 0)
                        {
                            shippingDateTime = query.Args.Esml.LastOrDefault()?.EsmlStatusDatetime;
                        }
                        shippingDateTime = shippingDateTime ?? query.Args.EsmlStatusShippingDatetime;

                        var updateData = new OrderInfoModel
                        {
                            CoomNo = query.Args.CoomNo,
                            CoomStatus = "30", // 配送中狀態
                            EsmmShipNo = fullShipNo,
                            EsmmStatus = "10", // 已寄件
                            EsmmRcvTotalAmt = coom?.CoomRcvTotalAmt ?? query.Args.CoomRcvTotalAmt,
                            EsmlStatusShippingDatetime = shippingDateTime
                        };

                        var updateCondition = new Condition(query.Args.CoomNo);
                        await _elasticRepo.UpdateData(JsonSerializer.Serialize(updateCondition), updateData);
                        _logger.LogInformation($"[Delivery_CargoDynamics_02] CoomNo: {query.Args.CoomNo}, CoomStatus: 30, EsmmStatus: 10, EsmmShipNo: {fullShipNo}");
                    }
                    break;

                #endregion

                default:
                    break;
            }

        }
    }
}
