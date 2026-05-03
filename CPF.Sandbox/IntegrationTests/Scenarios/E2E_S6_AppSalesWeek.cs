using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.Scenarios;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.IntegrationTests.Scenarios
{
    public static class E2E_S6_AppSalesWeek
    {
        public static async Task RunAsync(TestDataset dataset)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== E2E S6: GetAppSalesWeek ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // 用預埋的 4/22~4/29 區間（Taiwan）
            var start = new DateTime(2026, 4, 22, 16, 0, 0, DateTimeKind.Utc);
            var end   = new DateTime(2026, 4, 29, 15, 59, 59, 999, DateTimeKind.Utc);

            var req = new OrderSearchRequest
            {
                CuamCid         = dataset.SellerCuamCid,
                SearchStartDate = start,
                SearchEndDate   = end,
                DateStartPoP    = start,
                DateEndPoP      = end,
                DateRangeType   = DateRangeType.SetWeek,
            };

            var expected = ExpectedValueCalculator.Calculate_Search56(
                dataset, dataset.SellerCuamCid, start, end, isHourly: false);

            var result = await svc.GetAppSalesWeekAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }
            if (result.Data is not { Length: > 0 }) { Console.WriteLine("  ❌ ERROR: Data 為空"); return; }

            var data = result.Data[0];
            var trendActual = data.SalesTrendData?
                .Where(t => t.TimePane != null)
                .ToDictionary(t => t.TimePane!, t => t.Value)
                ?? new Dictionary<string, int>();
            var top = data.ProductSalesRanking?.FirstOrDefault();

            Console.WriteLine($"  Out — TotalAmount={data.TotalAmount}, Cnt={data.TotalOrderCnt}");
            Console.WriteLine($"  Out — Trend buckets: {string.Join(",", trendActual.Where(kv => kv.Value > 0).Select(kv => $"{kv.Key}={kv.Value}"))}");
            Console.WriteLine($"  Out — TopProduct cgdmid={top?.ProductCgdmid}, qty={top?.ProductTotalSales}");
            Console.WriteLine($"  Expected: Total={expected.TotalAmount}, Cnt={expected.TotalOrderCnt}, TopCgdmid={expected.TopProductCgdmid}, TopQty={expected.TopProductTotalSales}");

            Check("TotalAmount",   data.TotalAmount,   expected.TotalAmount);
            Check("TotalOrderCnt", data.TotalOrderCnt, expected.TotalOrderCnt);
            Check("TopProduct.Cgdmid",     top?.ProductCgdmid,      expected.TopProductCgdmid);
            Check("TopProduct.TotalSales", top?.ProductTotalSales,  expected.TopProductTotalSales);

            foreach (var (key, val) in expected.SalesTrend.Where(kv => kv.Value > 0))
            {
                trendActual.TryGetValue(key, out var actualVal);
                Check($"Trend[\"{key}\"]", actualVal, val);
            }
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
