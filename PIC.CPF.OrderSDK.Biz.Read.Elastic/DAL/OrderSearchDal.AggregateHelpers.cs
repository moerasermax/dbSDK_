using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Fluent;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    public partial class OrderSearchDal
    {
        #region Sub-agg name constants

        private const string BO_Unpaid = "BO_Unpaid";
        private const string BO_Toship = "BO_Toship";
        private const string BO_ToFinish = "BO_ToFinish";
        private const string BO_Cancel = "BO_Cancel";
        private const string BO_SellerQaNeverReply = "BO_SellerQaNeverReply";
        private const string BO_Finish = "BO_Finish";
        private const string BO_BuyerReturnReq = "BO_BuyerReturnReq";

        private const string BP_OrderCount = "BP_OrderCount";
        private const string BP_PickupCount = "BP_PickupCount";

        private const string SO_DealWith = "SO_DealWith";
        private const string SO_Toship = "SO_Toship";
        private const string SO_Shipping = "SO_Shipping";
        private const string SO_WaitReturn = "SO_WaitReturn";
        private const string SO_BuyerQaNeverReply = "SO_BuyerQaNeverReply";
        private const string SO_SellerReturnReq = "SO_SellerReturnReq";

        private const string SP_OrderCount = "SP_OrderCount";
        private const string SP_SendCount = "SP_SendCount";
        private const string SP_SalesAmt = "SP_SalesAmt";

        private const string ASO_NewOrderCnt = "ASO_NewOrderCnt";
        private const string ASO_ShippedCnt = "ASO_ShippedCnt";
        private const string ASO_RepliedCnt = "ASO_RepliedCnt";
        private const string ASO_PickupCnt = "ASO_PickupCnt";

        private const string ASP_TotalOrderQty = "ASP_TotalOrderQty";
        private const string ASP_SalesAmount = "ASP_SalesAmount";

        // Trend sub-agg suffixes
        private const string SMT_Hist = "SMT_Hist";
        private const string SMT_Valid = "SMT_Valid";
        private const string SMT_SalesAmt = "SMT_SalesAmt";

        // Product Ranking sub-agg suffixes
        private const string PR_Nested = "PR_Nested";
        private const string PR_Terms = "PR_Terms";
        private const string PR_QtySum = "PR_QtySum";
        private const string PR_Cgdmid = "PR_Cgdmid";
        private const string PR_Name = "PR_Name";
        private const string PR_Cgdsid = "PR_Cgdsid";
        private const string PR_ImgPath = "PR_ImgPath";

        #endregion

        #region Aggregation Builders

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> BuyerOverviewAggregates(
            string name, BuyerOverviewAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoocMemSid = model.CoocMemSid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{BO_Unpaid}", f => f.Filter(OrderStateUnpaidQuery()))
                    .Add($"{name}_{BO_Toship}", f => f.Filter(OrderStateToshipForBuyerQuery()))
                    .Add($"{name}_{BO_ToFinish}", f => f.Filter(OrderStateToFinishQuery()))
                    .Add($"{name}_{BO_Cancel}", f => f.Filter(OrderStateCancelQuery()))
                    .Add($"{name}_{BO_SellerQaNeverReply}", f => f.Filter(SellerQaNeverReplyQuery()))
                    .Add($"{name}_{BO_Finish}", f => f.Filter(OrderStateFinishQuery()))
                    .Add($"{name}_{BO_BuyerReturnReq}", f => f.Filter(OrderStateRtnShippingQuery()))
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> BuyerPerformanceAggregates(
            string name, BuyerPerformanceAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoocMemSid = model.CoocMemSid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{BP_OrderCount}", f => f.Filter(PurchaseOrderQuery()))
                    .Add($"{name}_{BP_PickupCount}", f => f.Filter(OrderStateFinishQuery()))
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> SellerOverviewAggregates(
            string name, SellerOverviewAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CoomCuamCid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{SO_DealWith}", f => f.Filter(OrderStateDealWithQuery()))
                    .Add($"{name}_{SO_Toship}", f => f.Filter(OrderStateToshipForSellerQuery()))
                    .Add($"{name}_{SO_Shipping}", f => f.Filter(OrderStateShippingQuery()))
                    .Add($"{name}_{SO_WaitReturn}", f => f.Filter(SellerOverviewWaitReturnQuery()))
                    .Add($"{name}_{SO_BuyerQaNeverReply}", f => f.Filter(BuyerQaNeverReplyQuery()))
                    .Add($"{name}_{SO_SellerReturnReq}", f => f.Filter(OrderStateRtnShippingQuery()))
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> SellerPerformanceAggregates(
            string name, SellerPerformanceAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CoomCuamCid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{SP_OrderCount}", f => f
                        .Filter(PurchaseOrderQuery())
                        .Aggregations(sa => sa
                            .Add($"{name}_{SP_SalesAmt}", sum => sum
                                .Sum(s => s.Field(o => o.CoomRcvTotalAmt))
                            )
                        )
                    )
                    .Add($"{name}_{SP_SendCount}", f => f.Filter(SellerOverviewShippingQuery()))
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSellerOverviewAggregates(
            string name, AppSellerOverViewAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CoomCuamCid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{ASO_NewOrderCnt}", f => f.Filter(OrderStateDealWithQuery()))
                    .Add($"{name}_{ASO_ShippedCnt}", f => f.Filter(OrderStateToshipForSellerQuery()))
                    .Add($"{name}_{ASO_RepliedCnt}", f => f.Filter(SellerQaNeverReplyQuery()))
                    .Add($"{name}_{ASO_PickupCnt}", f => f.Filter(EsmlStatusWaitPickQuery()))
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSellerPerformanceAggregates(
            string name, AppSellerPerformanceAggregateModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CoomCuamCid,
                OrderDateStart = model.OrderDateStart,
                OrderDateEnd = model.OrderDateEnd
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{ASP_TotalOrderQty}", f => f
                        .Filter(PurchaseOrderQuery())
                        .Aggregations(sa => sa
                            .Add($"{name}_{ASP_SalesAmount}", sum => sum
                                .Sum(s => s.Field(o => o.CoomRcvTotalAmt))
                            )
                        )
                    )
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSalesMetricsPoP(
            string name, AppSalesMetricsModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CuamCid,
                OrderDateStart = model.StartDatePoP,
                OrderDateEnd = model.EndDatePoP
            };
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{SalesMetricsTotalOrderCnt}", fa => fa
                        .Filter(PurchaseOrderQuery())
                        .Aggregations(sa => sa
                            .Add($"{name}_{SalesMetricsTotalAmount}", sum => sum
                                .Sum(s => s.Field(o => o.CoomRcvTotalAmt))
                            )
                        )
                    )
                    .Add($"{name}_{SalesMetricsShipmentsCnt}", fa => fa
                        .Filter(SellerOverviewShippingQuery())
                    )
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSalesMetricsTrend(
            string name, AppSalesMetricsModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CuamCid,
                OrderDateStart = model.SearchStartDate,
                OrderDateEnd = model.SearchEndDate
            };

            bool isHourly = model.DateRangeType == DateRangeType.Today;
            var interval = isHourly ? CalendarInterval.Hour : CalendarInterval.Day;
            var fmt = isHourly ? "HH" : "MM/dd";

            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{SMT_Hist}", hist => hist
                        .DateHistogram(dh => dh
                            .Field("coom_create_datetime")
                            .CalendarInterval(interval)
                            .Format(fmt)
                            .MinDocCount(0)
                            .TimeZone("+08:00")
                        )
                        .Aggregations(histAggs => histAggs
                            .Add($"{name}_{SMT_Valid}", fa => fa
                                .Filter(PurchaseOrderQuery())
                                .Aggregations(validAggs => validAggs
                                    .Add($"{name}_{SMT_SalesAmt}", sum => sum
                                        .Sum(s => s.Field(o => o.CoomRcvTotalAmt))
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }

        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSalesMetricsProductRanking(
            string name, AppSalesMetricsModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CuamCid,
                OrderDateStart = model.SearchStartDate,
                OrderDateEnd = model.SearchEndDate
            };

            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    .Add($"{name}_{PR_Nested}", nested => nested
                        .Nested(n => n.Path("cood_items"))
                        .Aggregations(nestedAggs => nestedAggs
                            .Add($"{name}_{PR_Terms}", terms => terms
                                .Terms(t => t
                                    .Field("cood_items.cgdd_id")
                                    .Size(20)
                                )
                                .Aggregations(termAggs => termAggs
                                    .Add($"{name}_{PR_QtySum}", sum => sum
                                        .Sum(s => s.Field("cood_items.cood_qty"))
                                    )
                                    .Add($"{name}_{PR_Cgdmid}", mt => mt
                                        .Terms(t => t.Field("cood_items.cgdd_cgdmid").Size(1))
                                    )
                                    .Add($"{name}_{PR_Name}", mt => mt
                                        .Terms(t => t.Field("cood_items.cood_name.keyword").Size(1))
                                    )
                                    .Add($"{name}_{PR_Cgdsid}", mt => mt
                                        .Terms(t => t.Field("cood_items.cood_cgdsid").Size(1))
                                    )
                                    .Add($"{name}_{PR_ImgPath}", mt => mt
                                        .Terms(t => t.Field("cood_items.cood_image_path").Size(1))
                                    )
                                )
                            )
                        )
                    )
                )
            );
        }

        #endregion

        #region Aggregation Parsers

        private static BuyerOverviewAggregateResultModel? ParseBuyerOverview(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            return new BuyerOverviewAggregateResultModel
            {
                Unpaid = GetFilterCount(sub, $"{name}_{BO_Unpaid}"),
                Toship = GetFilterCount(sub, $"{name}_{BO_Toship}"),
                ToFinish = GetFilterCount(sub, $"{name}_{BO_ToFinish}"),
                Cancel = GetFilterCount(sub, $"{name}_{BO_Cancel}"),
                SellerQaNeverReply = GetFilterCount(sub, $"{name}_{BO_SellerQaNeverReply}"),
                Finish = GetFilterCount(sub, $"{name}_{BO_Finish}"),
                BuyerReturnReq = GetFilterCount(sub, $"{name}_{BO_BuyerReturnReq}"),
            };
        }

        private static BuyerPerformanceAggregateResultModel? ParseBuyerPerformance(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            return new BuyerPerformanceAggregateResultModel
            {
                OrderCount = GetFilterCount(sub, $"{name}_{BP_OrderCount}"),
                PickupCount = GetFilterCount(sub, $"{name}_{BP_PickupCount}"),
            };
        }

        private static SellerOverviewAggregateResultModel? ParseSellerOverview(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            return new SellerOverviewAggregateResultModel
            {
                DealWith = GetFilterCount(sub, $"{name}_{SO_DealWith}"),
                Toship = GetFilterCount(sub, $"{name}_{SO_Toship}"),
                Shipping = GetFilterCount(sub, $"{name}_{SO_Shipping}"),
                WaitReturn = GetFilterCount(sub, $"{name}_{SO_WaitReturn}"),
                BuyerQaNeverReply = GetFilterCount(sub, $"{name}_{SO_BuyerQaNeverReply}"),
                SellerReturnReq = GetFilterCount(sub, $"{name}_{SO_SellerReturnReq}"),
            };
        }

        private static SellerPerformanceAggregateResultModel? ParseSellerPerformance(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            sub.TryGetValue($"{name}_{SP_OrderCount}", out var orderAgg);
            var orderFilter = orderAgg as FilterAggregate;
            return new SellerPerformanceAggregateResultModel
            {
                OrderCount = (int)(orderFilter?.DocCount ?? 0),
                SalesAmt = GetNestedSum(orderFilter?.Aggregations, $"{name}_{SP_SalesAmt}"),
                SendCount = GetFilterCount(sub, $"{name}_{SP_SendCount}"),
            };
        }

        private static AppSellerOverViewAggregateResultModel? ParseAppSellerOverview(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            return new AppSellerOverViewAggregateResultModel
            {
                NewOrderCnt = GetFilterCount(sub, $"{name}_{ASO_NewOrderCnt}"),
                ShippedCnt = GetFilterCount(sub, $"{name}_{ASO_ShippedCnt}"),
                RepliedCnt = GetFilterCount(sub, $"{name}_{ASO_RepliedCnt}"),
                PickupCnt = GetFilterCount(sub, $"{name}_{ASO_PickupCnt}"),
            };
        }

        private static AppSellerPerformanceAggregateResultModel? ParseAppSellerPerformance(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            sub.TryGetValue($"{name}_{ASP_TotalOrderQty}", out var orderAgg);
            var orderFilter = orderAgg as FilterAggregate;
            return new AppSellerPerformanceAggregateResultModel
            {
                TotalOrderQty = (int)(orderFilter?.DocCount ?? 0),
                SalesAmount = GetNestedSum(orderFilter?.Aggregations, $"{name}_{ASP_SalesAmount}"),
            };
        }

        private static AppSalesMetricsResultModel? ParseSalesMetrics(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            sub.TryGetValue($"{name}_{SalesMetricsTotalOrderCnt}", out var orderAgg);
            var orderFilter = orderAgg as FilterAggregate;
            return new AppSalesMetricsResultModel
            {
                TotalOrderCnt = (int)(orderFilter?.DocCount ?? 0),
                TotalAmount = GetNestedSum(orderFilter?.Aggregations, $"{name}_{SalesMetricsTotalAmount}"),
                ShipmentsCnt = GetFilterCount(sub, $"{name}_{SalesMetricsShipmentsCnt}"),
            };
        }

        private static AppSalesMetricsResultModel? ParseSalesMetricsPoP(string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;
            var sub = outer.Aggregations;
            sub.TryGetValue($"{name}_{SalesMetricsTotalOrderCnt}", out var orderAgg);
            var orderFilter = orderAgg as FilterAggregate;
            return new AppSalesMetricsResultModel
            {
                TotalOrderCntPoP = (int)(orderFilter?.DocCount ?? 0),
                TotalAmountPoP = GetNestedSum(orderFilter?.Aggregations, $"{name}_{SalesMetricsTotalAmount}"),
                ShipmentsCntPoP = GetFilterCount(sub, $"{name}_{SalesMetricsShipmentsCnt}"),
            };
        }

        private static AppSalesMetricsResultModel? ParseSalesMetricsTrend(
            string name, DateRangeType dateRangeType, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;

            if (!outer.Aggregations.TryGetValue($"{name}_{SMT_Hist}", out var histAgg) ||
                histAgg is not DateHistogramAggregate hist)
                return null;

            var salesTrend = new List<OrderTrendData>();
            var orderTrend = new List<OrderTrendData>();

            foreach (var bucket in hist.Buckets)
            {
                var timePane = bucket.KeyAsString ?? bucket.Key.ToString();
                int orderCount = 0;
                int salesAmt = 0;

                if (bucket.Aggregations != null &&
                    bucket.Aggregations.TryGetValue($"{name}_{SMT_Valid}", out var validAgg) &&
                    validAgg is FilterAggregate valid)
                {
                    orderCount = (int)valid.DocCount;
                    salesAmt = GetNestedSum(valid.Aggregations, $"{name}_{SMT_SalesAmt}");
                }

                salesTrend.Add(new OrderTrendData { TimePane = timePane, Value = salesAmt });
                orderTrend.Add(new OrderTrendData { TimePane = timePane, Value = orderCount });
            }

            return new AppSalesMetricsResultModel
            {
                SalesTrendData = salesTrend,
                OrderTrendData = orderTrend,
            };
        }

        private static AppSalesMetricsResultModel? ParseSalesMetricsProductRanking(
            string name, AggregateDictionary aggs)
        {
            if (!aggs.TryGetValue(name, out var outerAgg) || outerAgg is not FilterAggregate outer || outer.Aggregations == null)
                return null;

            if (!outer.Aggregations.TryGetValue($"{name}_{PR_Nested}", out var nestedAgg) ||
                nestedAgg is not NestedAggregate nested || nested.Aggregations == null)
                return null;

            if (!nested.Aggregations.TryGetValue($"{name}_{PR_Terms}", out var termsAgg) ||
                termsAgg is not StringTermsAggregate terms)
                return null;

            var rankings = terms.Buckets
                .Select(bucket => new
                {
                    Bucket = bucket,
                    TotalSales = bucket.Aggregations != null
                        ? GetNestedSum(bucket.Aggregations, $"{name}_{PR_QtySum}")
                        : 0
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5)
                .Select((x, idx) => new ProductSalesRanking
                {
                    RankingNo = idx + 1,
                    ProductId = x.Bucket.Key.ToString(),
                    ProductTotalSales = x.TotalSales,
                    ProductCgdmid = x.Bucket.Aggregations != null
                        ? GetFirstTermKey(x.Bucket.Aggregations, $"{name}_{PR_Cgdmid}")
                        : null,
                    ProductName = x.Bucket.Aggregations != null
                        ? GetFirstTermKey(x.Bucket.Aggregations, $"{name}_{PR_Name}")
                        : null,
                    ProductCgdsId = x.Bucket.Aggregations != null
                        ? GetFirstTermKey(x.Bucket.Aggregations, $"{name}_{PR_Cgdsid}")
                        : null,
                    ProductImgPath = x.Bucket.Aggregations != null
                        ? GetFirstTermKey(x.Bucket.Aggregations, $"{name}_{PR_ImgPath}")
                        : null,
                })
                .ToList();

            return new AppSalesMetricsResultModel { ProductSalesRanking = rankings };
        }

        #endregion

        #region Parse helper utilities

        private static int GetFilterCount(AggregateDictionary aggs, string key)
        {
            if (!aggs.TryGetValue(key, out var agg) || agg is not FilterAggregate f) return 0;
            return (int)f.DocCount;
        }

        private static int GetNestedSum(AggregateDictionary? aggs, string key)
        {
            if (aggs == null || !aggs.TryGetValue(key, out var agg) || agg is not SumAggregate s) return 0;
            return (int)(s.Value ?? 0);
        }

        private static string? GetFirstTermKey(AggregateDictionary aggs, string key)
        {
            if (!aggs.TryGetValue(key, out var agg) || agg is not StringTermsAggregate terms) return null;
            return terms.Buckets.FirstOrDefault()?.Key.ToString();
        }

        #endregion
    }
}
