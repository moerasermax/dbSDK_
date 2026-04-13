using CPF.Services.Redis.Post.Model.Elastic;
using CPF.Services.Redis.Post.Model.MongoDB;
using CPF.Services.Redis.Post.Model.MongoDB.Order;
using NO3._dbSDK_Imporve.Infrastructure.External;


namespace CPF.Services.Redis.Post.Model
{
    public class AddOrderEventRandomDataGenerator : BaseRandomDataGenerator<AddOrderArgs>
    {
        /// <summary>
        /// 一鍵產生包含所有子結構的 OrderModel 假資料
        /// </summary>

        public override AddOrderArgs CreateRandomItem()
        {
            // 1. 先產生明細資料 (D)，方便後續計算總金額與總數量
            int itemCount = _random.Next(1, 5); // 產生 1 到 4 筆明細
            var details = new C_Order_D_CreateOrder[itemCount];
            int totalQty = 0;
            int totalAmt = 0;

            for (int i = 0; i < itemCount; i++)
            {
                int qty = _random.Next(1, 10);
                int price = _random.Next(100, 5000); // 隨機單價
                details[i] = new C_Order_D_CreateOrder
                {
                    CgddId = Guid.NewGuid().ToString().Substring(0, 10),
                    CoodCgdsId = "G" + RandomString(14),
                    CoodName = $"隨機商品 - {RandomString(5)}",
                    CoodQty = qty,
                    CoodOriginalPrice = price,
                    CoodDiscountPrice = 0,
                    CoodReceivePrice = price,
                    CoodImagePath = $"https://example.com/img/{i}.jpg"
                };
                totalQty += qty;
                totalAmt += (price * qty);
            }

            // 2. 產生主檔資料 (M)
            var master = new C_Order_M_CreateOrder
            {
                CoomNo = "CM"+RandomNumericString(13), // 必須是 15 個字元
                CoomOrderDate = DateTime.Now,
                CoomName = "測試訂單_" + DateTime.Now.ToString("yyyyMMddHHmm"),
                CoomStatus = "10", // 固定值
                CoomTempType = _random.Next(1, 4).ToString("D2"), // 01, 02, 03
                CoomCreateDatetime = DateTime.Now.ToString(),
                CoomCuamCid = _random.Next(1, 99999),
                CoomSellerGoodsTotalAmt = Math.Min(totalAmt, 20000), // 限制在 20000 內
                CoomGoodsItemNum = itemCount,
                CoomGoodsTotalNum = totalQty,
                CoomRcvTotalAmt = Math.Min(totalAmt, 20000),
                CoomCgdmId = "GM"+RandomNumericString(13)
            };

            // 3. 產生付款/配送相關資料 (C)
            var context = new C_Order_C_CreateOrder
            {
                CoocNo = "CC"+RandomNumericString(13),
                CoocPaymentType = _random.Next(0, 7).ToString(), // '0' 到 '6'
                CoocDeliverMethod = _random.Next(1, 4).ToString(), // '1' 到 '3'
                CoocOrdChannelKind = _random.Next(1, 7).ToString(), // '1' 到 '6'
                CoocMemSid = _random.Next(1, 999999),
                CoocCreateDatetime = DateTime.Now,
                CoocOrdNameEnc = "Encrypted_Name",
                CoocRcvNameEnc = "Encrypted_Receiver",
                CoocRcvMobileEnc = "Encrypted_Mobile",
                CoocPaymentPayDatetime = DateTime.Now.AddMinutes(10),
                CoocPaymentDueday = DateTime.Now.AddDays(3)
            };

            // 4. 產生商品項額外資訊 (可選)
            var goodsItem = new C_Goods_Item_CreateOrder
            {
                CgdiNumberColumn1 = _random.Next(1, 100)
            };

            return new AddOrderArgs
            {
                coom = master,
                cooc = context,
                cood = details,
                cgdi = goodsItem
            };
        }

        // 輔助工具：產生隨機英數字字串
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        // 輔助工具：產生隨機數字字串 (用於符合 CoomNo 等格式)
        private static string RandomNumericString(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public MongoDBAddOrder GetMongoDataObject(AddOrderArgs source)
        {
            MongoDBAddOrder OrderModel = new MongoDBAddOrder()
            {
                Name = "",
                Args = new MongoDB.OrderArgs()
            };

            OrderModel.Name = "AddOrderEvent";
            OrderModel.Args.CoomNo = source.coom.CoomNo;
            OrderModel.Args.CoocNo = source.cooc.CoocNo;

            // --- coom: 訂單主檔 Mapping ---
            OrderModel.Args.coom.CoomName = source.coom.CoomName;
            OrderModel.Args.coom.CoomOrderDate = source.coom.CoomOrderDate;
            OrderModel.Args.coom.CoomStatus = source.coom.CoomStatus;
            OrderModel.Args.coom.CoomCccmNo = null; // 修正範例漏寫的屬性
            OrderModel.Args.coom.CoomTempType = source.coom.CoomTempType;
            OrderModel.Args.coom.CoomCreateDatetime = source.coom.CoomCreateDatetime;
            OrderModel.Args.coom.CoomCuamCid = source.coom.CoomCuamCid;
            OrderModel.Args.coom.CoomSellerGoodsTotalAmt = source.coom.CoomSellerGoodsTotalAmt;
            OrderModel.Args.coom.CoomGoodsItemNum = source.coom.CoomGoodsItemNum;
            OrderModel.Args.coom.CoomGoodsTotalNum = source.coom.CoomGoodsTotalNum;
            OrderModel.Args.coom.CoomRcvTotalAmt = source.coom.CoomRcvTotalAmt;
            OrderModel.Args.coom.CoomCgdmId = source.coom.CoomCgdmId;
            OrderModel.Args.coom.CoomSellerMemo = null;
            OrderModel.Args.coom.CoomReChoiceFlag = null;
            OrderModel.Args.coom.CoomMergeListCoomNo = null;
            OrderModel.Args.coom.CoomShipPrintFlag = null;

            // --- cooc: 訂單內容/付款相關 Mapping ---
            OrderModel.Args.cooc.CoocPaymentType = source.cooc.CoocPaymentType;
            OrderModel.Args.cooc.CoocPaymentPayDatetime = source.cooc.CoocPaymentPayDatetime;
            OrderModel.Args.cooc.CoocDeliverMethod = source.cooc.CoocDeliverMethod;
            OrderModel.Args.cooc.CoocOrdChannelKind = source.cooc.CoocOrdChannelKind;
            OrderModel.Args.cooc.CoocMemSid = source.cooc.CoocMemSid;
            OrderModel.Args.cooc.CoocCreateDatetime = source.cooc.CoocCreateDatetime;
            OrderModel.Args.cooc.CoocPaymentCode = null;
            OrderModel.Args.cooc.CoocOrdNameEnc = source.cooc.CoocOrdNameEnc;
            OrderModel.Args.cooc.CoocRcvNameEnc = source.cooc.CoocRcvNameEnc;
            OrderModel.Args.cooc.CoocRcvMobileEnc = source.cooc.CoocRcvMobileEnc;
            OrderModel.Args.cooc.CoocPaymentTradeNo = null;
            OrderModel.Args.cooc.CoocPaymentNote = null;
            OrderModel.Args.cooc.CoocPaymentBankCode = null;
            OrderModel.Args.cooc.CoocPaymentDueday = source.cooc.CoocPaymentDueday;

            // --- cood: 訂單明細 Mapping (使用 LINQ Select 轉換 List/Array) ---
            OrderModel.Args.cood = source.cood.Select(d => new CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order.C_Order_D_Model
            {
                CoodName = d.CoodName,
                CoodQty = d.CoodQty,
                CoodOriginalPrice = d.CoodOriginalPrice,
                CoodDiscountPrice = d.CoodDiscountPrice,
                CoodReceivePrice = d.CoodReceivePrice,
                CoodImagePath = d.CoodImagePath
            }).ToList(); // 若 Args.cood 是 List 則用 .ToList()


            // ---cgdi ---
            if (source.cgdi != null)
            {
                OrderModel.Args.cgdi = new CPF.Services.Redis.Post.Model.MongoDB.AddOrderEvent.Order.C_Goods_Item_Model
                {
                    CgdiNumberColumn1 = source.cgdi.CgdiNumberColumn1
                };
            }

            OrderModel.Args.CoomNoList = null;
            return OrderModel;

        }
        public ElasticAddOrder GetElasticDataObject(AddOrderArgs source)
        {
            ElasticAddOrder elasticDataObject = new ElasticAddOrder()
            {
                Name = "",
                Args = new Elastic.OrderArgs()
            };

            ///coom
            elasticDataObject.Name = "AddOrderEvent";
            elasticDataObject.Args.CoomNo = source.coom.CoomNo;
            elasticDataObject.Args.CoomName = source.coom.CoomName;
            elasticDataObject.Args.CoomStatus = source.coom.CoomStatus;
            elasticDataObject.Args.CoomTempType = source.coom.CoomTempType;
            elasticDataObject.Args.CoomCreateDatetime = source.coom.CoomCreateDatetime;
            elasticDataObject.Args.CoomCuamCid = source.coom.CoomCuamCid;
            elasticDataObject.Args.CoomRcvTotalAmt = source.coom.CoomRcvTotalAmt;

            ///cooc
            elasticDataObject.Args.CoocNo = source.cooc.CoocNo;
            elasticDataObject.Args.CoocPaymentType = source.cooc.CoocPaymentType;
            elasticDataObject.Args.CoocDeliverMethod = source.cooc.CoocDeliverMethod;
            elasticDataObject.Args.CoocOrdChannelKind = source.cooc.CoocOrdChannelKind;
            elasticDataObject.Args.CoocMemSid = source.cooc.CoocMemSid;
            elasticDataObject.Args.CoocPaymentPayDatetime = source.cooc.CoocPaymentPayDatetime;

            ///cood
            elasticDataObject.Args.CoodNames = source.cood.Select(x => x.CoodName).ToList();
            elasticDataObject.Args.CoodItems = new List<CoodItem>();
            foreach (var item in source.cood)
            {
                CoodItem coodItem = new CoodItem()
                {
                    CgddCgdmid = source.coom.CoomCgdmId,
                    CoodName = item.CoodName,
                    CoodQty = item.CoodQty,
                    CoodImagePath = item.CoodImagePath,
                    CgddId = item.CgddId,
                    CoodCgdsId = item.CoodCgdsId
                };
                elasticDataObject.Args.CoodItems.Add(coodItem);
            }

            return elasticDataObject;
        }


        public MongodbUpdateOrder GetMongoUpDataObject(string coomNo)
        {
            return new MongodbUpdateOrder()
            {
                Name = "UpdateSellerMemoEvent",
                Args = new SellerMemoArgs()
                {
                    CoomNo = coomNo,
                    coom = new C_Order_M_Model()
                    {
                        CoomSellerMemo = "更新備註"
                    }
                }
            };

        }

    }
}
