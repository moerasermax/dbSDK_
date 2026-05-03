using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace CPF.Sandbox.IntegrationTests.DataFactory
{
    /// <summary>
    /// 從 TestDataset 反算每個 Search_X 的 expected 值。
    /// 必須 mirror BLL 的 query / aggregate 邏輯（PurchaseOrderQuery 等）。
    /// 修 BLL 邏輯時要同步修這裡，否則 expected 會 drift。
    /// </summary>
    public static class ExpectedValueCalculator
    {
        // ===== BLL Query Predicates (mirror OrderSearchDal.Query.cs) =====

        /// <summary>PurchaseOrderQuery: status NOT IN (00,11,12,1X)</summary>
        public static bool IsPurchaseOrder(OrderDocument d)
            => d.CoomStatus != null && !new[] { "00", "11", "12", "1X" }.Contains(d.CoomStatus);

        /// <summary>OrderStateFinishQuery: esmm_status == "30"</summary>
        public static bool IsOrderStateFinish(OrderDocument d) => d.EsmmStatus == "30";

        /// <summary>SellerOverviewShippingQuery: coom_status == "30"</summary>
        public static bool IsSellerShipping(OrderDocument d) => d.CoomStatus == "30";

        /// <summary>OrderStateDealWithQuery: status=10 + (EXISTS pay_datetime OR payment_type=1)</summary>
        public static bool IsOrderStateDealWith(OrderDocument d)
            => d.CoomStatus == "10"
               && (d.CoocPaymentPayDatetime.HasValue || d.CoocPaymentType == "1");

        /// <summary>OrderStateToshipForSellerQuery: esmm_status=01 + payment</summary>
        public static bool IsOrderStateToshipForSeller(OrderDocument d)
            => d.EsmmStatus == "01"
               && (d.CoocPaymentPayDatetime.HasValue || d.CoocPaymentType == "1");

        /// <summary>SellerQaNeverReplyQuery: seller_qa_never_reply_count > 0</summary>
        public static bool IsSellerQaNeverReply(OrderDocument d)
            => d.SellerQaNeverReplyCount > 0;

        /// <summary>EsmlStatusWaitPickQuery: esmm_status=20 + leavestoredate <= UtcNow+3d</summary>
        public static bool IsEsmlStatusWaitPick(OrderDocument d, DateTime nowForThreshold)
        {
            var threshold = nowForThreshold.AddDays(3).Date;
            return d.EsmmStatus == "20"
                   && d.EsmmLeaveStoreDateB.HasValue
                   && d.EsmmLeaveStoreDateB.Value <= threshold;
        }

        public static bool DateInRange(OrderDocument d, DateTime start, DateTime end)
            => d.CoomCreateDatetime.HasValue
               && d.CoomCreateDatetime.Value >= start
               && d.CoomCreateDatetime.Value <= end;

        // ===== Search 1: GetHomeToDoOverView =====

        public class Search1Expected
        {
            public int BuyerOrderCount { get; init; }
            public int BuyerPickupCount { get; init; }
            public int SellerOrderCount { get; init; }
            public int SellerSendCount { get; init; }
            public int SellerSalesAmt { get; init; }
        }

        public static Search1Expected Calculate_Search1(
            TestDataset dataset, int cuamCid, int memSid, DateTime start, DateTime end)
        {
            var inRange = dataset.Documents.Where(d => DateInRange(d, start, end)).ToList();

            var buyerScope  = inRange.Where(d => d.CoocMemSid == memSid).ToList();
            var sellerScope = inRange.Where(d => d.CoomCuamCid == cuamCid).ToList();

            var sellerPurchase = sellerScope.Where(IsPurchaseOrder).ToList();

            return new Search1Expected
            {
                BuyerOrderCount   = buyerScope.Count(IsPurchaseOrder),
                BuyerPickupCount  = buyerScope.Count(IsOrderStateFinish),
                SellerOrderCount  = sellerPurchase.Count,
                SellerSendCount   = sellerScope.Count(IsSellerShipping),
                SellerSalesAmt    = sellerPurchase.Sum(d => d.CoomRcvTotalAmt ?? 0),
            };
        }

        // ===== Search 2 / 3: SearchBy Seller / Buyer =====

        public class Search23Expected
        {
            public long Total { get; init; }
            public string? FirstCoomNo { get; init; }
        }

        /// <summary>
        /// Search 2/3：filter by ownerField + 日期 + 自訂 sort。
        /// 不過濾 status（BLL 沒套 PurchaseOrderQuery）。
        /// BLL sort 規則：依 OrderSorts 順序 + tie-breaker coom_no asc。
        /// </summary>
        public static Search23Expected Calculate_Search23(
            TestDataset dataset,
            Func<OrderDocument, bool> ownerFilter,
            DateTime start, DateTime end,
            (Func<OrderDocument, object?> KeySelector, bool Desc)[] sorts,
            bool addTieBreakerCoomNoAsc)
        {
            var matched = dataset.Documents
                .Where(d => DateInRange(d, start, end))
                .Where(ownerFilter)
                .ToList();

            IOrderedEnumerable<OrderDocument>? ordered = null;
            for (int i = 0; i < sorts.Length; i++)
            {
                var (key, desc) = sorts[i];
                if (ordered == null)
                    ordered = desc ? matched.OrderByDescending(key) : matched.OrderBy(key);
                else
                    ordered = desc ? ordered.ThenByDescending(key) : ordered.ThenBy(key);
            }
            if (addTieBreakerCoomNoAsc)
            {
                ordered = ordered == null
                    ? matched.OrderBy(d => d.CoomNo)
                    : ordered.ThenBy(d => d.CoomNo);
            }

            var sorted = ordered?.ToList() ?? matched;

            return new Search23Expected
            {
                Total = matched.Count,
                FirstCoomNo = sorted.FirstOrDefault()?.CoomNo,
            };
        }

        // ===== Search 4: GetAppDashboard =====

        public class Search4Expected
        {
            public int NewOrderCnt { get; init; }
            public int ShippedCnt { get; init; }
            public int RepliedCnt { get; init; }
            public int PickupCnt { get; init; }
            public int SalesAmount { get; init; }
            public int TotalOrderQty { get; init; }
        }

        /// <summary>
        /// Search 4：BLL 用 UtcNow-90d window，traceCalc 也用同樣 UtcNow（毫秒差不會跨日）
        /// </summary>
        public static Search4Expected Calculate_Search4(
            TestDataset dataset, int cuamCid, DateTime nowUtc)
        {
            var end = nowUtc;
            var start = end.AddDays(-90);

            var scope = dataset.Documents
                .Where(d => DateInRange(d, start, end))
                .Where(d => d.CoomCuamCid == cuamCid)
                .ToList();

            var purchase = scope.Where(IsPurchaseOrder).ToList();

            return new Search4Expected
            {
                NewOrderCnt   = scope.Count(IsOrderStateDealWith),
                ShippedCnt    = scope.Count(IsOrderStateToshipForSeller),
                RepliedCnt    = scope.Count(IsSellerQaNeverReply),
                PickupCnt     = scope.Count(d => IsEsmlStatusWaitPick(d, nowUtc)),
                SalesAmount   = purchase.Sum(d => d.CoomRcvTotalAmt ?? 0),
                TotalOrderQty = purchase.Count,
            };
        }

        // ===== Search 5 / 6: GetAppSalesToday / Week =====

        public class Search56Expected
        {
            public int TotalAmount { get; init; }
            public int TotalOrderCnt { get; init; }
            public int ShipmentsCnt { get; init; }
            public Dictionary<string, int> SalesTrend { get; init; } = new();
            public string? TopProductCgdmid { get; init; }
            public int TopProductTotalSales { get; init; }
        }

        /// <summary>
        /// Search 5/6：AppSalesMetrics + Trend + ProductRanking
        /// - Outer filter: CoomCuamCid + 日期區間（任何 status）
        /// - TotalOrderCnt = COUNT(PurchaseOrderQuery 內)
        /// - TotalAmount = SUM(CoomRcvTotalAmt) PurchaseOrderQuery 內
        /// - ShipmentsCnt = COUNT(SellerOverviewShippingQuery)
        /// - SalesTrend = DateHistogram by hour/day with TimeZone(+08:00) inside PurchaseOrderQuery
        /// - ProductRanking = nested cood_items terms by cgdd_id, sum cood_qty
        /// </summary>
        public static Search56Expected Calculate_Search56(
            TestDataset dataset, int cuamCid, DateTime start, DateTime end, bool isHourly)
        {
            var scope = dataset.Documents
                .Where(d => DateInRange(d, start, end))
                .Where(d => d.CoomCuamCid == cuamCid)
                .ToList();

            var purchase = scope.Where(IsPurchaseOrder).ToList();

            // Trend: bucket by Taiwan time (UTC+8)
            var taiwanTz = TimeSpan.FromHours(8);
            var trend = purchase
                .GroupBy(d =>
                {
                    var taipei = (d.CoomCreateDatetime!.Value).Add(taiwanTz);
                    return isHourly ? taipei.ToString("HH") : taipei.ToString("MM/dd");
                })
                .ToDictionary(g => g.Key, g => g.Sum(d => d.CoomRcvTotalAmt ?? 0));

            // ProductRanking: 對 scope 全部（不過濾 status）做 cood_items.cgdd_id 聚合
            // (BLL outer filter 是 OrderInfoQuery(cuamCid+date)，沒套 PurchaseOrderQuery)
            var ranking = scope
                .SelectMany(d => d.CoodItems ?? Array.Empty<CoodItems>(),
                            (d, item) => new { item.CgddId, item.CgddCgdmid, Qty = item.CoodQty ?? 0 })
                .Where(x => !string.IsNullOrEmpty(x.CgddId))
                .GroupBy(x => x.CgddId!)
                .Select(g => new
                {
                    CgddId = g.Key,
                    Cgdmid = g.First().CgddCgdmid,
                    TotalQty = g.Sum(x => x.Qty)
                })
                .OrderByDescending(x => x.TotalQty)
                .ThenBy(x => x.CgddId)  // tie-break for determinism
                .ToList();

            var top = ranking.FirstOrDefault();

            return new Search56Expected
            {
                TotalAmount    = purchase.Sum(d => d.CoomRcvTotalAmt ?? 0),
                TotalOrderCnt  = purchase.Count,
                ShipmentsCnt   = scope.Count(IsSellerShipping),
                SalesTrend     = trend,
                TopProductCgdmid     = top?.Cgdmid,
                TopProductTotalSales = top?.TotalQty ?? 0,
            };
        }

        // ===== Search 7: GetUserCgdmData =====

        public class Search7Expected
        {
            public int CuamCid { get; init; }
            public List<(string CgdmId, string CgdmUpdateDatetime)> Cgdm { get; init; } = new();
        }

        /// <summary>
        /// Search 7：nested cood_items by cgdmid + max _ord_modify_date
        /// 注意：用 orders-* wildcard 跨全部 index，filter 只 cuamCid（沒日期）
        /// terms agg 預設按 doc_count desc，size=10000 全列
        /// </summary>
        public static Search7Expected Calculate_Search7(TestDataset dataset, int cuamCid)
        {
            var scope = dataset.Documents
                .Where(d => d.CoomCuamCid == cuamCid)
                .ToList();

            var cgdmGroups = scope
                .SelectMany(d => (d.CoodItems ?? Array.Empty<CoodItems>())
                    .Where(it => !string.IsNullOrEmpty(it.CgddCgdmid))
                    .Select(it => new { Doc = d, Item = it }))
                .GroupBy(x => x.Item.CgddCgdmid!)
                .Select(g => new
                {
                    CgdmId = g.Key,
                    DocCount = g.Count(),  // ES nested terms 用 nested doc_count
                    MaxModifyDate = g.Where(x => x.Doc.OrdModifyDate.HasValue)
                                     .Max(x => (long?)x.Doc.OrdModifyDate),
                })
                .OrderByDescending(x => x.DocCount)
                .ThenBy(x => x.CgdmId)  // tie-breaker for determinism
                .ToList();

            return new Search7Expected
            {
                CuamCid = cuamCid,
                Cgdm = cgdmGroups.Select(x => (
                    x.CgdmId,
                    CgdmUpdateDatetime: x.MaxModifyDate.HasValue
                        ? DateTimeOffset.FromUnixTimeMilliseconds(x.MaxModifyDate.Value)
                            .LocalDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fff")
                        : string.Empty
                )).ToList()
            };
        }
    }
}
