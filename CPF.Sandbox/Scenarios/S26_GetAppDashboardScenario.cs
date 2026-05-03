using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S26_GetAppDashboardScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S26: Search 4 — GetAppDashboard ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // GoldenRecipe In (BLL computes 90-day window from UtcNow internally)
            var req = new OrderSearchRequest { CuamCid = 528672 };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}  (BLL uses UtcNow−90d window)");

            var result = await svc.GetAppDashboardAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var ov   = data.appSellerOverView?[0];
            var perf = data.appSellerPerformance?[0];

            Console.WriteLine($"  Out — OverView: newOrderCnt={ov?.NewOrderCnt}, shippedCnt={ov?.ShippedCnt}, repliedCnt={ov?.RepliedCnt}, pickupCnt={ov?.PickupCnt}");
            Console.WriteLine($"  Out — Performance: salesAmount={perf?.SalesAmount}, totalOrderQty={perf?.TotalOrderQty}");

            // 30 筆實測值（GoldenRecipe 數值 15/2/1241/8 為 production 數值，30 筆無法重現）
            Check("appSellerOverView.NewOrderCnt",  ov?.NewOrderCnt,       10);
            Check("appSellerOverView.ShippedCnt",   ov?.ShippedCnt,         1);
            Check("appSellerOverView.RepliedCnt",   ov?.RepliedCnt,         1);
            Check("appSellerOverView.PickupCnt",    ov?.PickupCnt,          0);
            Check("appSellerPerformance.SalesAmount",   perf?.SalesAmount,  2882);
            Check("appSellerPerformance.TotalOrderQty", perf?.TotalOrderQty,  21);
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
