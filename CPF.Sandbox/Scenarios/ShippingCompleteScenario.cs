using CPF.Sandbox.Generators;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using CPF.Sandbox.Mocks;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// S15 寄貨完成模擬 (Status 30)
    /// 對應資料流：UpdateShippingCompleteEvent
    /// 驗證 $set (狀態更新) + $push (陣列追加) 並行操作的正確性
    /// </summary>
    public static class ShippingCompleteScenario
    {
        private static readonly JsonWriterSettings _pretty = new() { Indent = true };

        public static void RunShippingCompleteSimulation()
        {
            PrintHeader("S15 寄貨完成模擬 (Status 30)：$set + $push 並行驗證");

            var gen  = new ProductionDataGenerator();
            var repo = new MockOrderRepository();

            // ── Step 1：建立 Status 20 初始狀態（沿用 S14 產生器）──
            PrintStep("Step 1：建立 Status 20 初始狀態（e_shipment_l 筆數=1）");

            var initialModel = gen.GenerateMongoOrderModel();
            initialModel.E_Shipment_M = null;
            initialModel.E_Shipment_L = null;
            initialModel.E_Shipment_S = null;

            string coomNo     = initialModel.PK!;
            string filterJson = $"{{\"coom_no\":\"{coomNo}\"}}";

            // Insert 基礎訂單
            var insertResult = repo.Insert(initialModel.ToBsonDocument()).Result;
            Console.WriteLine(insertResult.IsSuccess ? $"  ✅ {insertResult.Msg}" : $"  ❌ {insertResult.Msg}");

            // 套用 Status 20 物流掛載
            // S15.1：直接傳入原始 BsonDocument，MockOrderRepository.Update 呼叫 SDK 扁平化
            var (_, s20patch) = gen.GenerateStatus20Patch(coomNo);
            repo.Update(filterJson, s20patch.ToBsonDocument()).Wait();
            Console.WriteLine($"  ✅ Status 20 物流模組掛載完成");

            // ── Step 2：Read V1（Status 20 狀態）────────────────────
            PrintStep("Step 2：Read V1（Status 20 初始狀態）");

            var v1Result = repo.GetData(filterJson).Result;
            BsonDocument v1 = BsonDocument.Parse(v1Result.DataJson);

            string v1_coomStatus  = GetNested(v1, "c_order_m", "coom_status");
            string v1_esmmStatus  = GetNested(v1, "e_shipment_m", "esmm_status");
            int    v1_esmlCount   = GetArrayCount(v1, "e_shipment_l");
            int    v1_esmsCount   = GetArrayCount(v1, "e_shipment_s");

            Console.WriteLine($"  coom_status      : {v1_coomStatus}");
            Console.WriteLine($"  esmm_status      : {v1_esmmStatus}");
            Console.WriteLine($"  e_shipment_l 筆數: {v1_esmlCount}  ← 預期=1");
            Console.WriteLine($"  e_shipment_s 筆數: {v1_esmsCount}  ← 預期=1");

            // ── Step 3：執行 Status 30 更新（$set + $push）──────────
            PrintStep("Step 3：執行 Status 30 更新（$set 狀態 + $push 追加歷程）");

            Status30Patch s30 = gen.GenerateStatus30Patch(coomNo);

            Console.WriteLine("  $set 指令：");
            Console.WriteLine(s30.SetFields.ToJson(_pretty));
            Console.WriteLine("  $push esml 新增元素：");
            Console.WriteLine(s30.PushEsml.ToJson(_pretty));
            Console.WriteLine("  $push esms 新增元素：");
            Console.WriteLine(s30.PushEsms.ToJson(_pretty));

            // 先執行 $set
            var setResult = repo.Update(filterJson, s30.SetFields).Result;
            Console.WriteLine(setResult.IsSuccess
                ? $"  ✅ $set 完成：{setResult.Msg}"
                : $"  ❌ $set 失敗：{setResult.Msg}");

            // 再執行 $push（分別追加 esml 與 esms）
            var pushDoc = new BsonDocument
            {
                { "e_shipment_l", s30.PushEsml },
                { "e_shipment_s", s30.PushEsms }
            };
            var pushResult = repo.Push(filterJson, pushDoc).Result;
            Console.WriteLine(pushResult.IsSuccess
                ? $"  ✅ $push 完成：{pushResult.Msg}"
                : $"  ❌ $push 失敗：{pushResult.Msg}");

            // ── Step 4：Read V2（Status 30 最終狀態）────────────────
            PrintStep("Step 4：Read V2（Status 30 最終狀態）");

            var v2Result = repo.GetData(filterJson).Result;
            BsonDocument v2 = BsonDocument.Parse(v2Result.DataJson);

            string v2_coomStatus = GetNested(v2, "c_order_m", "coom_status");
            string v2_esmmStatus = GetNested(v2, "e_shipment_m", "esmm_status");
            int    v2_esmlCount  = GetArrayCount(v2, "e_shipment_l");
            int    v2_esmsCount  = GetArrayCount(v2, "e_shipment_s");

            Console.WriteLine($"  coom_status      : {v2_coomStatus}");
            Console.WriteLine($"  esmm_status      : {v2_esmmStatus}");
            Console.WriteLine($"  e_shipment_l 筆數: {v2_esmlCount}  ← 預期=2");
            Console.WriteLine($"  e_shipment_s 筆數: {v2_esmsCount}  ← 預期=2");

            // 印出 esml 陣列內容，確認兩筆都在
            // 印出 esml 陣列內容，確認兩筆都在
            if (v2.Contains("e_shipment_l") && v2["e_shipment_l"].IsBsonArray)
            {
                Console.WriteLine("  e_shipment_l 內容：");
                foreach (var item in v2["e_shipment_l"].AsBsonArray)
                    Console.WriteLine($"    → {item.ToJson()}");
            }

            // ── Step 5：對比報告 ─────────────────────────────────────
            PrintStep("Step 5：對比報告");

            // 驗證 esml 陣列同時包含 "01" 與 "10"
            bool hasEsml01 = false, hasEsml10 = false;
            bool hasEsms1001 = false, hasEsms1A01 = false;

            if (v2.Contains("e_shipment_l") && v2["e_shipment_l"].IsBsonArray)
            {
                foreach (var item in v2["e_shipment_l"].AsBsonArray)
                {
                    if (!item.IsBsonDocument) continue;
                    var d = item.AsBsonDocument;
                    if (d.Contains("esml_esmm_status"))
                    {
                        // S15.3：使用安全轉換，相容 BsonDateTime 和 BsonString
                        var val = d["esml_esmm_status"];
                        var s = val.BsonType == BsonType.String ? val.AsString : val.ToString();
                        if (s == "01") hasEsml01 = true;
                        if (s == "10") hasEsml10 = true;
                    }
                }
            }

            if (v2.Contains("e_shipment_s") && v2["e_shipment_s"].IsBsonArray)
            {
                foreach (var item in v2["e_shipment_s"].AsBsonArray)
                {
                    if (!item.IsBsonDocument) continue;
                    var d = item.AsBsonDocument;
                    if (d.Contains("esms_dlv_status_no"))
                    {
                        // S15.3：使用安全轉換，相容 BsonDateTime 和 BsonString
                        var val = d["esms_dlv_status_no"];
                        var s = val.BsonType == BsonType.String ? val.AsString : val.ToString();
                        if (s == "1001") hasEsms1001 = true;
                        if (s == "1A01") hasEsms1A01 = true;
                    }
                }
            }

            string v1_name = GetNested(v1, "c_order_m", "coom_name");
            string v2_name = GetNested(v2, "c_order_m", "coom_name");
            string v1_amt  = GetNested(v1, "c_order_m", "coom_rcv_total_amt");
            string v2_amt  = GetNested(v2, "c_order_m", "coom_rcv_total_amt");

            Console.WriteLine();
            Console.WriteLine($"  {"驗證項目",-48} {"V1",-10} {"V2",-10} 結果");
            Console.WriteLine($"  {new string('-', 85)}");

            PrintRow("coom_status [20→30]",                v1_coomStatus, v2_coomStatus, v2_coomStatus == "30",  "已更新");
            PrintRow("esmm_status [01→10]",                v1_esmmStatus, v2_esmmStatus, v2_esmmStatus == "10",  "已更新");
            PrintRow("e_shipment_l 筆數 [1→2]",            v1_esmlCount.ToString(), v2_esmlCount.ToString(), v2_esmlCount == 2, "追加");
            PrintRow("e_shipment_s 筆數 [1→2]",            v1_esmsCount.ToString(), v2_esmsCount.ToString(), v2_esmsCount == 2, "追加");
            PrintRow("esml 含取號記錄 (01) [應保留]",       "-", hasEsml01 ? "存在" : "不存在", hasEsml01,  "保留");
            PrintRow("esml 含寄貨記錄 (10) [應新增]",       "-", hasEsml10 ? "存在" : "不存在", hasEsml10,  "新增");
            PrintRow("esms 含 1001 記錄 [應保留]",          "-", hasEsms1001 ? "存在" : "不存在", hasEsms1001, "保留");
            PrintRow("esms 含 1A01 記錄 [應新增]",          "-", hasEsms1A01 ? "存在" : "不存在", hasEsms1A01, "新增");
            PrintRow("coom_name [應保留不變]",              v1_name, v2_name, v1_name == v2_name, "保留");
            PrintRow("coom_rcv_total_amt [應保留不變]",     v1_amt,  v2_amt,  v1_amt == v2_amt,   "保留");

            bool allPass =
                v2_coomStatus == "30" &&
                v2_esmmStatus == "10" &&
                v2_esmlCount  == 2    &&
                v2_esmsCount  == 2    &&
                hasEsml01 && hasEsml10 &&
                hasEsms1001 && hasEsms1A01 &&
                v1_name == v2_name &&
                v1_amt  == v2_amt;

            Console.WriteLine();
            Console.WriteLine(allPass
                ? "  🎉 全部驗證通過！$set + $push 並行操作邏輯正確，陣列增量追加無資料遺失。"
                : "  ⚠️  部分驗證失敗，請檢查更新邏輯。");

            // ── Step 6：完整 BsonDocument 前後對比 ──────────────────
            PrintStep("Step 6：完整 BsonDocument 前後對比");
            Console.WriteLine("【V1 Status 20】");
            Console.WriteLine(v1.ToJson(_pretty));
            Console.WriteLine();
            Console.WriteLine("【V2 Status 30】");
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

        private static int GetArrayCount(BsonDocument doc, string field)
        {
            if (!doc.Contains(field)) return 0;
            var val = doc[field];
            return val.IsBsonArray ? val.AsBsonArray.Count : 0;
        }

        private static void PrintRow(string field, string v1, string v2, bool pass, string expect)
        {
            string icon = pass ? "✅" : "❌";
            Console.WriteLine($"  {field,-48} {v1,-10} {v2,-10} {expect,-8} {icon} {(pass ? "PASS" : "FAIL")}");
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
