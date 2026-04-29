using CPF.Sandbox.Generators;
using CPF.Service.SendDataToMongoDB.Model.Order;
using CPF.Service.SendDataToElasticCloud.Model;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using System.Text.Json;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// 全流程沙盒場景執行器
    /// 資料產生 → 映射轉換 → 指令產出 → 驗證
    /// </summary>
    public static class SandboxRunner
    {
        private static readonly JsonWriterSettings _bsonPretty = new() { Indent = true };
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static void RunAll()
        {
            var gen = new ProductionDataGenerator();

            PrintHeader("CPF.Sandbox 全域模擬沙盒啟動");

            ScenarioAddOrder(gen);
            ScenarioElasticMapping(gen);
            ScenarioSellerMemo(gen);
            ScenarioChangePayType(gen);

            // S17 貨態更新場景
            ScenarioDeliveryCargoDynamics01(gen);
            ScenarioDeliveryCargoDynamics02(gen);
            ScenarioPaymentUpdate(gen);

            // S21 Elastic JSON 格式驗證
            ScenarioElasticJsonFormatVerification(gen);

            PrintHeader("CPF.Sandbox 全部場景完成");
        }

        // ─────────────────────────────────────────────
        // 場景 1：新增訂單 → MongoDB Insert BsonDocument
        // ─────────────────────────────────────────────
        private static void ScenarioAddOrder(ProductionDataGenerator gen)
        {
            PrintSection("場景 1：新增訂單 (AddOrder) → MongoDB Insert");

            OrderModel model = gen.GenerateMongoOrderModel();
            BsonDocument doc = model.ToBsonDocument();

            Console.WriteLine($"CoomNo  : {model.PK}");
            Console.WriteLine($"CoocNo  : {model.CoocNo}");
            Console.WriteLine($"明細筆數 : {model.C_Order_D?.Count ?? 0}");
            Console.WriteLine();
            Console.WriteLine("--- Insert BsonDocument ---");
            Console.WriteLine(doc.ToJson(_bsonPretty));

            // 驗證
            bool ok_coom_no  = doc.Contains("coom_no")   && !doc["coom_no"].IsBsonNull;
            bool ok_cooc_no  = doc.Contains("cooc_no")   && !doc["cooc_no"].IsBsonNull;
            bool ok_c_order_m= doc.Contains("c_order_m") && doc["c_order_m"].IsBsonDocument;
            bool ok_c_order_c= doc.Contains("c_order_c") && doc["c_order_c"].IsBsonDocument;
            bool ok_c_order_d= doc.Contains("c_order_d") && doc["c_order_d"].AsBsonArray.Count > 0;
            bool no_null_pk  = !doc.Any(e => e.Value.IsBsonNull); // 確認 [BsonIgnoreIfNull] 生效

            Console.WriteLine();
            Console.WriteLine($"✅ coom_no 存在且有值    : {Pass(ok_coom_no)}");
            Console.WriteLine($"✅ cooc_no 存在且有值    : {Pass(ok_cooc_no)}");
            Console.WriteLine($"✅ c_order_m 為子文件   : {Pass(ok_c_order_m)}");
            Console.WriteLine($"✅ c_order_c 為子文件   : {Pass(ok_c_order_c)}");
            Console.WriteLine($"✅ c_order_d 有明細資料 : {Pass(ok_c_order_d)}");
            Console.WriteLine($"✅ 無 null 頂層欄位      : {Pass(no_null_pk)}");
        }

        // ─────────────────────────────────────────────
        // 場景 2：ElasticSearch OrderInfoModel 映射
        // ─────────────────────────────────────────────
        private static void ScenarioElasticMapping(ProductionDataGenerator gen)
        {
            PrintSection("場景 2：ElasticSearch OrderInfoModel 映射");

            OrderInfoModel model = gen.GenerateElasticOrderInfoModel();
            string json = JsonSerializer.Serialize(model, _jsonOpts);

            Console.WriteLine($"CoomNo     : {model.CoomNo}");
            Console.WriteLine($"CoomStatus : {model.CoomStatus}");
            Console.WriteLine($"商品數量   : {model.CoodNames?.Length ?? 0}");
            Console.WriteLine();
            Console.WriteLine("--- ElasticSearch Document JSON ---");
            Console.WriteLine(json);

            bool ok_id     = !string.IsNullOrEmpty(model.Id);
            bool ok_names  = model.CoodNames?.Length > 0;
            bool ok_items  = model.CoodItems?.Length > 0;

            Console.WriteLine();
            Console.WriteLine($"✅ Id (= CoomNo) 有值    : {Pass(ok_id)}");
            Console.WriteLine($"✅ CoodNames 有商品名稱  : {Pass(ok_names)}");
            Console.WriteLine($"✅ CoodItems 有商品明細  : {Pass(ok_items)}");
        }

        // ─────────────────────────────────────────────
        // 場景 3：UpdateSellerMemo → $set 局部更新指令
        // ─────────────────────────────────────────────
        private static void ScenarioSellerMemo(ProductionDataGenerator gen)
        {
            PrintSection("場景 3：UpdateSellerMemo → $set 局部更新");

            const string testCoomNo = "CM4216179510575";
            var (coomNo, patch) = gen.GenerateSellerMemoPatch(testCoomNo);

            BsonDocument raw     = patch.ToBsonDocument();
            BsonDocument setDoc  = MongoRepository<BsonDocument>.FlattenBsonDocument(raw);
            BsonDocument filter  = new BsonDocument("coom_no", coomNo);
            BsonDocument cmd     = new BsonDocument("$set", setDoc);

            Console.WriteLine($"Filter  : {filter.ToJson()}");
            Console.WriteLine($"$set 指令:");
            Console.WriteLine(cmd.ToJson(_bsonPretty));

            bool ok_filter = filter["coom_no"].AsString == testCoomNo;
            bool ok_memo   = setDoc.Any(e => e.Name.Contains("coom_seller_memo"));
            bool no_null   = !setDoc.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 正確對應 coom_no          : {Pass(ok_filter)}");
            Console.WriteLine($"✅ $set 包含 coom_seller_memo 路徑  : {Pass(ok_memo)}");
            Console.WriteLine($"✅ $set 無 null 值 (局部更新規範)   : {Pass(no_null)}");
        }

        // ─────────────────────────────────────────────
        // 場景 4：ChangePayType → $set 局部更新指令
        // ─────────────────────────────────────────────
        private static void ScenarioChangePayType(ProductionDataGenerator gen)
        {
            PrintSection("場景 4：ChangePayType → $set 局部更新");

            const string testCoocNo = "CC2265481053604";
            var (coocNo, patch) = gen.GenerateChangePayTypePatch(testCoocNo);

            BsonDocument raw    = patch.ToBsonDocument();
            BsonDocument setDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(raw);
            BsonDocument filter = new BsonDocument("cooc_no", coocNo);
            BsonDocument cmd    = new BsonDocument("$set", setDoc);

            Console.WriteLine($"Filter  : {filter.ToJson()}");
            Console.WriteLine($"$set 指令:");
            Console.WriteLine(cmd.ToJson(_bsonPretty));

            bool ok_filter   = filter["cooc_no"].AsString == testCoocNo;
            bool ok_paytype  = setDoc.Any(e => e.Name.Contains("cooc_payment_type"));
            bool no_null     = !setDoc.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 正確對應 cooc_no           : {Pass(ok_filter)}");
            Console.WriteLine($"✅ $set 包含 cooc_payment_type 路徑  : {Pass(ok_paytype)}");
            Console.WriteLine($"✅ $set 無 null 值 (局部更新規範)    : {Pass(no_null)}");
        }

        // ─────────────────────────────────────────────
        // S17 場景 5：取號 (Delivery_01) → $set 初始化物流結構
        // 對應資料流：新增功能_貨態更新_取號_資料流
        // ─────────────────────────────────────────────
        private static void ScenarioDeliveryCargoDynamics01(ProductionDataGenerator gen)
        {
            PrintSection("S17 場景 5：取號 (Delivery_CargoDynamics_01) → $set 初始化");

            const string testCoomNo = "CM2604160395986";
            var (coomNo, patch) = gen.GenerateGetNumberPatch(testCoomNo);

            BsonDocument raw    = patch.ToBsonDocument();
            BsonDocument setDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(raw);
            BsonDocument filter = new BsonDocument("coom_no", coomNo);
            BsonDocument cmd    = new BsonDocument("$set", setDoc);

            Console.WriteLine($"Filter  : {filter.ToJson()}");
            Console.WriteLine($"$set 指令:");
            Console.WriteLine(cmd.ToJson(_bsonPretty));

            // 驗證點
            bool ok_filter      = filter["coom_no"].AsString == testCoomNo;
            bool ok_coom_status = setDoc.Any(e => e.Name == "c_order_m.coom_status");
            bool ok_esmm        = setDoc.Any(e => e.Name.StartsWith("e_shipment_m."));
            // 陣列欄位直接檢查是否存在（不是點符號路徑）
            bool ok_esml        = setDoc.Any(e => e.Name == "e_shipment_l");
            bool ok_esms        = setDoc.Any(e => e.Name == "e_shipment_s");
            bool no_null        = !setDoc.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 正確對應 coom_no           : {Pass(ok_filter)}");
            Console.WriteLine($"✅ $set 包含 c_order_m.coom_status   : {Pass(ok_coom_status)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_m (物流主檔) : {Pass(ok_esmm)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_l (貨態歷程) : {Pass(ok_esml)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_s (物流狀態) : {Pass(ok_esms)}");
            Console.WriteLine($"✅ $set 無 null 值                   : {Pass(no_null)}");
        }

        // ─────────────────────────────────────────────
        // S17 場景 6：寄貨 (Delivery_02) → $set + $push 增量更新
        // 對應資料流：新增功能_貨態更新_寄貨_資料流
        // ─────────────────────────────────────────────
        private static void ScenarioDeliveryCargoDynamics02(ProductionDataGenerator gen)
        {
            PrintSection("S17 場景 6：寄貨 (Delivery_CargoDynamics_02) → $set + $push");

            const string testCoomNo = "CM2604160395986";
            var (coomNo, patch) = gen.GenerateShippingPatch(testCoomNo);

            BsonDocument raw    = patch.ToBsonDocument();
            BsonDocument setDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(raw);
            BsonDocument filter = new BsonDocument("coom_no", coomNo);

            // 建立 $set + $push 合併指令
            var cmd = new BsonDocument();
            cmd.Add("$set", setDoc);

            // 模擬 $push - 從 patch 中提取陣列元素
            var pushDoc = new BsonDocument();
            
            // 從 patch 取得 E_Shipment_L 和 E_Shipment_S 的資料
            // 這裡模擬 $push 行為：追加新元素到陣列
            if (patch.E_Shipment_L != null && patch.E_Shipment_L.Count > 0)
            {
                var lastEsml = patch.E_Shipment_L.Last();
                var esmlDoc = new BsonDocument();
                if (!string.IsNullOrEmpty(lastEsml.EsmlEsmmStatus))
                    esmlDoc.Add("esml_esmm_status", lastEsml.EsmlEsmmStatus);
                if (lastEsml.EsmlStatusDatetime.HasValue)
                    esmlDoc.Add("esml_status_datetime", lastEsml.EsmlStatusDatetime.Value);
                
                pushDoc.Add("e_shipment_l", new BsonDocument("$each", new BsonArray { esmlDoc }));
            }

            if (patch.E_Shipment_S != null && patch.E_Shipment_S.Count > 0)
            {
                var lastEsms = patch.E_Shipment_S.Last();
                var esmsDoc = new BsonDocument();
                if (!string.IsNullOrEmpty(lastEsms.EsmsDlvStatusNo))
                    esmsDoc.Add("esms_dlv_status_no", lastEsms.EsmsDlvStatusNo);
                if (lastEsms.EsmsStatusDatetime.HasValue)
                    esmsDoc.Add("esms_status_datetime", lastEsms.EsmsStatusDatetime.Value);
                
                pushDoc.Add("e_shipment_s", new BsonDocument("$each", new BsonArray { esmsDoc }));
            }

            if (pushDoc.ElementCount > 0)
            {
                cmd.Add("$push", pushDoc);
            }

            Console.WriteLine($"Filter  : {filter.ToJson()}");
            Console.WriteLine($"$set + $push 指令:");
            Console.WriteLine(cmd.ToJson(_bsonPretty));

            // 驗證點
            bool ok_filter      = filter["coom_no"].AsString == testCoomNo;
            bool ok_coom_status = setDoc.Any(e => e.Name == "c_order_m.coom_status");
            bool ok_esmm_status = setDoc.Any(e => e.Name == "e_shipment_m.esmm_status");
            bool ok_push_esml   = cmd.Contains("$push") && cmd["$push"].AsBsonDocument.Contains("e_shipment_l");
            bool ok_push_esms   = cmd.Contains("$push") && cmd["$push"].AsBsonDocument.Contains("e_shipment_s");
            bool no_null        = !setDoc.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 正確對應 coom_no           : {Pass(ok_filter)}");
            Console.WriteLine($"✅ $set 包含 c_order_m.coom_status   : {Pass(ok_coom_status)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_m.esmm_status: {Pass(ok_esmm_status)}");
            Console.WriteLine($"✅ $push 追加 e_shipment_l           : {Pass(ok_push_esml)}");
            Console.WriteLine($"✅ $push 追加 e_shipment_s           : {Pass(ok_push_esms)}");
            Console.WriteLine($"✅ $set 無 null 值                   : {Pass(no_null)}");
        }

        // ─────────────────────────────────────────────
        // S17 場景 7：付款資訊更新 → $set 初始化物流結構
        // 對應資料流：新增功能_貨態更新_更新付款資訊_資料流
        // ─────────────────────────────────────────────
        private static void ScenarioPaymentUpdate(ProductionDataGenerator gen)
        {
            PrintSection("S17 場景 7：付款資訊更新 (Update_Payment_Info) → $set");

            const string testCoomNo = "CM2604160395986";
            var (coomNo, patch) = gen.GeneratePaymentUpdatePatch(testCoomNo);

            BsonDocument raw    = patch.ToBsonDocument();
            BsonDocument setDoc = MongoRepository<BsonDocument>.FlattenBsonDocument(raw);
            BsonDocument filter = new BsonDocument("coom_no", coomNo);
            BsonDocument cmd    = new BsonDocument("$set", setDoc);

            Console.WriteLine($"Filter  : {filter.ToJson()}");
            Console.WriteLine($"$set 指令:");
            Console.WriteLine(cmd.ToJson(_bsonPretty));

            // 驗證點
            bool ok_filter      = filter["coom_no"].AsString == testCoomNo;
            bool ok_coom_status = setDoc.Any(e => e.Name == "c_order_m.coom_status");
            bool ok_esmm        = setDoc.Any(e => e.Name.StartsWith("e_shipment_m."));
            // 陣列欄位直接檢查是否存在（不是點符號路徑）
            bool ok_esml        = setDoc.Any(e => e.Name == "e_shipment_l");
            bool ok_esms        = setDoc.Any(e => e.Name == "e_shipment_s");
            bool no_null        = !setDoc.Any(e => e.Value.IsBsonNull);

            Console.WriteLine();
            Console.WriteLine($"✅ Filter 正確對應 coom_no           : {Pass(ok_filter)}");
            Console.WriteLine($"✅ $set 包含 c_order_m.coom_status   : {Pass(ok_coom_status)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_m (物流主檔) : {Pass(ok_esmm)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_l (貨態歷程) : {Pass(ok_esml)}");
            Console.WriteLine($"✅ $set 包含 e_shipment_s (物流狀態) : {Pass(ok_esms)}");
            Console.WriteLine($"✅ $set 無 null 值                   : {Pass(no_null)}");
        }

        // ─────────────────────────────────────────────
        // S21 場景：Elastic JSON 格式驗證 (Sample_Data 比對)
        // 對應資料流：新增功能_貨態更新_寄貨_資料流.txt / 取號_資料流.txt
        // ─────────────────────────────────────────────
        private static void ScenarioElasticJsonFormatVerification(ProductionDataGenerator gen)
        {
            PrintSection("S21 場景：Elastic JSON 格式驗證 (Sample_Data 比對)");

            // 測試 1：寄貨更新後 JSON
            Console.WriteLine("=== [寄貨更新後] Elastic JSON 輸出 ===");
            var shippingModel = gen.GenerateElasticShippingUpdateModel();
            string shippingJson = JsonSerializer.Serialize(shippingModel, _jsonOpts);
            Console.WriteLine(shippingJson);

            // 測試 2：取號更新後 JSON
            Console.WriteLine();
            Console.WriteLine("=== [取號更新後] Elastic JSON 輸出 ===");
            var getNumberModel = gen.GenerateElasticGetNumberUpdateModel();
            string getNumberJson = JsonSerializer.Serialize(getNumberModel, _jsonOpts);
            Console.WriteLine(getNumberJson);

            // 驗證關鍵欄位 (使用 JSON 解析)
            Console.WriteLine();
            Console.WriteLine("=== 驗證結果 ===");
            
            // 寄貨驗證
            var shippingDoc = JsonDocument.Parse(shippingJson);
            var shippingRoot = shippingDoc.RootElement;
            
            bool shippingCoomStatus = shippingRoot.TryGetProperty("coom_status", out var sCoomStatus) && sCoomStatus.GetString() == "30";
            bool shippingEsmmStatus = shippingRoot.TryGetProperty("esmm_status", out var sEsmmStatus) && sEsmmStatus.GetString() == "10";
            bool shippingShipNo = shippingRoot.TryGetProperty("esmm_ship_no", out var sShipNo) && sShipNo.GetString() == "D88032120964";
            bool shippingDateTime = shippingRoot.TryGetProperty("esml_status_shipping_datetime", out var sDateTime);
            bool shippingCoodNames = shippingRoot.TryGetProperty("cood_name", out var sCoodNames) && sCoodNames.ValueKind == JsonValueKind.Array;
            bool shippingCoodItems = shippingRoot.TryGetProperty("cood_items", out var sCoodItems) && sCoodItems.ValueKind == JsonValueKind.Array;
            bool shippingCoomRcvTotalAmt = shippingRoot.TryGetProperty("coom_rcv_totalamt", out var sCoomRcv) && sCoomRcv.GetInt32() == 138;
            bool shippingEsmmRcvTotalAmt = shippingRoot.TryGetProperty("esmm_rcv_total_amt", out var sEsmmRcv) && sEsmmRcv.GetInt32() == 138;
            
            Console.WriteLine($"[寄貨] coom_status=30: {(shippingCoomStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esmm_status=10: {(shippingEsmmStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esmm_ship_no=D88032120964: {(shippingShipNo ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esml_status_shipping_datetime: {(shippingDateTime ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] cood_name 陣列: {(shippingCoodNames ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] cood_items 陣列: {(shippingCoodItems ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] coom_rcv_totalamt=138: {(shippingCoomRcvTotalAmt ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esmm_rcv_total_amt=138: {(shippingEsmmRcvTotalAmt ? "✅ PASS" : "❌ FAIL")}");

            // 取號驗證
            var getNumberDoc = JsonDocument.Parse(getNumberJson);
            var getNumberRoot = getNumberDoc.RootElement;
            
            bool getNumberCoomStatus = getNumberRoot.TryGetProperty("coom_status", out var gCoomStatus) && gCoomStatus.GetString() == "20";
            bool getNumberEsmmStatus = getNumberRoot.TryGetProperty("esmm_status", out var gEsmmStatus) && gEsmmStatus.GetString() == "01";
            bool getNumberShipNo = getNumberRoot.TryGetProperty("esmm_ship_no", out var gShipNo) && gShipNo.GetString() == "D88032120964";
            bool getNumberCoodNames = getNumberRoot.TryGetProperty("cood_name", out var gCoodNames) && gCoodNames.ValueKind == JsonValueKind.Array;
            bool getNumberCoodItems = getNumberRoot.TryGetProperty("cood_items", out var gCoodItems) && gCoodItems.ValueKind == JsonValueKind.Array;
            
            Console.WriteLine($"[取號] coom_status=20: {(getNumberCoomStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] esmm_status=01: {(getNumberEsmmStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] esmm_ship_no=D88032120964: {(getNumberShipNo ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] cood_name 陣列: {(getNumberCoodNames ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] cood_items 陣列: {(getNumberCoodItems ? "✅ PASS" : "❌ FAIL")}");
        }

        // ─────────────────────────────────────────────
        // 輔助：Pass 標記
        // ─────────────────────────────────────────────
        private static string Pass(bool ok) => ok ? "PASS ✅" : "FAIL ❌";

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
            Console.WriteLine($"▶ {title}");
            Console.WriteLine(new string('-', 50));
        }
    }
}
