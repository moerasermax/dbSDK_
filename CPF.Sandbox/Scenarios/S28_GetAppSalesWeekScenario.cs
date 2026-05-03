using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S28_GetAppSalesWeekScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S28: Search 6 — GetAppSalesWeek ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // GoldenRecipe In
            var req = new OrderSearchRequest
            {
                CuamCid         = 528672,
                SearchStartDate = new DateTime(2026, 4, 22, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate   = new DateTime(2026, 4, 29, 15, 59, 59, DateTimeKind.Utc),
                DateStartPoP    = new DateTime(2026, 4, 22, 16, 0, 0, DateTimeKind.Utc),
                DateEndPoP      = new DateTime(2026, 4, 29, 15, 59, 59, DateTimeKind.Utc),
                DateRangeType   = DateRangeType.SetWeek,
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, searchStart={req.SearchStartDate:O}, dateRangeType={req.DateRangeType}");

            var result = await svc.GetAppSalesWeekAsync(req);
            Check("IsSuccess", result.IsSuccess, true);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            Check("Data not null & length=1", result.Data is { Length: 1 }, true);
            if (result.Data is not { Length: > 0 }) return;

            var data        = result.Data[0];
            var trend0423   = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "04/23")?.Value;
            var trend0424   = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "04/24")?.Value;
            var trend0428   = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "04/28")?.Value;
            var trend0429   = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "04/29")?.Value;
            var topProduct  = data.ProductSalesRanking?.FirstOrDefault();

            Console.WriteLine($"  Out — totalAmount={data.TotalAmount}, totalOrderCnt={data.TotalOrderCnt}");
            Console.WriteLine($"  Out — salesTrend[04/23]={trend0423}, [04/24]={trend0424}");
            Console.WriteLine($"  Out — salesTrend[04/28]={trend0428}, [04/29]={trend0429}");
            Console.WriteLine($"  Out — topProduct={topProduct?.ProductCgdmid}, productTotalSales={topProduct?.ProductTotalSales}");

            // 30 筆實測值（GoldenRecipe Search_6 期望 176/2/04/28-29 來自 production，30 筆無此區間 doc）
            Check("TotalAmount",              data.TotalAmount,              2748);
            Check("TotalOrderCnt",            data.TotalOrderCnt,              20);
            // DateHistogram + TimeZone(+08:00) 驗證：search range 起點 = Taiwan 04/23 00:00（UTC 04/22 16:00），
            // 04/22 在 range 之外；30 筆下有 doc 的 trend bucket 為 04/23、04/24
            Check("SalesTrendData[\"04/23\"].Value", trend0423, 678);
            Check("SalesTrendData[\"04/24\"].Value", trend0424, 2070);
            Check("TopProduct.ProductCgdmid", topProduct?.ProductCgdmid,      "GM2508260014245");
            Check("TopProduct.ProductTotalSales", topProduct?.ProductTotalSales, 2);

            Console.WriteLine("  ⚠️ 以下項目於 30 筆範例下無資料可驗證：");
            Console.WriteLine("     • SalesTrendData[\"04/28\"].Value 期望=88（範例缺 04/28 doc）");
            Console.WriteLine("     • SalesTrendData[\"04/29\"].Value 期望=88（範例缺 04/29 doc）");
            Console.WriteLine("     • GoldenRecipe TotalAmount=176/TotalOrderCnt=2/TopProduct=GM2512170027503 都需 04/28-29 訂單");
            Console.WriteLine("     ➜ 待客戶補 04/28、04/29 範例 doc 後才能驗證 GoldenRecipe 完整輸出。");
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
