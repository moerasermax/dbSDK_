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
