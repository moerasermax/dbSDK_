using CPF.Sandbox.IntegrationTests.DataFactory;
using CPF.Sandbox.Scenarios;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.Scenarios
{
    public static class E2E_S2_SearchBySeller
    {
        public static async Task RunAsync(TestDataset dataset)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine("=== E2E S2: SearchBySeller ===");
            Console.WriteLine("========================================");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest
            {
                CuamCid = dataset.SellerCuamCid,
                SearchStartDate = dataset.DateRangeStart,
                SearchEndDate   = dataset.DateRangeEnd,
                PageInfo = new OrderSearchPageInfo { PageIndex = 0, PageSize = 50 },
                Sorts = new[] { OrderSort.CoomCreateDatetimeDesc, OrderSort.CoomNoDesc },
                IsQaList = false,
            };

            var expected = ExpectedValueCalculator.Calculate_Search23(
                dataset,
                ownerFilter: d => d.CoomCuamCid == dataset.SellerCuamCid,
                start: dataset.DateRangeStart, end: dataset.DateRangeEnd,
                sorts: new (Func<OrderDocument, object?>, bool)[]
                {
                    (d => d.CoomCreateDatetime ?? DateTime.MinValue, true),
                    (d => d.CoomNo ?? "", true),
                },
                addTieBreakerCoomNoAsc: false  // sort 已含 coom_no
            );

            var result = await svc.SearchBySellerAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var firstCoomNo = data.OrderInfos?.Length > 0
                ? data.OrderInfos[0].GetProperty("coom_no").GetString()
                : null;

            Console.WriteLine($"  Out — Total={data.Total}, first={firstCoomNo}");
            Console.WriteLine($"  Expected: Total={expected.Total}, first={expected.FirstCoomNo}");

            Check("Total",       data.Total,  expected.Total);
            Check("First coom_no", firstCoomNo, expected.FirstCoomNo);
            Console.WriteLine("========================================\n");
        }

        private static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }
    }
}
