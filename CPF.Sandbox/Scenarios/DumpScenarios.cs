using System.Text.Encodings.Web;
using System.Text.Json;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    internal static class DumpScenarios
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        internal static async Task DumpAsync(int searchNo)
        {
            var svc = SearchSdkSetup.Build();
            var req = BuildSampleRequest(searchNo);

            object payload = searchNo switch
            {
                1 => await svc.GetHomeToDoOverViewAsync(req),
                2 => await svc.SearchBySellerAsync(req),
                3 => await svc.SearchByBuyerAsync(req),
                4 => await svc.GetAppDashboardAsync(req),
                5 => await svc.GetAppSalesTodayAsync(req),
                6 => await svc.GetAppSalesWeekAsync(req),
                7 => await svc.GetUserCgdmDataAsync(req),
                _ => throw new ArgumentOutOfRangeException(nameof(searchNo), $"Search_{searchNo} 不存在 (僅支援 1-7)")
            };

            Console.WriteLine($"=== dump-s{searchNo} ({DescribeMethod(searchNo)}) ===");
            Console.WriteLine(JsonSerializer.Serialize(payload, JsonOpts));
        }

        private static OrderSearchRequest BuildSampleRequest(int searchNo)
        {
            // 對齊客戶 CUN9101 GoldenRecipe sample: cuamCid=528672 / 04-22~04-29 區間
            return new OrderSearchRequest
            {
                CuamCid = 528672,
                MemSid = 528672,
                SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                DateStartPoP = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
                DateEndPoP = new DateTime(2026, 4, 21, 23, 59, 59, DateTimeKind.Utc),
                DateRangeType = searchNo == 6 ? DateRangeType.ThisWeek : DateRangeType.Today,
                PageInfo = new OrderSearchPageInfo { PageIndex = 0, PageSize = 10 },
            };
        }

        private static string DescribeMethod(int n) => n switch
        {
            1 => "GetHomeToDoOverView",
            2 => "SearchBySellerId",
            3 => "SearchByBuyerId",
            4 => "GetAppDashboardOverview",
            5 => "GetAppSalesMetrics(Today)",
            6 => "GetAppSalesMetrics(Week)",
            7 => "GetUserCgdmData",
            _ => "Unknown"
        };
    }
}
