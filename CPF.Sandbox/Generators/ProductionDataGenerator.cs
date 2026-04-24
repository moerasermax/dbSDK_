using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Service.SendDataToMongoDB.Model.Order;
using CPF.Services.Redis.Post.Model;
using CPF.Services.Redis.Post.Model.QueryModel.MongoDB;
using MongoDB.Bson;

// 使用別名避免跨專案同名衝突
using MongoOrderModel = CPF.Service.SendDataToMongoDB.Model.Order.OrderModel;
using MongoCoom = CPF.Service.SendDataToMongoDB.Model.Order.C_Order_M_Model;
using MongoCooc = CPF.Service.SendDataToMongoDB.Model.Order.C_Order_C_Model;
using MongoCood = CPF.Service.SendDataToMongoDB.Model.Order.C_Order_D_Model;
using MongoCgdi = CPF.Service.SendDataToMongoDB.Model.Order.C_Goods_Item_Model;
using MongoEsmm = CPF.Service.SendDataToMongoDB.Model.Order.E_Shipment_M_Model;
using MongoEsml = CPF.Service.SendDataToMongoDB.Model.Order.E_Shipment_L_Model;
using MongoEsms = CPF.Service.SendDataToMongoDB.Model.Order.E_Shipment_S_Model;

namespace CPF.Sandbox.Generators
{
    /// <summary>
    /// 生產級資料產生器
    /// 整合 AddOrderEventRandomDataGenerator，直接產出正式 Model 實例
    /// </summary>
    public class ProductionDataGenerator
    {
        private readonly AddOrderEventRandomDataGenerator _source;
        private static readonly Random _rnd = new();

        public ProductionDataGenerator()
        {
            _source = new AddOrderEventRandomDataGenerator();
        }

        // ─────────────────────────────────────────────
        // MongoDB OrderModel (SendDataToMongoDB 正式 Model)
        // ─────────────────────────────────────────────
        public MongoOrderModel GenerateMongoOrderModel()
        {
            AddOrderArgs args   = _source.CreateRandomItem();
            MongoDBAddOrder dto = _source.GetMongoDataObject(args);
            var a               = dto.Args!;

            return new MongoOrderModel
            {
                PK     = a.CoomNo,
                CoocNo = a.CoocNo,

                C_Order_M = new MongoCoom
                {
                    CoomName                = a.coom.CoomName,
                    CoomOrderDate           = a.coom.CoomOrderDate,
                    CoomStatus              = a.coom.CoomStatus,
                    CoomTempType            = a.coom.CoomTempType,
                    CoomCreateDatetime      = a.coom.CoomCreateDatetime,
                    CoomCuamCid             = a.coom.CoomCuamCid,
                    CoomSellerGoodsTotalAmt = a.coom.CoomSellerGoodsTotalAmt,
                    CoomGoodsItemNum        = a.coom.CoomGoodsItemNum,
                    CoomGoodsTotalNum       = a.coom.CoomGoodsTotalNum,
                    CoomRcvTotalAmt         = a.coom.CoomRcvTotalAmt,
                    CoomCgdmId              = a.coom.CoomCgdmId
                },

                C_Order_C = new MongoCooc
                {
                    CoocPaymentType        = a.cooc.CoocPaymentType,
                    CoocPaymentPayDatetime = a.cooc.CoocPaymentPayDatetime,
                    CoocDeliverMethod      = a.cooc.CoocDeliverMethod,
                    CoocOrdChannelKind     = a.cooc.CoocOrdChannelKind,
                    CoocMemSid             = a.cooc.CoocMemSid,
                    CoocCreateDatetime     = a.cooc.CoocCreateDatetime,
                    CoocOrdNameEnc         = a.cooc.CoocOrdNameEnc,
                    CoocRcvNameEnc         = a.cooc.CoocRcvNameEnc,
                    CoocRcvMobileEnc       = a.cooc.CoocRcvMobileEnc,
                    CoocPaymentDueday      = a.cooc.CoocPaymentDueday
                },

                C_Order_D = a.cood.Select(d => new MongoCood
                {
                    CoodName          = d.CoodName,
                    CoodQty           = d.CoodQty,
                    CoodOriginalPrice = d.CoodOriginalPrice,
                    CoodDiscountPrice = d.CoodDiscountPrice,
                    CoodReceivePrice  = d.CoodReceivePrice,
                    CoodImagePath     = d.CoodImagePath
                }).ToList(),

                C_Goods_Item = a.cgdi != null ? new MongoCgdi
                {
                    CgdiNumberColumn1 = a.cgdi.CgdiNumberColumn1
                } : null,

                // 模擬物流子模型（生產環境初始為空，後續由貨態更新填入）
                E_Shipment_M = new MongoEsmm(),
                E_Shipment_L = new List<MongoEsml>(),
                E_Shipment_S = new List<MongoEsms>()
            };
        }

        // ─────────────────────────────────────────────
        // ElasticSearch OrderInfoModel (SendDataToElasticCloud 正式 Model)
        // ─────────────────────────────────────────────
        public OrderInfoModel GenerateElasticOrderInfoModel()
        {
            AddOrderArgs args   = _source.CreateRandomItem();
            var coom            = args.coom;
            var cooc            = args.cooc;

            return new OrderInfoModel
            {
                CoomNo                = coom.CoomNo,
                CoomName              = coom.CoomName,
                CoomStatus            = coom.CoomStatus,
                CoomTempType          = coom.CoomTempType,
                CoomCreateDatetime    = coom.CoomCreateDatetime,
                CoomCuamCid           = coom.CoomCuamCid,
                CoomRcvTotalAmt       = coom.CoomRcvTotalAmt,
                CoocNo                = cooc.CoocNo,
                CoocPaymentType       = cooc.CoocPaymentType,
                CoocPaymentPayDatetime= cooc.CoocPaymentPayDatetime,
                CoocDeliverMethod     = cooc.CoocDeliverMethod,
                CoocOrdChannelKind    = cooc.CoocOrdChannelKind,
                CoocMemSid            = cooc.CoocMemSid,
                CoodNames             = args.cood.Select(d => d.CoodName).ToArray(),
                CoodItems             = args.cood.Select(d => new CoodItems
                {
                    CgddCgdmid   = coom.CoomCgdmId,
                    CoodName     = d.CoodName,
                    CoodQty      = d.CoodQty,
                    CoodImagePath= d.CoodImagePath
                }).ToArray()
            };
        }

        // ─────────────────────────────────────────────
        // UpdateSellerMemo 局部更新 Model
        // ─────────────────────────────────────────────
        public (string coomNo, MongoOrderModel patch) GenerateSellerMemoPatch(string coomNo)
        {
            return (coomNo, new MongoOrderModel
            {
                PK        = coomNo,
                C_Order_M = new MongoCoom { CoomSellerMemo = $"沙盒備註_{DateTime.Now:HHmmss}" }
            });
        }

        // ─────────────────────────────────────────────
        // ChangePayType 局部更新 Model
        // ─────────────────────────────────────────────
        public (string coocNo, MongoOrderModel patch) GenerateChangePayTypePatch(string coocNo)
        {
            var payTypes = new[] { "0", "1", "3", "4", "5", "6" };
            return (coocNo, new MongoOrderModel
            {
                CoocNo    = coocNo,
                C_Order_C = new MongoCooc { CoocPaymentType = payTypes[_rnd.Next(payTypes.Length)] }
            });
        }

        // ─────────────────────────────────────────────
        // Status 20：賣家取號 — 物流模組掛載 patch
        // 對應資料流：UpdateSellerGetNumberEvent (Redis_2)
        // ─────────────────────────────────────────────
        public (string coomNo, MongoOrderModel patch) GenerateStatus20Patch(string coomNo)
        {
            var shipNo      = "D" + RandomNumericString(10);
            var esmmNo      = "SM" + RandomNumericString(13);
            var statusTime  = DateTime.UtcNow;

            return (coomNo, new MongoOrderModel
            {
                PK = coomNo,

                // 更新訂單狀態為 20
                C_Order_M = new MongoCoom { CoomStatus = "20" },

                // 掛載物流主檔 (e_shipment_m)
                E_Shipment_M = new MongoEsmm
                {
                    EsmmNo            = esmmNo,
                    EsmmShipNo        = shipNo,
                    EsmmStatus        = "01",
                    EsmmShipMethod    = "1",
                    EsmmShipNoAuthCode= "0964",
                    EsmmShipNoA       = "7M0",
                    EsmmIbonAppFlag   = "0"
                },

                // 初始化貨態歷程 (e_shipment_l)，筆數 = 1
                E_Shipment_L = new List<MongoEsml>
                {
                    new MongoEsml
                    {
                        EsmlEsmmStatus    = "01",
                        EsmlStatusDatetime= statusTime
                    }
                },

                // 初始化物流狀態 (e_shipment_s)，筆數 = 1
                E_Shipment_S = new List<MongoEsms>
                {
                    new MongoEsms
                    {
                        EsmsDlvStatusNo    = "1001",
                        EsmsStatusDatetime = statusTime
                    }
                }
            });
        }

        // ─────────────────────────────────────────────
        // Status 30：寄貨完成 — $set + $push 增量更新
        // 對應資料流：UpdateShippingCompleteEvent (Redis_1)
        // ─────────────────────────────────────────────
        public Status30Patch GenerateStatus30Patch(string coomNo)
        {
            var shippingTime = DateTime.UtcNow;
            var takeTime     = shippingTime.AddMinutes(-8); // 模擬取號時間早於寄貨時間

            return new Status30Patch
            {
                CoomNo = coomNo,

                // $set 部分
                SetFields = new BsonDocument
                {
                    { "c_order_m.coom_status",    "30" },
                    { "e_shipment_m.esmm_status",  "10" }
                },

                // $push 部分：追加新的貨態歷程
                PushEsml = new BsonDocument
                {
                    { "esml_esmm_status",     "10" },
                    { "esml_status_datetime", shippingTime.ToString("o") }
                },

                PushEsms = new BsonDocument
                {
                    { "esms_dlv_status_no",    "1A01" },
                    { "esms_status_datetime",  shippingTime.ToString("o") }
                }
            };
        }

        private static string RandomNumericString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_rnd.Next(s.Length)]).ToArray());
        }
    }

    /// <summary>
    /// Status 30 更新資料容器
    /// 包含 $set 欄位與 $push 陣列元素
    /// </summary>
    public class Status30Patch
    {
        public string CoomNo { get; set; } = "";
        public BsonDocument SetFields { get; set; } = new();
        public BsonDocument PushEsml  { get; set; } = new();
        public BsonDocument PushEsms  { get; set; } = new();
    }
}
