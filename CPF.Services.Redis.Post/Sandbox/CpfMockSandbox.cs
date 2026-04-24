using CPF.Services.Redis.Post.Model;
using CPF.Services.Redis.Post.Model.MongoDB.Order;
using CPF.Services.Redis.Post.Model.QueryModel.MongoDB;
using MongoDB.Bson;
using System.Text.Json;

namespace CPF.Services.Redis.Post.Sandbox
{
    /// <summary>
    /// CPF 業務邏輯驗證沙盒
    /// 不依賴任何外部環境（Redis / MongoDB），純粹驗證資料轉換與指令產生邏輯
    /// </summary>
    public static class CpfMockSandbox
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 執行全部模擬場景
        /// </summary>
        public static void RunSimulation()
        {
            PrintHeader("CPF Mock Sandbox 啟動");

            SimulateAddOrder();
            SimulateChangePayType();
            SimulateUpdateSellerMemo();

            PrintHeader("CPF Mock Sandbox 完成");
        }

        // ─────────────────────────────────────────────
        // 場景 1：新增訂單 (AddOrder)
        // ─────────────────────────────────────────────
        private static void SimulateAddOrder()
        {
            PrintSection("場景 1：新增訂單 (AddOrder)");

            var generator = new AddOrderEventRandomDataGenerator();
            AddOrderArgs args = generator.CreateRandomItem();
            MongoDBAddOrder mongoObj = generator.GetMongoDataObject(args);

            // 模擬 Insert 指令：直接將 OrderModel 轉為 BsonDocument
            var orderModel = BuildOrderModelFromArgs(mongoObj);
            BsonDocument insertDoc = orderModel.ToBsonDocument();

            Console.WriteLine($"[AddOrder] CoomNo  : {mongoObj.Args!.CoomNo}");
            Console.WriteLine($"[AddOrder] CoocNo  : {mongoObj.Args.CoocNo}");
            Console.WriteLine($"[AddOrder] 明細筆數 : {mongoObj.Args.cood?.Count ?? 0}");
            Console.WriteLine();
            Console.WriteLine("--- Insert BsonDocument ---");
            Console.WriteLine(insertDoc.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));

            // 關鍵欄位驗證
            bool coomNoOk = insertDoc.Contains("coom_no") && insertDoc["coom_no"].AsString == mongoObj.Args.CoomNo;
            bool coocNoOk = insertDoc.Contains("cooc_no") && insertDoc["cooc_no"].AsString == mongoObj.Args.CoocNo;
            bool coomOk   = insertDoc.Contains("c_order_m");
            bool coocOk   = insertDoc.Contains("c_order_c");
            bool coodOk   = insertDoc.Contains("c_order_d") && insertDoc["c_order_d"].AsBsonArray.Count > 0;

            Console.WriteLine();
            Console.WriteLine($"✅ coom_no 正確對應 CoomNo : {(coomNoOk ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ cooc_no 正確對應 CoocNo : {(coocNoOk ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ c_order_m 存在          : {(coomOk   ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ c_order_c 存在          : {(coocOk   ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ c_order_d 有明細資料    : {(coodOk   ? "PASS" : "FAIL")}");
        }

        // ─────────────────────────────────────────────
        // 場景 2：變更付款方式 (ChangePayType)
        // ─────────────────────────────────────────────
        private static void SimulateChangePayType()
        {
            PrintSection("場景 2：變更付款方式 (ChangePayType)");

            var generator = new AddOrderEventRandomDataGenerator();

            // 使用固定 CoocNo 模擬
            const string testCoocNo = "CC2265481053604";
            UpdateChangePayTypeEvent updateObj = generator.GetMongoUpdateChangePayTypeEventObject(testCoocNo);

            // 模擬 $set 指令產生（扁平化）
            BsonDocument setPayload = BuildFlatSetDocument(updateObj.Args!.cooc);
            BsonDocument filter     = new BsonDocument("cooc_no", testCoocNo);
            BsonDocument updateCmd  = new BsonDocument("$set", setPayload);

            Console.WriteLine($"[ChangePayType] Filter CoocNo : {testCoocNo}");
            Console.WriteLine($"[ChangePayType] 新付款方式    : {updateObj.Args.cooc?.CoocPaymentType}");
            Console.WriteLine();
            Console.WriteLine("--- Filter ---");
            Console.WriteLine(filter.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));
            Console.WriteLine("--- $set 指令 ---");
            Console.WriteLine(updateCmd.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));

            // 關鍵欄位驗證
            bool filterOk      = filter["cooc_no"].AsString == testCoocNo;
            bool payTypeOk     = setPayload.Contains("cooc_payment_type");
            bool noNullFields  = !setPayload.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 條件正確對應 CoocNo : {(filterOk     ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ $set 包含 cooc_payment_type : {(payTypeOk   ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ $set 中無 null 值 (局部更新) : {(noNullFields ? "PASS" : "FAIL")}");
        }

        // ─────────────────────────────────────────────
        // 場景 3：更新備註 (UpdateSellerMemo)
        // ─────────────────────────────────────────────
        private static void SimulateUpdateSellerMemo()
        {
            PrintSection("場景 3：更新備註 (UpdateSellerMemo)");

            var generator = new AddOrderEventRandomDataGenerator();

            const string testCoomNo = "CM4216179510575";
            UpdateCoomSellerMemo updateObj = generator.GetMongoUpdateCoomSellerMemoObject(testCoomNo);

            // 模擬 $set 指令產生（扁平化，加上 c_order_m 前綴）
            BsonDocument setPayload = BuildFlatSetDocumentWithPrefix(updateObj.Args!.coom, "c_order_m");
            BsonDocument filter     = new BsonDocument("coom_no", testCoomNo);
            BsonDocument updateCmd  = new BsonDocument("$set", setPayload);

            Console.WriteLine($"[UpdateSellerMemo] Filter CoomNo : {testCoomNo}");
            Console.WriteLine($"[UpdateSellerMemo] 新備註內容    : {updateObj.Args.coom?.CoomSellerMemo}");
            Console.WriteLine();
            Console.WriteLine("--- Filter ---");
            Console.WriteLine(filter.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));
            Console.WriteLine("--- $set 指令 ---");
            Console.WriteLine(updateCmd.ToJson(new MongoDB.Bson.IO.JsonWriterSettings { Indent = true }));

            // 關鍵欄位驗證
            bool filterOk     = filter["coom_no"].AsString == testCoomNo;
            bool memoOk       = setPayload.Contains("c_order_m.coom_seller_memo");
            bool noNullFields = !setPayload.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 條件正確對應 CoomNo        : {(filterOk     ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ $set 包含 c_order_m.coom_seller_memo : {(memoOk    ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ $set 中無 null 值 (局部更新)        : {(noNullFields ? "PASS" : "FAIL")}");
        }

        // ─────────────────────────────────────────────
        // 輔助：從 MongoDBAddOrder 建立 OrderModel BsonDocument
        // ─────────────────────────────────────────────
        private static Model.MongoDB.AddOrderEvent.Order.OrderModel BuildOrderModelFromArgs(MongoDBAddOrder source)
        {
            var args = source.Args!;
            return new Model.MongoDB.AddOrderEvent.Order.OrderModel
            {
                PK      = args.CoomNo!,
                CoocNo  = args.CoocNo,
                C_Order_M = new Model.MongoDB.AddOrderEvent.Order.C_Order_M_Model
                {
                    CoomName                 = args.coom.CoomName,
                    CoomOrderDate            = args.coom.CoomOrderDate,
                    CoomStatus               = args.coom.CoomStatus,
                    CoomTempType             = args.coom.CoomTempType,
                    CoomCreateDatetime       = args.coom.CoomCreateDatetime,
                    CoomCuamCid              = args.coom.CoomCuamCid,
                    CoomSellerGoodsTotalAmt  = args.coom.CoomSellerGoodsTotalAmt,
                    CoomGoodsItemNum         = args.coom.CoomGoodsItemNum,
                    CoomGoodsTotalNum        = args.coom.CoomGoodsTotalNum,
                    CoomRcvTotalAmt          = args.coom.CoomRcvTotalAmt,
                    CoomCgdmId               = args.coom.CoomCgdmId
                },
                C_Order_C = new Model.MongoDB.AddOrderEvent.Order.C_Order_C_Model
                {
                    CoocPaymentType         = args.cooc.CoocPaymentType,
                    CoocPaymentPayDatetime  = args.cooc.CoocPaymentPayDatetime,
                    CoocDeliverMethod       = args.cooc.CoocDeliverMethod,
                    CoocOrdChannelKind      = args.cooc.CoocOrdChannelKind,
                    CoocMemSid              = args.cooc.CoocMemSid,
                    CoocCreateDatetime      = args.cooc.CoocCreateDatetime,
                    CoocOrdNameEnc          = args.cooc.CoocOrdNameEnc,
                    CoocRcvNameEnc          = args.cooc.CoocRcvNameEnc,
                    CoocRcvMobileEnc        = args.cooc.CoocRcvMobileEnc,
                    CoocPaymentDueday       = args.cooc.CoocPaymentDueday
                },
                C_Order_D = args.cood,
                C_Goods_Item = args.cgdi
            };
        }

        /// <summary>
        /// 將物件序列化為 BsonDocument，並移除所有 null 值欄位（模擬局部更新行為）
        /// </summary>
        private static BsonDocument BuildFlatSetDocument<T>(T obj) where T : class
        {
            if (obj == null) return new BsonDocument();
            var raw = obj.ToBsonDocument();
            return RemoveNullFields(raw);
        }

        /// <summary>
        /// 將物件序列化為 BsonDocument，加上點符號前綴，並移除 null 值欄位
        /// </summary>
        private static BsonDocument BuildFlatSetDocumentWithPrefix<T>(T? obj, string prefix) where T : class
        {
            if (obj == null) return new BsonDocument();
            var raw    = obj.ToBsonDocument();
            var result = new BsonDocument();
            foreach (var element in RemoveNullFields(raw))
            {
                result.Add($"{prefix}.{element.Name}", element.Value);
            }
            return result;
        }

        /// <summary>
        /// 遞迴移除 BsonDocument 中所有 null 值欄位
        /// </summary>
        private static BsonDocument RemoveNullFields(BsonDocument doc)
        {
            var result = new BsonDocument();
            foreach (var element in doc)
            {
                if (element.Value.IsBsonNull) continue;
                result.Add(element.Name, element.Value);
            }
            return result;
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 60));
        }

        private static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"--- {title} ---");
            Console.WriteLine();
        }
    }
}
