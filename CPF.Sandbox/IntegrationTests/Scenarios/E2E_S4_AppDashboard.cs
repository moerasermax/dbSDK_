using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.Scenarios;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.IntegrationTests.Scenarios
{
    public static class E2E_S4_AppDashboard
    {
        public static async Task RunAsync(TestDataset dataset)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== E2E S4: GetAppDashboard ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest { CuamCid = dataset.SellerCuamCid };

            // BLL 用 DateTime.UtcNow（動態），Calculator 也用同樣 UtcNow
            var nowUtc = DateTime.UtcNow;
            var expected = ExpectedValueCalculator.Calculate_Search4(dataset, dataset.SellerCuamCid, nowUtc);

            var result = await svc.GetAppDashboardAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var ov   = result.Data!.appSellerOverView?[0];
            var perf = result.Data!.appSellerPerformance?[0];

            Console.WriteLine($"  Out — OverView: NewOrder={ov?.NewOrderCnt}, Ship={ov?.ShippedCnt}, Replied={ov?.RepliedCnt}, Pickup={ov?.PickupCnt}");
            Console.WriteLine($"  Out — Perf: Sales={perf?.SalesAmount}, Qty={perf?.TotalOrderQty}");
            Console.WriteLine($"  Expected: NewOrder={expected.NewOrderCnt}, Ship={expected.ShippedCnt}, Replied={expected.RepliedCnt}, Pickup={expected.PickupCnt}, Sales={expected.SalesAmount}, Qty={expected.TotalOrderQty}");

            Check("NewOrderCnt",   ov?.NewOrderCnt,   expected.NewOrderCnt);
            Check("ShippedCnt",    ov?.ShippedCnt,    expected.ShippedCnt);
            Check("RepliedCnt",    ov?.RepliedCnt,    expected.RepliedCnt);
            Check("PickupCnt",     ov?.PickupCnt,     expected.PickupCnt);
            Check("SalesAmount",   perf?.SalesAmount, expected.SalesAmount);
            Check("TotalOrderQty", perf?.TotalOrderQty, expected.TotalOrderQty);
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
