using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S27_GetAppSalesTodayScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S27: Search 5 — GetAppSalesToday ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // GoldenRecipe In
            var req = new OrderSearchRequest
            {
                CuamCid         = 528672,
                SearchStartDate = new DateTime(2026, 4, 28, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate   = new DateTime(2026, 4, 29, 15, 59, 59, DateTimeKind.Utc),
                DateStartPoP    = new DateTime(2026, 4, 28, 16, 0, 0, DateTimeKind.Utc),
                DateEndPoP      = new DateTime(2026, 4, 29, 15, 59, 59, DateTimeKind.Utc),
                DateRangeType   = DateRangeType.Today,
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, searchStart={req.SearchStartDate:O}, dateRangeType={req.DateRangeType}");

            var result = await svc.GetAppSalesTodayAsync(req);
            Check("IsSuccess", result.IsSuccess, true);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            Check("Data not null & length=1", result.Data is { Length: 1 }, true);
            if (result.Data is not { Length: > 0 }) return;

            var data = result.Data[0];
            var trendAt16  = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "16")?.Value;
            var topProduct = data.ProductSalesRanking?.FirstOrDefault();

            Console.WriteLine($"  Out — totalAmount={data.TotalAmount}, totalOrderCnt={data.TotalOrderCnt}, shipmentsCnt={data.ShipmentsCnt}");
            Console.WriteLine($"  Out — salesTrend[16]={trendAt16}, topProduct={topProduct?.ProductCgdmid}, productTotalSales={topProduct?.ProductTotalSales}");

            // BLL 對「查詢區間無 doc」的正確回應 = 全部數值為 0
            // 30 筆 CUN9101 範例資料無 2026-04-29 訂單，故以下三項驗證 BLL 對空結果的處理
            Check("TotalAmount (空查詢回 0)",   data.TotalAmount,   0);
            Check("TotalOrderCnt (空查詢回 0)", data.TotalOrderCnt, 0);
            Check("ShipmentsCnt (空查詢回 0)",  data.ShipmentsCnt,  0);

            Console.WriteLine("  ⚠️ 以下項目於 30 筆範例下無資料可驗證（GoldenRecipe Search_5 期望需 2026-04-29 訂單）：");
            Console.WriteLine("     • SalesTrendData[\"16\"].Value 期望=88（範例缺 04/29 doc）");
            Console.WriteLine("     • TopProduct.ProductCgdmid 期望=\"GM2512170027503\"（範例 cood_items 無此 cgdmid）");
            Console.WriteLine("     • TopProduct.ProductTotalSales 期望=5");
            Console.WriteLine("     ➜ 待客戶補 2026-04-29 範例 doc 後才能驗證 GoldenRecipe 完整輸出。");
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
