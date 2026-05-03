using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.Scenarios;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.IntegrationTests.Scenarios
{
    /// <summary>
    /// 整合測試 S1 (HomeOverview)：以 100 筆 dataset 驗證 BLL aggregate 邏輯
    /// 對 ExpectedValueCalculator 從 dataset 反算的值。
    /// </summary>
    public static class E2E_S1_HomeOverview
    {
        public static async Task RunAsync(TestDataset dataset)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== E2E S1: GetHomeToDoOverView ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // 用 dataset 完整區間 + 預埋的 SellerCuamCid
            var req = new OrderSearchRequest
            {
                CuamCid = dataset.SellerCuamCid,
                MemSid  = dataset.BuyerMemSid,
                SearchStartDate = dataset.DateRangeStart,
                SearchEndDate   = dataset.DateRangeEnd,
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, memSid={req.MemSid}, range={req.SearchStartDate:yyyy-MM-dd}~{req.SearchEndDate:yyyy-MM-dd}");

            // BLL 不接受 MemSid 在 GetHomeToDoOverView？看 BLL line 30~38：buyerOverview 用 cid（!）
            // 實際 BLL：buyerOverview/buyerPerformance 也是用 CuamCid。讓 expected 與 actual 一致。
            var expected = ExpectedValueCalculator.Calculate_Search1(
                dataset, dataset.SellerCuamCid, dataset.SellerCuamCid,
                dataset.DateRangeStart, dataset.DateRangeEnd);

            var result = await svc.GetHomeToDoOverViewAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var bp   = data.BuyerPerformance?[0];
            var sp   = data.SellerPerformance?[0];

            Console.WriteLine($"  Out — BuyerPerformance: orderCount={bp?.OrderCount}, pickupCount={bp?.PickupCount}");
            Console.WriteLine($"  Out — SellerPerformance: orderCount={sp?.OrderCount}, sendCount={sp?.SendCount}, salesAmt={sp?.SalesAmt}");
            Console.WriteLine($"  Expected: BuyerOrder={expected.BuyerOrderCount}, BuyerPickup={expected.BuyerPickupCount}, "
                            + $"SellerOrder={expected.SellerOrderCount}, SellerSend={expected.SellerSendCount}, SalesAmt={expected.SellerSalesAmt}");

            Check("BuyerPerformance.OrderCount",  bp?.OrderCount,  expected.BuyerOrderCount);
            Check("BuyerPerformance.PickupCount", bp?.PickupCount, expected.BuyerPickupCount);
            Check("SellerPerformance.OrderCount", sp?.OrderCount,  expected.SellerOrderCount);
            Check("SellerPerformance.SendCount",  sp?.SendCount,   expected.SellerSendCount);
            Check("SellerPerformance.SalesAmt",   sp?.SalesAmt,    expected.SellerSalesAmt);
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
