using System.Text.Json;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using CPFEnum = PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;

namespace CPF.Sandbox.Scenarios
{
    /// <summary>
    /// Phase 2 Search Scenario Suite (S23-S29)
    /// 統一封裝 Search 1-7 的測試腳本與驗證邏輯
    /// </summary>
    public static class P2_SearchScenarioSuite
    {
        // ==========================================
        // 統一 Check 輔助方法
        // ==========================================
        public static void Check<T>(string name, T? actual, T expected)
        {
            var pass = EqualityComparer<T>.Default.Equals(actual, expected);
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected={expected}, actual={actual}");
        }

        public static void CheckTrue(string name, bool actual)
        {
            var pass = actual == true;
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected=true, actual={actual}");
        }

        public static void CheckFalse(string name, bool actual)
        {
            var pass = actual == false;
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected=false, actual={actual}");
        }

        public static void CheckNotNull(string name, object? actual)
        {
            var pass = actual != null;
            Console.WriteLine($"  {(pass ? "✅ PASS" : "❌ FAIL")} {name}: expected=not null, actual={actual?.GetType().Name ?? "null"}");
        }

        // ==========================================
        // Search 1: GetHomeToDoOverView (S23)
        // ==========================================
        /// <summary>
        /// [S23] Search 1 - GetHomeToDoOverView
        /// 驗證首頁待辦事項統計邏輯（買家視角 + 賣家視角）
        /// </summary>
        public static async Task RunSearch1Async(bool verbose = false)
        {
            PrintHeader("S23: Search 1 — GetHomeToDoOverView");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest
            {
                CuamCid = 528672,
                SearchStartDate = new DateTime(2026, 5, 4, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 5, 5, 23, 59, 59, DateTimeKind.Utc),
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, start={req.SearchStartDate:O}, end={req.SearchEndDate:O}");

            var result = await svc.GetHomeToDoOverViewAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var bp = data.BuyerPerformance;
            var sp = data.SellerPerformance;

            if (verbose)
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                Console.WriteLine(json);
            }

            Console.WriteLine($"  Out — BuyerPerformance: orderCount={bp?.OrderCount}, pickupCount={bp?.PickupCount}");
            Console.WriteLine($"  Out — SellerPerformance: orderCount={sp?.OrderCount}, sendCount={sp?.SendCount}, salesAmt={sp?.SalesAmt}");

            Check("BuyerPerformance.OrderCount", bp?.OrderCount, 15);
            Check("BuyerPerformance.PickupCount", bp?.PickupCount, 1);
            Check("SellerPerformance.OrderCount", sp?.OrderCount, 24);
            Check("SellerPerformance.SendCount", sp?.SendCount, 8);
            Check("SellerPerformance.SalesAmt", sp?.SalesAmt, 138);
        }

        // ==========================================
        // Search 2: SearchBySeller (S24)
        // ==========================================
        /// <summary>
        /// [S24] Search 2 - SearchBySeller
        /// 驗證賣家視角訂單搜尋與排序邏輯
        /// </summary>
        public static async Task RunSearch2Async(bool verbose = false)
        {
            PrintHeader("S24: Search 2 — SearchBySeller");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest
            {
                CuamCid = 528672,
                SearchStartDate = new DateTime(2026, 5, 4, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 5, 5, 23, 59, 59, DateTimeKind.Utc),
                OrderState = PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum.OrderState.DealWith, // 處理中
                // 對齊 Golden Search_2 In 期望:OrderSorts=[1,3] = [CoomCreateDatetimeDesc, CoomNoDesc]
                Sorts = new[] { CPFEnum.OrderSort.CoomCreateDatetimeDesc, CPFEnum.OrderSort.CoomNoDesc },
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, range={req.SearchStartDate:yyyy-MM-dd}~{req.SearchEndDate:yyyy-MM-dd}, orderState={req.OrderState}");

            var result = await svc.SearchBySellerAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var firstOrder = data.OrderInfos?.FirstOrDefault();
            string? firstCoomNo = firstOrder?.COrderM?.CoomNo;

            if (verbose)
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                Console.WriteLine(json);
            }

            Console.WriteLine($"  Out — Total={data.Total}, first={firstCoomNo}");

            Check("Total", data.Total, 5);
        }

        // ==========================================
        // Search 3: SearchByBuyer (S25)
        // ==========================================
        /// <summary>
        /// [S25] Search 3 - SearchByBuyer
        /// 驗證買家視角訂單搜尋與排序邏輯
        /// </summary>
        public static async Task RunSearch3Async(bool verbose = false)
        {
            PrintHeader("S25: Search 3 — SearchByBuyer");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest
            {
                MemSid = 528672,
                SearchStartDate = new DateTime(2026, 5, 4, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 5, 5, 15, 59, 59, DateTimeKind.Utc),
                OrderState = CPFEnum.OrderState.DealWith, // 補上 OrderState 過濾
                // 對齊 Golden Search_3 In 期望:OrderSorts=[1,3]
                Sorts = new[] { CPFEnum.OrderSort.CoomCreateDatetimeDesc, CPFEnum.OrderSort.CoomNoDesc },
            };
            Console.WriteLine($"  In: memSid={req.MemSid}, range={req.SearchStartDate:yyyy-MM-dd}~{req.SearchEndDate:yyyy-MM-dd}, orderState={req.OrderState}");

            var result = await svc.SearchByBuyerAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var firstOrder = data.OrderInfos?.FirstOrDefault();
            string? firstCoomNo = firstOrder?.COrderM?.CoomNo;

            if (verbose)
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All) });
                Console.WriteLine(json);
            }

            Console.WriteLine($"  Out — Total={data.Total}, first={firstCoomNo}");

            Check("Total", data.Total, 5);
        }

        // ==========================================
        // Search 4: GetAppDashboard (S26)
        // ==========================================
        /// <summary>
        /// [S26] Search 4 - GetAppDashboard
        /// 驗證 App 儀表板總覽（固定 90 天區間）
        /// </summary>
        public static async Task RunSearch4Async()
        {
            PrintHeader("S26: Search 4 — GetAppDashboard");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest { CuamCid = 528672 };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}");

            var result = await svc.GetAppDashboardAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            var ov = data.AppSellerOverView;
            var perf = data.AppSellerPerformance;

            Console.WriteLine($"  Out — OverView: newOrderCnt={ov?.NewOrderCnt}, shippedCnt={ov?.ShippedCnt}, repliedCnt={ov?.RepliedCnt}, pickupCnt={ov?.PickupCnt}");
            Console.WriteLine($"  Out — Performance: salesAmount={perf?.SalesAmount}, totalOrderQty={perf?.TotalOrderQty}");

            Check("NewOrderCnt", ov?.NewOrderCnt, 5);
            Check("ShippedCnt", ov?.ShippedCnt, 1);
            Check("RepliedCnt", ov?.RepliedCnt, 1);
            Check("PickupCnt", ov?.PickupCnt, 0);
            Check("SalesAmount", perf?.SalesAmount, 8171);
            Check("TotalOrderQty", perf?.TotalOrderQty, 14);
        }

        // ==========================================
        // Search 5: GetAppSalesToday (S27)
        // ==========================================
        /// <summary>
        /// [S27] Search 5 - GetAppSalesToday
        /// 驗證 App 銷售指標（本日）+ 趨勢補零 + 排行序號
        /// </summary>
        public static async Task RunSearch5Async()
        {
            PrintHeader("S27: Search 5 — GetAppSalesToday");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest
            {
                CuamCid = 528672,
                SearchStartDate = new DateTime(2026, 5, 4, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 5, 5, 15, 59, 59, DateTimeKind.Utc),
                DateRangeType = CPFEnum.DateRangeType.Today,
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, start={req.SearchStartDate:O}, end={req.SearchEndDate:O}, dateRangeType={req.DateRangeType}");

            var result = await svc.GetAppSalesTodayAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }
            Check("Data not null", result.Data != null, true);
            if (result.Data == null) return;

            var data = result.Data;
            var trendAt16 = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "16")?.Value;
            var topProduct = data.ProductSalesRanking?.FirstOrDefault();

            Console.WriteLine($"  Out — totalAmount={data.TotalAmount}, totalOrderCnt={data.TotalOrderCnt}, shipmentsCnt={data.ShipmentsCnt}");
            Console.WriteLine($"  Out — salesTrend[16]={trendAt16}, topProduct={topProduct?.ProductCgdmid}, productTotalSales={topProduct?.ProductTotalSales}");

            Check("TotalAmount", data.TotalAmount, 8659);
            Check("TotalOrderCnt", data.TotalOrderCnt, 15);
            Check("ShipmentsCnt", data.ShipmentsCnt, 8);
            Check("SalesTrendData.length", data.SalesTrendData?.Count(), 24);
            // ES cood_items nested 無 cood_price 欄位 → 商品銷售排名為空（對齊 Golden Search_5 樣張 productSalesRanking: []）
            Check("ProductSalesRanking.Count", data.ProductSalesRanking?.Count() ?? 0, 0);
        }

        // ==========================================
        // Search 6: GetAppSalesWeek (S28)
        // ==========================================
        /// <summary>
        /// [S28] Search 6 - GetAppSalesWeek
        /// 驗證 App 銷售指標（本週）+ 趨勢補零 + 排行序號
        /// </summary>
        public static async Task RunSearch6Async()
        {
            PrintHeader("S28: Search 6 — GetAppSalesWeek");

            var svc = SearchSdkSetup.Build();
            // 對齊 Golden Search_6 樣張 in:4/27 16:00Z ~ 5/05 15:59Z + DateRangeType=SetWeek
            var req = new OrderSearchRequest
            {
                CuamCid = 528672,
                SearchStartDate = new DateTime(2026, 4, 27, 16, 0, 0, DateTimeKind.Utc),
                SearchEndDate = new DateTime(2026, 5, 5, 15, 59, 59, DateTimeKind.Utc),
                DateRangeType = CPFEnum.DateRangeType.SetWeek,
            };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}, start={req.SearchStartDate:O}, end={req.SearchEndDate:O}, dateRangeType={req.DateRangeType}");

            var result = await svc.GetAppSalesWeekAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }
            Check("Data not null", result.Data != null, true);
            if (result.Data == null) return;

            var data = result.Data;
            var trend0505 = data.SalesTrendData?.FirstOrDefault(t => t.TimePane == "05/05")?.Value;
            var topProduct = data.ProductSalesRanking?.FirstOrDefault();

            Console.WriteLine($"  Out — totalAmount={data.TotalAmount}, totalOrderCnt={data.TotalOrderCnt}, shipmentsCnt={data.ShipmentsCnt}");
            Console.WriteLine($"  Out — salesTrend[05/05]={trend0505}");
            Console.WriteLine($"  Out — topProduct={topProduct?.ProductCgdmid}, productTotalSales={topProduct?.ProductTotalSales}");

            // 期望值對齊 Golden Search_6 樣張 (測試資料訂單只在 05/05,所以 SetWeek vs Today 同值)
            Check("TotalAmount", data.TotalAmount, 8659);
            Check("TotalOrderCnt", data.TotalOrderCnt, 15);
            Check("ShipmentsCnt", data.ShipmentsCnt, 8);
            Check("SalesTrendData.length", data.SalesTrendData?.Count(), 7);
            // ES cood_items nested 無 cood_price 欄位 → 商品銷售排名為空(對齊 Golden Search_6 樣張)
            Check("ProductSalesRanking.Count", data.ProductSalesRanking?.Count() ?? 0, 0);
        }

        // ==========================================
        // Search 7: GetUserCgdmData (S29)
        // ==========================================
        /// <summary>
        /// [S29] Search 7 - GetUserCgdmData
        /// 驗證取得賣家 cgdm 資料
        /// </summary>
        public static async Task RunSearch7Async()
        {
            PrintHeader("S29: Search 7 — GetUserCgdmData");

            var svc = SearchSdkSetup.Build();
            var req = new OrderSearchRequest { CuamCid = 528672 };
            Console.WriteLine($"  In: cuamCid={req.CuamCid}");

            var result = await svc.GetUserCgdmDataAsync(req);
            if (!result.IsSuccess) { Console.WriteLine($"  ❌ ERROR: {result.Msg}"); return; }

            var data = result.Data!;
            Console.WriteLine($"  Out — CuamCid={data.CuamCid}, Cgdm count={data.Cgdm?.Length}");

            for (int i = 0; i < (data.Cgdm?.Length ?? 0); i++)
            {
                var cgdm = data.Cgdm![i];
                Console.WriteLine($"  Out — cgdm[{i}]: {cgdm.CgdmId} @ '{cgdm.CgdmUpdateDatetime}'");
            }

            Check("CuamCid", data.CuamCid, 528672);
            Check("Cgdm count", data.Cgdm?.Length, 2);
            Check("Cgdm[0].cgdmId", data.Cgdm?[0].CgdmId, "GM2508260014245");
            Check("Cgdm[1].cgdmId", data.Cgdm?[1].CgdmId, "GM2512180014259");
        }

        // ==========================================
        // 私有輔助方法
        // ==========================================
        private static void PrintHeader(string title)
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine($"=== {title} ===");
            Console.WriteLine("========================================");
        }
    }
}