using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S23_GetHomeToDoOverViewScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S23: Search 1 — GetHomeToDoOverView ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // 30 筆範圍：2026-04-22 ~ 2026-04-25（涵蓋全部 30 筆，驗證 aggregate 邏輯）
            // 註：GoldenRecipe Search_1 In 為 start=end 同毫秒（production 跑出 27/40），
            //     30 筆下同毫秒區間查不到任何 doc，故改用涵蓋區間驗證 BLL 邏輯本身。
            var req = new OrderSearchRequest
            {
                CuamCid = 528672,
                SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                SearchEndDate   = new DateTime(2026, 4, 25, 0, 0, 0, DateTimeKind.Utc),
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, start={req.SearchStartDate:O}, end={req.SearchEndDate:O}");

            var result = await svc.GetHomeToDoOverViewAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var bp   = data.BuyerPerformance?[0];
            var sp   = data.SellerPerformance?[0];

            Console.WriteLine($"  Out — BuyerPerformance: orderCount={bp?.OrderCount}, pickupCount={bp?.PickupCount}");
            Console.WriteLine($"  Out — SellerPerformance: orderCount={sp?.OrderCount}, sendCount={sp?.SendCount}, salesAmt={sp?.SalesAmt}");

            // 30 筆實測值（CUN9101 範例 4/22~4/24 區間下）
            Check("BuyerPerformance.OrderCount",  bp?.OrderCount,  21);
            Check("BuyerPerformance.PickupCount",  bp?.PickupCount,  1);
            Check("SellerPerformance.OrderCount",  sp?.OrderCount,  21);
            Check("SellerPerformance.SendCount",   sp?.SendCount,    9);
            Check("SellerPerformance.SalesAmt",    sp?.SalesAmt,  2882);
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
