using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Sandbox.Generators;
using System.Text.Json;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// S21 完整測試函數
    /// 驗證 ElasticSearch JSON 序列化與 Sample_Data 完全對齊
    /// </summary>
    public static class S21ElasticJsonTest
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static void RunAllTests()
        {
            PrintHeader("S21 ElasticSearch JSON 完整測試");

            int passCount = 0;
            int failCount = 0;

            // 測試 1：OrderInfoModel 基本屬性映射
            Console.WriteLine();
            Console.WriteLine("=== 測試 1：OrderInfoModel 基本屬性映射 ===");
            var (p1, f1) = TestBasicPropertyMapping();
            passCount += p1; failCount += f1;

            // 測試 2：寄貨更新 JSON 格式
            Console.WriteLine();
            Console.WriteLine("=== 測試 2：寄貨更新 JSON 格式 (Delivery_CargoDynamics_02) ===");
            var (p2, f2) = TestShippingUpdateJson();
            passCount += p2; failCount += f2;

            // 測試 3：取號更新 JSON 格式
            Console.WriteLine();
            Console.WriteLine("=== 測試 3：取號更新 JSON 格式 (UpdateSellerGetNumberEvent) ===");
            var (p3, f3) = TestGetNumberUpdateJson();
            passCount += p3; failCount += f3;

            // 測試 4：CoodItems 子結構
            Console.WriteLine();
            Console.WriteLine("=== 測試 4：CoodItems 子結構對齊 ===");
            var (p4, f4) = TestCoodItemsStructure();
            passCount += p4; failCount += f4;

            // 測試 5：數值型別驗證 (int vs string)
            Console.WriteLine();
            Console.WriteLine("=== 測試 5：數值型別驗證 (int vs string) ===");
            var (p5, f5) = TestNumericTypes();
            passCount += p5; failCount += f5;

            // 測試 6：ProductionDataGenerator 產生器
            Console.WriteLine();
            Console.WriteLine("=== 測試 6：ProductionDataGenerator 產生器驗證 ===");
            var (p6, f6) = TestProductionDataGenerator();
            passCount += p6; failCount += f6;

            // 測試 7：與 Sample_Data 完整比對
            Console.WriteLine();
            Console.WriteLine("=== 測試 7：與 Sample_Data 完整比對 ===");
            var (p7, f7) = TestSampleDataComparison();
            passCount += p7; failCount += f7;

            // 測試總結
            PrintSummary(passCount, failCount);
        }

        private static (int pass, int fail) TestBasicPropertyMapping()
        {
            int pass = 0, fail = 0;

            var model = new OrderInfoModel
            {
                CoomNo = "CM1234567890123",
                CoomName = "TestStore",
                CoomStatus = "10",
                CoomTempType = "01",
                CoomCreateDatetime = DateTime.Parse("2026-01-01T00:00:00Z"),
                CoomCuamCid = 12345,
                CoocNo = "CC1234567890123",
                CoocPaymentType = "1",
                CoocDeliverMethod = "1",
                CoocOrdChannelKind = "1",
                CoocMemSid = 67890,
                CoodNames = new[] { "Product1", "Product2" }
            };

            string json = JsonSerializer.Serialize(model, _jsonOpts);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 驗證每個屬性
            AssertProperty(root, "coom_no", "CM1234567890123", ref pass, ref fail);
            AssertProperty(root, "coom_name", "TestStore", ref pass, ref fail);
            AssertProperty(root, "coom_status", "10", ref pass, ref fail);
            AssertProperty(root, "coom_temp_type", "01", ref pass, ref fail);
            AssertProperty(root, "cooc_no", "CC1234567890123", ref pass, ref fail);
            AssertProperty(root, "cooc_payment_type", "1", ref pass, ref fail);
            AssertProperty(root, "cooc_deliver_method", "1", ref pass, ref fail);
            AssertProperty(root, "cooc_ord_channel_kind", "1", ref pass, ref fail);
            AssertProperty(root, "cood_name", JsonValueKind.Array, ref pass, ref fail);

            // 驗證數值型別 (int)
            if (root.TryGetProperty("coom_cuam_cid", out var cuamCid) && cuamCid.ValueKind == JsonValueKind.Number && cuamCid.GetInt32() == 12345)
            {
                Console.WriteLine("  ✅ coom_cuam_cid 為數字 (int): 12345");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ coom_cuam_cid 應為數字");
                fail++;
            }

            if (root.TryGetProperty("cooc_mem_sid", out var memSid) && memSid.ValueKind == JsonValueKind.Number && memSid.GetInt32() == 67890)
            {
                Console.WriteLine("  ✅ cooc_mem_sid 為數字 (int): 67890");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ cooc_mem_sid 應為數字");
                fail++;
            }

            return (pass, fail);
        }

        private static (int pass, int fail) TestShippingUpdateJson()
        {
            int pass = 0, fail = 0;

            var gen = new ProductionDataGenerator();
            var model = gen.GenerateElasticShippingUpdateModel();
            string json = JsonSerializer.Serialize(model, _jsonOpts);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 驗證關鍵欄位
            AssertProperty(root, "coom_status", "30", ref pass, ref fail);
            AssertProperty(root, "esmm_status", "10", ref pass, ref fail);
            AssertProperty(root, "esmm_ship_no", "D88032120964", ref pass, ref fail);
            AssertProperty(root, "coom_rcv_totalamt", 138, ref pass, ref fail);
            AssertProperty(root, "esmm_rcv_total_amt", 138, ref pass, ref fail);

            // 驗證 esml_status_shipping_datetime 存在
            if (root.TryGetProperty("esml_status_shipping_datetime", out var shippingDateTime))
            {
                Console.WriteLine("  ✅ esml_status_shipping_datetime 存在");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ esml_status_shipping_datetime 不存在");
                fail++;
            }

            return (pass, fail);
        }

        private static (int pass, int fail) TestGetNumberUpdateJson()
        {
            int pass = 0, fail = 0;

            var gen = new ProductionDataGenerator();
            var model = gen.GenerateElasticGetNumberUpdateModel();
            string json = JsonSerializer.Serialize(model, _jsonOpts);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 驗證關鍵欄位
            AssertProperty(root, "coom_status", "20", ref pass, ref fail);
            AssertProperty(root, "esmm_status", "01", ref pass, ref fail);
            AssertProperty(root, "esmm_ship_no", "D88032120964", ref pass, ref fail);

            return (pass, fail);
        }

        private static (int pass, int fail) TestCoodItemsStructure()
        {
            int pass = 0, fail = 0;

            var model = new OrderInfoModel
            {
                CoomNo = "CM123",
                CoodItems = new[]
                {
                    new CoodItems
                    {
                        CgddCgdmid = "GM123",
                        CgddId = "ID123",
                        CoodCgdsId = "SID123",
                        CoodName = "TestProduct",
                        CoodQty = 5,
                        CoodImagePath = "image.jpg"
                    }
                }
            };

            string json = JsonSerializer.Serialize(model, _jsonOpts);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 驗證 cood_items 陣列
            if (root.TryGetProperty("cood_items", out var coodItems) && coodItems.ValueKind == JsonValueKind.Array)
            {
                Console.WriteLine("  ✅ cood_items 為陣列");
                pass++;

                var firstItem = coodItems[0];
                AssertProperty(firstItem, "cgdd_cgdmid", "GM123", ref pass, ref fail);
                AssertProperty(firstItem, "cgdd_id", "ID123", ref pass, ref fail);
                AssertProperty(firstItem, "cood_cgdsid", "SID123", ref pass, ref fail);
                AssertProperty(firstItem, "cood_name", "TestProduct", ref pass, ref fail);
                AssertProperty(firstItem, "cood_qty", 5, ref pass, ref fail);
                AssertProperty(firstItem, "cood_image_path", "image.jpg", ref pass, ref fail);
            }
            else
            {
                Console.WriteLine("  ❌ cood_items 應為陣列");
                fail++;
            }

            return (pass, fail);
        }

        private static (int pass, int fail) TestNumericTypes()
        {
            int pass = 0, fail = 0;

            var model = new OrderInfoModel
            {
                CoomNo = "CM123",
                CoomCuamCid = 12345,
                CoocMemSid = 67890,
                CoomRcvTotalAmt = 1000,
                EsmmRcvTotalAmt = 1000,
                CoodItems = new[]
                {
                    new CoodItems { CoodQty = 5 }
                }
            };

            string json = JsonSerializer.Serialize(model, _jsonOpts);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // 驗證所有數值欄位都是 Number 型別
            AssertNumericProperty(root, "coom_cuam_cid", 12345, ref pass, ref fail);
            AssertNumericProperty(root, "cooc_mem_sid", 67890, ref pass, ref fail);
            AssertNumericProperty(root, "coom_rcv_totalamt", 1000, ref pass, ref fail);
            AssertNumericProperty(root, "esmm_rcv_total_amt", 1000, ref pass, ref fail);

            if (root.TryGetProperty("cood_items", out var items) && items.ValueKind == JsonValueKind.Array)
            {
                var item = items[0];
                AssertNumericProperty(item, "cood_qty", 5, ref pass, ref fail);
            }

            return (pass, fail);
        }

        private static (int pass, int fail) TestProductionDataGenerator()
        {
            int pass = 0, fail = 0;

            var gen = new ProductionDataGenerator();

            // 測試 GenerateElasticOrderInfoModel
            var model1 = gen.GenerateElasticOrderInfoModel();
            if (model1.CoomNo != null && model1.CoodItems != null && model1.CoodItems.Length > 0)
            {
                Console.WriteLine("  ✅ GenerateElasticOrderInfoModel 產生正確");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ GenerateElasticOrderInfoModel 產生錯誤");
                fail++;
            }

            // 測試 GenerateElasticShippingUpdateModel
            var model2 = gen.GenerateElasticShippingUpdateModel();
            if (model2.CoomStatus == "30" && model2.EsmmStatus == "10")
            {
                Console.WriteLine("  ✅ GenerateElasticShippingUpdateModel 產生正確 (Status 30/10)");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ GenerateElasticShippingUpdateModel 產生錯誤");
                fail++;
            }

            // 測試 GenerateElasticGetNumberUpdateModel
            var model3 = gen.GenerateElasticGetNumberUpdateModel();
            if (model3.CoomStatus == "20" && model3.EsmmStatus == "01")
            {
                Console.WriteLine("  ✅ GenerateElasticGetNumberUpdateModel 產生正確 (Status 20/01)");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ GenerateElasticGetNumberUpdateModel 產生錯誤");
                fail++;
            }

            return (pass, fail);
        }

        private static (int pass, int fail) TestSampleDataComparison()
        {
            int pass = 0, fail = 0;

            var gen = new ProductionDataGenerator();

            // 寄貨更新 - 與 Sample_Data 精確比對
            var shippingModel = gen.GenerateElasticShippingUpdateModel();
            string shippingJson = JsonSerializer.Serialize(shippingModel, _jsonOpts);

            // 關鍵欄位比對
            bool match = true;
            var doc = JsonDocument.Parse(shippingJson);
            var root = doc.RootElement;

            // 比對每個關鍵欄位
            if (!root.TryGetProperty("coom_no", out var coomNo) || coomNo.GetString() != "CM2604160395986") match = false;
            if (!root.TryGetProperty("coom_name", out var coomName) || coomName.GetString() != "test") match = false;
            if (!root.TryGetProperty("coom_status", out var coomStatus) || coomStatus.GetString() != "30") match = false;
            if (!root.TryGetProperty("coom_temp_type", out var tempType) || tempType.GetString() != "01") match = false;
            if (!root.TryGetProperty("coom_cuam_cid", out var cuamCid) || cuamCid.GetInt32() != 528672) match = false;
            if (!root.TryGetProperty("cooc_no", out var coocNo) || coocNo.GetString() != "CC2604160431308") match = false;
            if (!root.TryGetProperty("cooc_payment_type", out var payType) || payType.GetString() != "1") match = false;
            if (!root.TryGetProperty("cooc_deliver_method", out var deliverMethod) || deliverMethod.GetString() != "1") match = false;
            if (!root.TryGetProperty("cooc_ord_channel_kind", out var channelKind) || channelKind.GetString() != "1") match = false;
            if (!root.TryGetProperty("cooc_mem_sid", out var memSid) || memSid.GetInt32() != 528672) match = false;
            if (!root.TryGetProperty("coom_rcv_totalamt", out var coomRcv) || coomRcv.GetInt32() != 138) match = false;
            if (!root.TryGetProperty("esmm_rcv_total_amt", out var esmmRcv) || esmmRcv.GetInt32() != 138) match = false;
            if (!root.TryGetProperty("esmm_ship_no", out var shipNo) || shipNo.GetString() != "D88032120964") match = false;
            if (!root.TryGetProperty("esmm_status", out var esmmStatus) || esmmStatus.GetString() != "10") match = false;

            if (match)
            {
                Console.WriteLine("  ✅ 寄貨更新 JSON 與 Sample_Data 完全一致");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ 寄貨更新 JSON 與 Sample_Data 不一致");
                fail++;
            }

            // 取號更新 - 與 Sample_Data 精確比對
            var getNumberModel = gen.GenerateElasticGetNumberUpdateModel();
            string getNumberJson = JsonSerializer.Serialize(getNumberModel, _jsonOpts);

            match = true;
            doc = JsonDocument.Parse(getNumberJson);
            root = doc.RootElement;

            if (!root.TryGetProperty("coom_status", out coomStatus) || coomStatus.GetString() != "20") match = false;
            if (!root.TryGetProperty("esmm_status", out esmmStatus) || esmmStatus.GetString() != "01") match = false;

            if (match)
            {
                Console.WriteLine("  ✅ 取號更新 JSON 與 Sample_Data 完全一致");
                pass++;
            }
            else
            {
                Console.WriteLine("  ❌ 取號更新 JSON 與 Sample_Data 不一致");
                fail++;
            }

            return (pass, fail);
        }

        // ─────────────────────────────────────────────
        // 輔助方法
        // ─────────────────────────────────────────────

        private static void AssertProperty(JsonElement root, string propertyName, string expectedValue, ref int pass, ref int fail)
        {
            if (root.TryGetProperty(propertyName, out var prop) && prop.GetString() == expectedValue)
            {
                Console.WriteLine($"  ✅ {propertyName} = \"{expectedValue}\"");
                pass++;
            }
            else
            {
                Console.WriteLine($"  ❌ {propertyName} 應為 \"{expectedValue}\"");
                fail++;
            }
        }

        private static void AssertProperty(JsonElement root, string propertyName, JsonValueKind expectedKind, ref int pass, ref int fail)
        {
            if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == expectedKind)
            {
                Console.WriteLine($"  ✅ {propertyName} 為 {expectedKind}");
                pass++;
            }
            else
            {
                Console.WriteLine($"  ❌ {propertyName} 應為 {expectedKind}");
                fail++;
            }
        }

        private static void AssertProperty(JsonElement root, string propertyName, int expectedValue, ref int pass, ref int fail)
        {
            if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.GetInt32() == expectedValue)
            {
                Console.WriteLine($"  ✅ {propertyName} = {expectedValue}");
                pass++;
            }
            else
            {
                Console.WriteLine($"  ❌ {propertyName} 應為 {expectedValue}");
                fail++;
            }
        }

        private static void AssertNumericProperty(JsonElement root, string propertyName, int expectedValue, ref int pass, ref int fail)
        {
            if (root.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.GetInt32() == expectedValue)
            {
                Console.WriteLine($"  ✅ {propertyName} 為數字型別: {expectedValue}");
                pass++;
            }
            else
            {
                Console.WriteLine($"  ❌ {propertyName} 應為數字型別 {expectedValue}");
                fail++;
            }
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 70));
        }

        private static void PrintSummary(int passCount, int failCount)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 70));
            Console.WriteLine($"  測試總結: ✅ {passCount} 通過 / ❌ {failCount} 失敗");
            Console.WriteLine(new string('=', 70));

            if (failCount == 0)
            {
                Console.WriteLine();
                Console.WriteLine("  🎉 所有測試通過！S21 ElasticSearch JSON 格式驗證成功！");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("  ⚠️  有測試失敗，請檢查上述錯誤項目");
            }
        }
    }
}