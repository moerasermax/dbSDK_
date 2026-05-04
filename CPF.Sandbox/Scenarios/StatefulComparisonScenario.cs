using CPF.Sandbox.Generators;
using CPF.Service.SendDataToMongoDB.Model.Order;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using CPF.Sandbox.Mocks;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// S13 狀態模擬場景
    /// Insert → Read(V1) → Update(含null混入) → Read(V2) → 對比報告
    /// </summary>
    public static class StatefulComparisonScenario
    {
        private static readonly JsonWriterSettings _pretty = new() { Indent = true };

        // ═══════════════════════════════════════════════════════════
        // S13 正式方法：步進驗證 (含 null 混入測試)
        // ═══════════════════════════════════════════════════════════
        public static void RunStepByStepVerification()
        {
            PrintHeader("S13 步進驗證：Insert → Read(V1) → Update(含null混入) → Read(V2) → 對比報告");

            var gen  = new ProductionDataGenerator();
            var repo = new MockOrderRepository();

            // ── Step 1：Insert ──────────────────────────────────────
            PrintStep("Step 1：Insert 隨機生產級訂單");

            OrderModel original    = gen.GenerateMongoOrderModel();
            BsonDocument insertDoc = original.ToBsonDocument();
            string coomNo          = original.PK!;
            string filterJson      = $"{{\"coom_no\":\"{coomNo}\"}}";

            var insertResult = repo.Insert(insertDoc).Result;
            Console.WriteLine(insertResult.IsSuccess
                ? $"  ✅ {insertResult.Msg}"
                : $"  ❌ {insertResult.Msg}");

            // ── Step 2：Read V1 ─────────────────────────────────────
            PrintStep("Step 2：Read V1（初始狀態）");

            var v1Result = repo.GetData(filterJson).Result;
            BsonDocument v1 = BsonDocument.Parse(v1Result.DataJson);
            PrintKeyFields("V1", v1);
            Console.WriteLine();
            Console.WriteLine("  完整 BsonDocument V1：");
            Console.WriteLine(v1.ToJson(_pretty));

            // ── Step 3：Update（故意混入 null 值）──────────────────
            PrintStep("Step 3：Update（故意混入 null 值，驗證 null 排除邏輯）");

            string newMemo = $"步進備註_{DateTime.Now:HHmmss}";
            string newPay  = "3";

            // 建立含 null 的 $set 指令：
            //   - coom_seller_memo  → 有值（應更新）
            //   - cooc_payment_type → 有值（應更新）
            //   - coom_name         → null（應被排除，不覆蓋舊值）
            //   - coom_cgdm_id      → null（應被排除，不覆蓋舊值）
            var rawSetDoc = new BsonDocument
            {
                { "c_order_m.coom_seller_memo",  newMemo        },
                { "c_order_c.cooc_payment_type", newPay         },
                { "c_order_m.coom_name",         BsonNull.Value },  // ← 故意 null
                { "c_order_m.coom_cgdm_id",      BsonNull.Value },  // ← 故意 null
            };

            Console.WriteLine("  $set 指令（含 null 欄位）：");
            Console.WriteLine(rawSetDoc.ToJson(_pretty));
            Console.WriteLine();
            Console.WriteLine("  預期：null 欄位應被 MockOrderRepository 自動排除，不覆蓋舊值。");

            var updateResult = repo.Update(filterJson, rawSetDoc).Result;
            Console.WriteLine(updateResult.IsSuccess
                ? $"  ✅ {updateResult.Msg}"
                : $"  ❌ {updateResult.Msg}");

            // ── Step 4：Read V2 ─────────────────────────────────────
            PrintStep("Step 4：Read V2（更新後狀態）");

            var v2Result = repo.GetData(filterJson).Result;
            BsonDocument v2 = BsonDocument.Parse(v2Result.DataJson);
            PrintKeyFields("V2", v2);
            Console.WriteLine();
            Console.WriteLine("  完整 BsonDocument V2：");
            Console.WriteLine(v2.ToJson(_pretty));

            // ── Step 5：對比報告 ────────────────────────────────────
            PrintStep("Step 5：對比報告");

            string v1_memo   = GetNested(v1, "c_order_m", "coom_seller_memo");
            string v2_memo   = GetNested(v2, "c_order_m", "coom_seller_memo");
            string v1_pay    = GetNested(v1, "c_order_c", "cooc_payment_type");
            string v2_pay    = GetNested(v2, "c_order_c", "cooc_payment_type");
            string v1_name   = GetNested(v1, "c_order_m", "coom_name");
            string v2_name   = GetNested(v2, "c_order_m", "coom_name");
            string v1_cgdmId = GetNested(v1, "c_order_m", "coom_cgdm_id");
            string v2_cgdmId = GetNested(v2, "c_order_m", "coom_cgdm_id");
            string v1_status = GetNested(v1, "c_order_m", "coom_status");
            string v2_status = GetNested(v2, "c_order_m", "coom_status");
            string v1_amt    = GetNested(v1, "c_order_m", "coom_rcv_total_amt");
            string v2_amt    = GetNested(v2, "c_order_m", "coom_rcv_total_amt");
            int    v1_cood   = v1.Contains("c_order_d") ? v1["c_order_d"].AsBsonArray.Count : 0;
            int    v2_cood   = v2.Contains("c_order_d") ? v2["c_order_d"].AsBsonArray.Count : 0;

            Console.WriteLine();
            Console.WriteLine($"  {"欄位",-42} {"V1 (更新前)",-22} {"V2 (更新後)",-22} {"預期",-8} 結果");
            Console.WriteLine($"  {new string('-', 105)}");

            PrintRow("c_order_m.coom_seller_memo  [應變更]",    v1_memo,   v2_memo,   v2_memo == newMemo,        "已更新");
            PrintRow("c_order_c.cooc_payment_type [應變更]",    v1_pay,    v2_pay,    v2_pay == newPay,          "已更新");
            PrintRow("c_order_m.coom_name         [null→不變]", v1_name,   v2_name,   v1_name == v2_name,        "保留");
            PrintRow("c_order_m.coom_cgdm_id      [null→不變]", v1_cgdmId, v2_cgdmId, v1_cgdmId == v2_cgdmId,   "保留");
            PrintRow("c_order_m.coom_status       [未觸及→不變]",v1_status, v2_status, v1_status == v2_status,   "保留");
            PrintRow("c_order_m.coom_rcv_total_amt[未觸及→不變]",v1_amt,    v2_amt,    v1_amt == v2_amt,          "保留");
            PrintRow($"c_order_d 明細筆數          [未觸及→不變]",
                v1_cood.ToString(), v2_cood.ToString(), v1_cood == v2_cood, "保留");

            bool allPass =
                v2_memo == newMemo &&
                v2_pay  == newPay  &&
                v1_name == v2_name &&
                v1_cgdmId == v2_cgdmId &&
                v1_status == v2_status &&
                v1_amt == v2_amt &&
                v1_cood == v2_cood;

            Console.WriteLine();
            Console.WriteLine(allPass
                ? "  🎉 全部驗證通過！點符號路徑更新與 null 排除邏輯均正確。"
                : "  ⚠️  部分驗證失敗，請檢查更新邏輯。");
        }

        // ─── 輔助方法 ─────────────────────────────────────────────

        private static void PrintKeyFields(string label, BsonDocument doc)
        {
            Console.WriteLine($"  [{label}] coom_status        : {GetNested(doc, "c_order_m", "coom_status")}");
            Console.WriteLine($"  [{label}] cooc_payment_type  : {GetNested(doc, "c_order_c", "cooc_payment_type")}");
            Console.WriteLine($"  [{label}] coom_seller_memo   : {GetNested(doc, "c_order_m", "coom_seller_memo")}");
            Console.WriteLine($"  [{label}] coom_name          : {GetNested(doc, "c_order_m", "coom_name")}");
            Console.WriteLine($"  [{label}] coom_rcv_total_amt : {GetNested(doc, "c_order_m", "coom_rcv_total_amt")}");
            Console.WriteLine($"  [{label}] c_order_d 筆數     : {(doc.Contains("c_order_d") ? doc["c_order_d"].AsBsonArray.Count.ToString() : "0")}");
        }

        private static void PrintRow(string field, string v1, string v2, bool pass, string expect)
        {
            string icon = pass ? "✅" : "❌";
            Console.WriteLine($"  {field,-42} {v1,-22} {v2,-22} {expect,-8} {icon} {(pass ? "PASS" : "FAIL")}");
        }

        private static string GetNested(BsonDocument doc, string parent, string field)
        {
            if (!doc.Contains(parent) || !doc[parent].IsBsonDocument) return "(無)";
            var sub = doc[parent].AsBsonDocument;
            if (!sub.Contains(field)) return "(無)";
            var val = sub[field];
            return val.IsBsonNull ? "(null)" : val.ToString() ?? "(無)";
        }

        private static string Pass(bool ok) => ok ? "PASS ✅" : "FAIL ❌";

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
