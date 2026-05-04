using CPF.Sandbox.Generators;
using CPF.Service.SendDataToMongoDB.Model.Order;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using CPF.Sandbox.Mocks;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// S14 賣家取號業務模擬 (Status 20)
    /// 對應資料流：UpdateSellerGetNumberEvent
    /// 驗證物流模組 (e_shipment_m/l/s) 動態掛載與訂單狀態更新
    /// </summary>
    public static class SellerGetNumberScenario
    {
        private static readonly JsonWriterSettings _pretty = new() { Indent = true };

        public static void RunSellerGetNumberSimulation()
        {
            PrintHeader("S14 賣家取號模擬 (Status 20)：物流模組掛載驗證");

            var gen  = new ProductionDataGenerator();
            var repo = new MockOrderRepository();

            // ── Step 1：Insert 初始訂單（僅含 c_order_m/c/d，無物流模組）──
            PrintStep("Step 1：Insert 初始訂單（Status=10，無物流模組）");

            OrderModel initial = gen.GenerateMongoOrderModel();
            // 確保初始狀態沒有物流模組
            initial.E_Shipment_M = null;
            initial.E_Shipment_L = null;
            initial.E_Shipment_S = null;

            BsonDocument insertDoc = initial.ToBsonDocument();
            string coomNo          = initial.PK!;
            string filterJson      = $"{{\"coom_no\":\"{coomNo}\"}}";

            var insertResult = repo.Insert(insertDoc).Result;
            Console.WriteLine(insertResult.IsSuccess
                ? $"  ✅ {insertResult.Msg}"
                : $"  ❌ {insertResult.Msg}");

            // ── Step 2：Read V1（初始狀態）──────────────────────────
            PrintStep("Step 2：Read V1（初始狀態）");

            var v1Result = repo.GetData(filterJson).Result;
            BsonDocument v1 = BsonDocument.Parse(v1Result.DataJson);

            Console.WriteLine($"  coom_status      : {GetNested(v1, "c_order_m", "coom_status")}");
            Console.WriteLine($"  coom_name        : {GetNested(v1, "c_order_m", "coom_name")}");
            Console.WriteLine($"  coom_rcv_total_amt: {GetNested(v1, "c_order_m", "coom_rcv_total_amt")}");
            Console.WriteLine($"  c_order_d 筆數   : {GetArrayCount(v1, "c_order_d")}");
            Console.WriteLine($"  e_shipment_m     : {(v1.Contains("e_shipment_m") ? "存在" : "不存在（預期）")}");
            Console.WriteLine($"  e_shipment_l     : {(v1.Contains("e_shipment_l") ? "存在" : "不存在（預期）")}");
            Console.WriteLine($"  e_shipment_s     : {(v1.Contains("e_shipment_s") ? "存在" : "不存在（預期）")}");

            // ── Step 3：執行 Status 20 更新（掛載物流模組）──────────
            PrintStep("Step 3：執行 UpdateSellerGetNumberEvent（Status 20）");

            var (_, patch) = gen.GenerateStatus20Patch(coomNo);

            // S15.1：直接傳入原始 BsonDocument，MockOrderRepository.Update 會呼叫
            // MongoRepository.FlattenBsonDocument() 進行扁平化，確保 100% SDK 邏輯
            BsonDocument patchDoc = patch.ToBsonDocument();

            Console.WriteLine("  原始 patch BsonDocument（傳入 MockOrderRepository.Update）：");
            Console.WriteLine(patchDoc.ToJson(_pretty));

            var updateResult = repo.Update(filterJson, patchDoc).Result;
            Console.WriteLine(updateResult.IsSuccess
                ? $"  ✅ {updateResult.Msg}"
                : $"  ❌ {updateResult.Msg}");

            // ── Step 4：Read V2（更新後狀態）────────────────────────
            PrintStep("Step 4：Read V2（更新後狀態）");

            var v2Result = repo.GetData(filterJson).Result;
            BsonDocument v2 = BsonDocument.Parse(v2Result.DataJson);

            Console.WriteLine($"  coom_status      : {GetNested(v2, "c_order_m", "coom_status")}");
            Console.WriteLine($"  coom_name        : {GetNested(v2, "c_order_m", "coom_name")}");
            Console.WriteLine($"  coom_rcv_total_amt: {GetNested(v2, "c_order_m", "coom_rcv_total_amt")}");
            Console.WriteLine($"  c_order_d 筆數   : {GetArrayCount(v2, "c_order_d")}");
            Console.WriteLine($"  e_shipment_m     : {(v2.Contains("e_shipment_m") ? "存在 ✅" : "不存在 ❌")}");
            Console.WriteLine($"  e_shipment_l 筆數: {GetArrayCount(v2, "e_shipment_l")}");
            Console.WriteLine($"  e_shipment_s 筆數: {GetArrayCount(v2, "e_shipment_s")}");

            if (v2.Contains("e_shipment_m") && v2["e_shipment_m"].IsBsonDocument)
            {
                var esmm = v2["e_shipment_m"].AsBsonDocument;
                Console.WriteLine($"  esmm_no          : {GetField(esmm, "esmm_no")}");
                Console.WriteLine($"  esmm_ship_no     : {GetField(esmm, "esmm_ship_no")}");
                Console.WriteLine($"  esmm_status      : {GetField(esmm, "esmm_status")}");
                Console.WriteLine($"  esmm_ship_method : {GetField(esmm, "esmm_ship_method")}");
            }

            // ── Step 5：對比報告 ─────────────────────────────────────
            PrintStep("Step 5：對比報告");

            string v1_status  = GetNested(v1, "c_order_m", "coom_status");
            string v2_status  = GetNested(v2, "c_order_m", "coom_status");
            string v1_name    = GetNested(v1, "c_order_m", "coom_name");
            string v2_name    = GetNested(v2, "c_order_m", "coom_name");
            string v1_amt     = GetNested(v1, "c_order_m", "coom_rcv_total_amt");
            string v2_amt     = GetNested(v2, "c_order_m", "coom_rcv_total_amt");
            int    v1_cood    = GetArrayCount(v1, "c_order_d");
            int    v2_cood    = GetArrayCount(v2, "c_order_d");
            bool   v2_hasEsmm = v2.Contains("e_shipment_m") && v2["e_shipment_m"].IsBsonDocument;
            int    v2_esmlCnt = GetArrayCount(v2, "e_shipment_l");
            int    v2_esmsCnt = GetArrayCount(v2, "e_shipment_s");
            string v2_esmmStatus = v2_hasEsmm ? GetField(v2["e_shipment_m"].AsBsonDocument, "esmm_status") : "(無)";

            Console.WriteLine();
            Console.WriteLine($"  {"驗證項目",-45} {"V1",-15} {"V2",-15} 結果");
            Console.WriteLine($"  {new string('-', 90)}");

            PrintRow("c_order_m.coom_status [應從(無)→20]",
                v1_status, v2_status, v2_status == "20", "已更新");
            PrintRow("c_order_m.coom_name   [應保留不變]",
                v1_name, v2_name, v1_name == v2_name, "保留");
            PrintRow("c_order_m.coom_rcv_total_amt [應保留不變]",
                v1_amt, v2_amt, v1_amt == v2_amt, "保留");
            PrintRow($"c_order_d 明細筆數    [應保留不變]",
                v1_cood.ToString(), v2_cood.ToString(), v1_cood == v2_cood, "保留");
            PrintRow("e_shipment_m          [應新增]",
                "不存在", v2_hasEsmm ? "存在" : "不存在", v2_hasEsmm, "新增");
            PrintRow("e_shipment_m.esmm_status [應為01]",
                "(無)", v2_esmmStatus, v2_esmmStatus == "01", "正確");
            PrintRow($"e_shipment_l 筆數     [應=1]",
                "0", v2_esmlCnt.ToString(), v2_esmlCnt == 1, "初始化");
            PrintRow($"e_shipment_s 筆數     [應=1]",
                "0", v2_esmsCnt.ToString(), v2_esmsCnt == 1, "初始化");

            bool allPass =
                v2_status == "20" &&
                v1_name == v2_name &&
                v1_amt == v2_amt &&
                v1_cood == v2_cood &&
                v2_hasEsmm &&
                v2_esmmStatus == "01" &&
                v2_esmlCnt == 1 &&
                v2_esmsCnt == 1;

            Console.WriteLine();
            Console.WriteLine(allPass
                ? "  🎉 全部驗證通過！Status 20 物流模組掛載邏輯正確。"
                : "  ⚠️  部分驗證失敗，請檢查更新邏輯。");

            // ── Step 6：完整 BsonDocument 前後對比 ──────────────────
            PrintStep("Step 6：完整 BsonDocument 前後對比");
            Console.WriteLine("【V1 更新前】");
            Console.WriteLine(v1.ToJson(_pretty));
            Console.WriteLine();
            Console.WriteLine("【V2 更新後】");
            Console.WriteLine(v2.ToJson(_pretty));
        }

        // ─── 輔助方法 ─────────────────────────────────────────────

        private static string GetNested(BsonDocument doc, string parent, string field)
        {
            if (!doc.Contains(parent) || !doc[parent].IsBsonDocument) return "(無)";
            var sub = doc[parent].AsBsonDocument;
            if (!sub.Contains(field)) return "(無)";
            var val = sub[field];
            return val.IsBsonNull ? "(null)" : val.ToString() ?? "(無)";
        }

        private static string GetField(BsonDocument doc, string field)
        {
            if (!doc.Contains(field)) return "(無)";
            var val = doc[field];
            return val.IsBsonNull ? "(null)" : val.ToString() ?? "(無)";
        }

        private static int GetArrayCount(BsonDocument doc, string field)
        {
            if (!doc.Contains(field)) return 0;
            var val = doc[field];
            return val.IsBsonArray ? val.AsBsonArray.Count : 0;
        }

        private static void PrintRow(string field, string v1, string v2, bool pass, string expect)
        {
            string icon = pass ? "✅" : "❌";
            Console.WriteLine($"  {field,-45} {v1,-15} {v2,-15} {expect,-8} {icon} {(pass ? "PASS" : "FAIL")}");
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 70));
        }

        private static void PrintStep(string title)
        {
            Console.WriteLine();
            Console.WriteLine($"▶ {title}");
            Console.WriteLine(new string('-', 55));
        }
    }
}
