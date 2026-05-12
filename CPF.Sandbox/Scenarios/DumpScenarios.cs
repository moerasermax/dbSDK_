using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace CPF.Sandbox.Scenarios
{
    internal static class DumpScenarios
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };

        internal static async Task DumpAsync(int searchNo)
        {
            var svc = SearchSdkSetup.Build();

            // 對齊客戶 GoldenRecipe sample:cuamCid=528672 / 04-22~04-29 區間
            object payload = searchNo switch
            {
                1 => await svc.GetHomeToDoOverViewAsync(new GetHomeToDoOverviewModel
                {
                    CuamCid = 528672,
                    SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    SearchEndDate = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                }),
                2 => await svc.SearchBySellerAsync(new SearchOrderInfoBySellerIdModel
                {
                    PageIndex = 0,
                    PageSize = 10,
                    CuamCid = 528672,
                    OrderDateStart = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    OrderDateEnd = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                }),
                3 => await svc.SearchByBuyerAsync(new SearchOrderInfoByBuyerIdModel
                {
                    PageIndex = 0,
                    PageSize = 10,
                    MemSid = 528672,
                    OrderDateStart = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    OrderDateEnd = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                }),
                4 => await svc.GetAppDashboardAsync(new GetAppDashboardOverviewModel
                {
                    CuamCid = 528672,
                    SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    SearchEndDate = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                }),
                5 => await svc.GetAppSalesTodayAsync(new GetAppSalesMetricsModel
                {
                    CuamCid = 528672,
                    SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    SearchEndDate = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                    DateStartPoP = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
                    DateEndPoP = new DateTime(2026, 4, 21, 23, 59, 59, DateTimeKind.Utc),
                    DateRangeType = DateRangeType.Today,
                }),
                6 => await svc.GetAppSalesWeekAsync(new GetAppSalesMetricsModel
                {
                    CuamCid = 528672,
                    SearchStartDate = new DateTime(2026, 4, 22, 0, 0, 0, DateTimeKind.Utc),
                    SearchEndDate = new DateTime(2026, 4, 29, 23, 59, 59, DateTimeKind.Utc),
                    DateStartPoP = new DateTime(2026, 4, 15, 0, 0, 0, DateTimeKind.Utc),
                    DateEndPoP = new DateTime(2026, 4, 21, 23, 59, 59, DateTimeKind.Utc),
                    DateRangeType = DateRangeType.SetWeek,
                }),
                7 => await svc.GetUserCgdmDataAsync(new SearchUserCGoodsMModel
                {
                    CuamCid = 528672,
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(searchNo), $"Search_{searchNo} 不存在 (僅支援 1-7)")
            };

            Console.WriteLine($"=== dump-s{searchNo} ({DescribeMethod(searchNo)}) ===");
            Console.WriteLine(JsonSerializer.Serialize(payload, JsonOpts));
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
