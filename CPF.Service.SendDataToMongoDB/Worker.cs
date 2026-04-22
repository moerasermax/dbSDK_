using CPF.Service.SendDataToMongoDB.Model;
using CPF.Service.SendDataToMongoDB.Model.Order;
using CPF.Services.Redis.Post.Model.QueryModel.MongoDB;
using NO3._dbSDK_Imporve.Core.Entity;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
using System.Reflection;
using System.Text.Json;

namespace CPF.Service.SendDataToMongoDB
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IMongoDBRepository<Orders> _mongoRepo;
        private readonly IRepository<Query> _Redis;
        private readonly IDTO _dto;
        private Result result { get; set;  }
        public Worker(ILogger<Worker> logger, IMongoDBRepository<Orders> repository, IRepository<Query> Redis, IDTO dTO)
        {
            _logger = logger;
            _mongoRepo = repository;
            _Redis = Redis;
            _dto = dTO;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            MongoDBAddOrder query = new MongoDBAddOrder();

            string Key = "Request_MongoDB";

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    result = await _Redis.GetData(Key) as Result;

                    if (result.DataJson != null) 
                    {

                        Do(result.DataJson);

                    }
                    else
                    {
                        result = Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, "目前沒有資料");
                        Console.WriteLine($"目前狀態：{result.IsSuccess}，回報訊息：{result.Msg}");

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

        async Task Do(string Query_Json)
        {
            Query query = JsonSerializer.Deserialize <Query>(Query_Json);

            OrderModel UpdateOrderData = null;

            switch (query.Name)
            {
                case "AddOrderEvent":
                    MongoDBAddOrder Addquery = JsonSerializer.Deserialize<MongoDBAddOrder>(Query_Json);
                    OrderModel data = new OrderModel()
                    {
                        C_Order_M = new C_Order_M_Model(),
                        C_Cancel_M = new C_Cancel_M_Model(),
                        CoocNo = "",
                        C_Goods_Item = new C_Goods_Item_Model(),
                        C_Order_C = new C_Order_C_Model(),
                        C_Order_D = new List<C_Order_D_Model>(),
                        C_Question_M = new C_Question_M_Model(),
                        E_CCCS = new E_CCCS_Model(),
                        E_CCDHL = new E_CCDHL_Model(),
                        E_RtnDHL_Apply = new E_Rtn_DHL_Apply_Model(),
                        E_Shipment_L = new List<E_Shipment_L_Model>(),
                        E_Shipment_M = new E_Shipment_M_Model(),
                        E_Shipment_S = new List<E_Shipment_S_Model>(),
                        PK = ""
                    };


                    
                    data.PK = Addquery.Args.CoomNo;
                    data.CoocNo = Addquery.Args.CoocNo;

                    // --- C_Order_M (主檔) ---
                    data.C_Order_M.CoomName = Addquery.Args.coom.CoomName;
                    data.C_Order_M.CoomOrderDate = Addquery.Args.coom.CoomOrderDate;
                    data.C_Order_M.CoomStatus = Addquery.Args.coom.CoomStatus;
                    data.C_Order_M.CoomTempType = Addquery.Args.coom.CoomTempType;
                    data.C_Order_M.CoomCuamCid = Addquery.Args.coom.CoomCuamCid;
                    data.C_Order_M.CoomSellerGoodsTotalAmt = Addquery.Args.coom.CoomSellerGoodsTotalAmt;
                    data.C_Order_M.CoomGoodsItemNum = Addquery.Args.coom.CoomGoodsItemNum;
                    data.C_Order_M.CoomGoodsTotalNum = Addquery.Args.coom.CoomGoodsTotalNum;
                    data.C_Order_M.CoomRcvTotalAmt = Addquery.Args.coom.CoomRcvTotalAmt;
                    data.C_Order_M.CoomCgdmId = Addquery.Args.coom.CoomCgdmId;
                    data.C_Order_M.CoomSellerMemo = Addquery.Args.coom.CoomSellerMemo;
                    data.C_Order_M.CoomReChoiceFlag = Addquery.Args.coom.CoomReChoiceFlag;
                    data.C_Order_M.CoomMergeListCoomNo = Addquery.Args.coom.CoomMergeListCoomNo;
                    data.C_Order_M.CoomShipPrintFlag = Addquery.Args.coom.CoomShipPrintFlag;
                    data.C_Order_M.CoomCreateDatetime = Addquery.Args.coom.CoomCreateDatetime;
                    data.C_Order_M.CoomCccmNo = Addquery.Args.coom.CoomCccmNo;

                    /// cooc
                    data.C_Order_C.CoocPaymentType = Addquery.Args.cooc.CoocPaymentType;
                    data.C_Order_C.CoocPaymentPayDatetime = Addquery.Args.cooc.CoocPaymentPayDatetime;
                    data.C_Order_C.CoocDeliverMethod = Addquery.Args.cooc.CoocDeliverMethod;
                    data.C_Order_C.CoocOrdChannelKind = Addquery.Args.cooc.CoocOrdChannelKind;
                    data.C_Order_C.CoocMemSid = Addquery.Args.cooc.CoocMemSid;
                    data.C_Order_C.CoocCreateDatetime = Addquery.Args.cooc.CoocCreateDatetime;
                    data.C_Order_C.CoocPaymentCode = Addquery.Args.cooc.CoocPaymentCode;
                    data.C_Order_C.CoocOrdNameEnc = Addquery.Args.cooc.CoocOrdNameEnc;
                    data.C_Order_C.CoocRcvNameEnc = Addquery.Args.cooc.CoocRcvNameEnc;
                    data.C_Order_C.CoocRcvMobileEnc = Addquery.Args.cooc.CoocRcvMobileEnc;
                    data.C_Order_C.CoocPaymentTradeNo = Addquery.Args.cooc.CoocPaymentTradeNo;
                    data.C_Order_C.CoocPaymentNote = Addquery.Args.cooc.CoocPaymentNote;
                    data.C_Order_C.CoocPaymentBankCode = Addquery.Args.cooc.CoocPaymentBankCode;
                    data.C_Order_C.CoocPaymentDueday = Addquery.Args.cooc.CoocPaymentDueday;


                    // --- C_Order_D (明細清單) ---
                    if (Addquery.Args.cood != null)
                    {
                        data.C_Order_D = Addquery.Args.cood.Select(x => new C_Order_D_Model
                        {
                            CoodName = x.CoodName,
                            CoodQty = x.CoodQty,
                            CoodOriginalPrice = x.CoodOriginalPrice,
                            CoodDiscountPrice = x.CoodDiscountPrice,
                            CoodReceivePrice = x.CoodReceivePrice,
                            CoodImagePath = x.CoodImagePath
                        }).ToList();
                    }

                    // --- C_Goods_Item (商品額外資訊) ---
                    if (Addquery.Args.cgdi != null)
                    {
                        data.C_Goods_Item.CgdiNumberColumn1 = Addquery.Args.cgdi.CgdiNumberColumn1;
                    }



                    await _mongoRepo.InsertData(data);
                    break;
                case "UpdateSellerMemoEvent":
                    UpdateCoomSellerMemo Updatequery = JsonSerializer.Deserialize<UpdateCoomSellerMemo>(Query_Json);

                    CRUD_Condition_COOM condition_m = new CRUD_Condition_COOM(Updatequery.Args.CoomNo);
                    UpdateOrderData = new OrderModel()
                    {
                        PK = Updatequery.Args.CoomNo,
                        C_Order_M = new C_Order_M_Model() { CoomSellerMemo = Updatequery.Args.coom.CoomSellerMemo }
                    };

                    await _mongoRepo.UpdateData(JsonSerializer.Serialize(condition_m) , UpdateOrderData);
                    break;

                case "UpdateChangePayTypeEvent":
                    UpdateChangePayTypeEvent UpdateQuery = JsonSerializer.Deserialize<UpdateChangePayTypeEvent>(Query_Json);

                    CRUD_Condition_COOC condition_c = new CRUD_Condition_COOC(UpdateQuery.Args.CoocNo);
                    UpdateOrderData = new OrderModel()
                    {
                        CoocNo = UpdateQuery.Args.CoocNo,
                        C_Order_C = new C_Order_C_Model() { CoocPaymentType = UpdateQuery.Args.cooc.CoocPaymentType }
                    };

                    // 移除特定欄位
                    var options = new MongoUpdateOptions
                    {
                        UnsetFields = new List<string> { "c_order_c.cooc_payment_dueday" }
                    };


                    await _mongoRepo.UpdateData(JsonSerializer.Serialize(condition_c), UpdateOrderData,options);

                    break;
                default:
                    break;
            }

        }
    }
}
