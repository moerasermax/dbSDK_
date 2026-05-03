using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    public static class S25_SearchByBuyerScenario
    {
        public static async Task RunAsync()
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== S25: Search 3 — SearchByBuyer ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();

            // GoldenRecipe In  (OrderSorts [1,3] => CoomCreateDatetimeDesc, CoomNoDesc)
            var req = new OrderSearchRequest
            {
                MemSid          = 528672,
                SearchStartDate = new DateTime(2026, 2, 27, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate   = new DateTime(2026, 4, 29, 15, 59, 59, 999, DateTimeKind.Utc),
                PageInfo        = new OrderSearchPageInfo { PageIndex = 0, PageSize = 50 },
                Sorts           = new[] { OrderSort.CoomCreateDatetimeDesc, OrderSort.CoomNoDesc },
                IsQaList        = false,
                BindMembersArray = Array.Empty<int>(),
            };
            Console.WriteLine($"  In: memSid={req.MemSid}, start={req.SearchStartDate:O}, end={req.SearchEndDate:O}, pageSize={req.PageInfo.PageSize}");

            var result = await svc.SearchByBuyerAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data  = result.Data!;
            var first = data.OrderInfos?.Length > 0 ? data.OrderInfos[0] : (System.Text.Json.JsonElement?)null;
            var firstCoomNo = first?.GetProperty("coom_no").GetString();

            Console.WriteLine($"  Out — Total={data.Total}, first coom_no={firstCoomNo}");

            // 30 筆實測值（GoldenRecipe Total=53 為 production mongo 數值；
            // 範例首筆 CM2604290379066 不在 30 筆內）
            Check("Total",              data.Total,   30L);
            Check("OrderInfos not null", data.OrderInfos != null, true);
            Check("First coom_no",      firstCoomNo,  "CM2604240044017");
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
