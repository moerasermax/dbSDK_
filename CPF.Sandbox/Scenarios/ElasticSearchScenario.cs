using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using CPF.Service.SendDataToElasticCloud.Model;
using CPF.Sandbox.Generators;
using System.Text.Json;

namespace CPF.Sandbox.Scenarios
{
    public static class ElasticSearchScenario
    {
        public static void RunElasticSearchSimulation()
        {
            PrintHeader("Elastic Search 強型別濾鏡驗證");

            var mockRepo = new MockElasticRepository<OrderInfoModel>("order_index");

            // 1. 測試：JsonPropertyName 映射 ("CoomNo" -> "coom_no")
            var filter1 = new ElasticFilter()
                .Term<OrderInfoModel>(m => m.CoomNo, "CM20260427");

            // 2. 測試：組合查詢 (AND + OR)
            var filterComplex = new ElasticFilter()
                .Term<OrderInfoModel>(m => m.CoomNo, "CM001")
                .Gte<OrderInfoModel>(m => m.CoomRcvTotalAmt, 100)
                .In<OrderInfoModel>(m => m.CoomStatus, new[] { "10", "20" });

            Console.WriteLine($"[1] JsonPropertyName 映射測試 (CoomNo -> coom_no.keyword)");
            Console.WriteLine(FormatJson(mockRepo.GetSearchDsl(filter1)));

            Console.WriteLine();
            Console.WriteLine($"[2] 組合查詢 (CoomNo=CM001 AND Amt>=100 AND Status IN [10,20])");
            Console.WriteLine(FormatJson(mockRepo.GetSearchDsl(filterComplex)));

            // 驗證斷言
            string dsl = mockRepo.GetSearchDsl(filterComplex);
            bool hasBool = dsl.Contains("bool");
            bool hasFilter = dsl.Contains("filter");
            bool hasCoomNo = dsl.Contains("coom_no.keyword"); // 驗證標籤有效

            Console.WriteLine();
            Console.WriteLine($"✅ DSL 結構包含 bool/filter : {(hasBool && hasFilter ? "PASS" : "FAIL")}");
            Console.WriteLine($"✅ JsonPropertyName 映射成功 : {(hasCoomNo ? "PASS" : "FAIL")}");
        }

        /// <summary>
        /// S21 新增：驗證 Elastic JSON 序列化格式是否符合 Sample_Data 規範
        /// </summary>
        public static void RunElasticJsonFormatVerification()
        {
            PrintHeader("S21 Elastic JSON 格式驗證 (Sample_Data 比對)");

            var generator = new ProductionDataGenerator();

            // 測試 1：寄貨更新後 JSON
            Console.WriteLine();
            Console.WriteLine("=== [寄貨更新後] Elastic JSON 輸出 ===");
            var shippingModel = generator.GenerateElasticShippingUpdateModel();
            string shippingJson = JsonSerializer.Serialize(shippingModel);
            Console.WriteLine(FormatJson(shippingJson));

            // 測試 2：取號更新後 JSON
            Console.WriteLine();
            Console.WriteLine("=== [取號更新後] Elastic JSON 輸出 ===");
            var getNumberModel = generator.GenerateElasticGetNumberUpdateModel();
            string getNumberJson = JsonSerializer.Serialize(getNumberModel);
            Console.WriteLine(FormatJson(getNumberJson));

            // 驗證關鍵欄位
            Console.WriteLine();
            Console.WriteLine("=== 驗證結果 ===");
            
            // 寄貨驗證
            bool shippingCoomStatus = shippingJson.Contains("\"coom_status\":\"30\"");
            bool shippingEsmmStatus = shippingJson.Contains("\"esmm_status\":\"10\"");
            bool shippingShipNo = shippingJson.Contains("\"esmm_ship_no\":\"D88032120964\"");
            bool shippingDateTime = shippingJson.Contains("\"esml_status_shipping_datetime\":\"2026-04-16T06:20:00Z\"");
            
            Console.WriteLine($"[寄貨] coom_status=30: {(shippingCoomStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esmm_status=10: {(shippingEsmmStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esmm_ship_no=D88032120964: {(shippingShipNo ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[寄貨] esml_status_shipping_datetime: {(shippingDateTime ? "✅ PASS" : "❌ FAIL")}");

            // 取號驗證
            bool getNumberCoomStatus = getNumberJson.Contains("\"coom_status\":\"20\"");
            bool getNumberEsmmStatus = getNumberJson.Contains("\"esmm_status\":\"01\"");
            bool getNumberShipNo = getNumberJson.Contains("\"esmm_ship_no\":\"D88032120964\"");
            
            Console.WriteLine($"[取號] coom_status=20: {(getNumberCoomStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] esmm_status=01: {(getNumberEsmmStatus ? "✅ PASS" : "❌ FAIL")}");
            Console.WriteLine($"[取號] esmm_ship_no=D88032120964: {(getNumberShipNo ? "✅ PASS" : "❌ FAIL")}");
        }

        private static string FormatJson(string json)
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                return JsonSerializer.Serialize(jsonElement, options);
            }
            catch
            {
                return json;
            }
        }

        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"  {title}");
            Console.WriteLine(new string('=', 60));
        }
    }
}
