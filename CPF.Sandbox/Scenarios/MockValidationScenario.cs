using CPF.Sandbox.Mocks;
using MongoDB.Bson;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// 離線驗證：$push 指令生成 + 日期格式解析（不依賴資料庫）
    /// </summary>
    public static class MockValidationScenario
    {
        public static void Run()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Mock 測試：貨態歷程追加驗證 ===");
            Console.WriteLine("========================================\n");

            var mockRepo = new MockOrderRepository();

            var status20Data = new MockOrderData
            {
                CoomNo = "CM2604160395986",
                CoomStatus = "20",
                EsmmStatus = "01"
            };

            var status30Options = new MockUpdateOptions
            {
                PushFields = new List<string> { "e_shipment_l", "e_shipment_s" }
            };

            var initResult = mockRepo.UpdateInit(
                @"{""coom_no"": ""CM2604160395986""}",
                status20Data,
                status30Options
            ).Result;

            Console.WriteLine($"測試結果: {initResult.Msg}");
            Console.WriteLine($"指令內容: {initResult.DataJson}");

            var updateDoc = BsonDocument.Parse(initResult.DataJson);
            var hasPush = updateDoc.Contains("UpdateDefinition") &&
                          updateDoc["UpdateDefinition"].AsBsonDocument.Contains("$push");

            if (hasPush)
            {
                Console.WriteLine("\n✅ Mock 測試通過：$push 指令正確生成");
                Console.WriteLine($"   - $push 內容: {updateDoc["UpdateDefinition"]["$push"]}");
            }
            else
            {
                Console.WriteLine("\n❌ Mock 測試失敗：未找到 $push 指令");
            }

            Console.WriteLine("\n--- 日期格式解析測試 ---");
            var testDates = new[] { "20260416061156.970", "20260416061156", "20260416" };
            foreach (var dateStr in testDates)
            {
                try
                {
                    var parsed = DateTime.ParseExact(dateStr,
                        new[] { "yyyyMMddHHmmss.fff", "yyyyMMddHHmmss", "yyyyMMdd" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None);
                    Console.WriteLine($"✅ '{dateStr}' → {parsed:yyyy-MM-dd HH:mm:ss.fff}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ '{dateStr}' 解析失敗: {ex.Message}");
                }
            }

            Console.WriteLine("\n========================================");
            Console.WriteLine("=== Mock 測試完成 ===");
            Console.WriteLine("========================================\n");
        }
    }
}
