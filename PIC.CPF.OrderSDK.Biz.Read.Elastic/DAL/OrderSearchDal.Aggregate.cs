using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.Fluent;
using Elastic.Clients.Elasticsearch.QueryDsl;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Enum;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL
{
    public partial class OrderSearchDal
    {
        // 1. 注入你強大的 dbSDK Repository
        private readonly ElasticRepository<OrderDocument> _elasticRepository;

        public OrderSearchDal(ElasticRepository<OrderDocument> elasticRepository)
        {
            _elasticRepository = elasticRepository;
        }

        #region 主查詢 (Query Translation)

        // 注意：回傳型別從 Func 改為 Action<QueryDescriptor>
        public static Action<QueryDescriptor<OrderDocument>> OrderInfoQuery(OrderInfoQueryModel model)
        {
            var filters = new List<Action<QueryDescriptor<OrderDocument>>>();


            // 訂購單所屬賣家會員編號
            if (model.CoomCuamCid.HasValue)
            {
                filters.Add(CoomCuamCidQuery(model.CoomCuamCid.Value));
                // 是否為問答清單
                if (model.IsQaList.HasValue && model.IsQaList.Value)
                {
                    filters.Add(SellerQaNeverReplyQuery());
                }
            }

            // 訂購人會員編號
            if (model.CoocMemSid.HasValue)
            {
                filters.Add(CoocMemSidQuery(model.CoocMemSid.Value, model.BindMembersArray));
                // 是否為問答清單
                if (model.IsQaList.HasValue && model.IsQaList.Value)
                {
                    filters.Add(BuyerQaNeverReplyQuery());
                }
            }

            // 訂單編號
            if (!string.IsNullOrWhiteSpace(model.CoomNo))
            {
                filters.Add(CoomNoQuery(model.CoomNo));
            }

            // 賣場名稱
            if (!string.IsNullOrWhiteSpace(model.CoomName))
            {
                filters.Add(CoomNameQuery(model.CoomName));
            }

            // 配送編號
            if (!string.IsNullOrWhiteSpace(model.EsmmShipNo))
            {
                filters.Add(EsmmShipNoQuery(model.EsmmShipNo));
            }

            // 商品名稱
            if (!string.IsNullOrWhiteSpace(model.CoodName))
            {
                filters.Add(CoodNameQuery(model.CoodName));
            }

            // 購物車編號
            if (!string.IsNullOrWhiteSpace(model.CoocNo))
            {
                filters.Add(CoocNoQuery(model.CoocNo));
            }

            // 訂單日期 - 開始
            if (model.OrderDateStart.HasValue)
            {
                filters.Add(OrderDateStartQuery(model.OrderDateStart.Value));
            }

            // 訂單日期 - 結束
            if (model.OrderDateEnd.HasValue)
            {
                filters.Add(OrderDateEndQuery(model.OrderDateEnd.Value));
            }

            // 賣場類型
            if (!string.IsNullOrWhiteSpace(model.OrdChannelKindSearchKind))
            {
                filters.Add(OrdChannelKindSearchKindQuery(model.OrdChannelKindSearchKind));
            }

            // 配送方式
            if (!string.IsNullOrWhiteSpace(model.DeliverMethodSearchKind))
            {
                filters.Add(DeliverMethodSearchKindQuery(model.DeliverMethodSearchKind));
            }

            // 使用溫層
            if (!string.IsNullOrWhiteSpace(model.TempTypeSearchKind))
            {
                filters.Add(TempTypeSearchKindQuery(model.TempTypeSearchKind));
            }

            // 訂單狀態
            if (model.OrderState != null)
            {
                switch (model.OrderState)
                {
                    case OrderState.Unpaid:
                        filters.Add(OrderStateUnpaidQuery());
                        break;
                    case OrderState.DealWith:
                        filters.Add(OrderStateDealWithQuery());
                        break;
                    case OrderState.ToshipForSeller:
                        filters.Add(OrderStateToshipForSellerQuery());
                        break;
                    case OrderState.ToshipForBuyer:
                        filters.Add(OrderStateToshipForBuyerQuery());
                        break;
                    case OrderState.Shipping:
                        filters.Add(OrderStateShippingQuery());
                        break;
                    case OrderState.ToFinish:
                        filters.Add(OrderStateToFinishQuery());
                        break;
                    case OrderState.Finish:
                        filters.Add(OrderStateFinishQuery());
                        break;
                    case OrderState.RtnShipping:
                        filters.Add(OrderStateRtnShippingQuery());
                        break;
                    case OrderState.NoShowToDHL:
                        filters.Add(OrderStateNoShowToDHLQuery());
                        break;
                    case OrderState.Exception:
                        filters.Add(OrderStateExceptionQuery());
                        break;
                    case OrderState.Cancel:
                        filters.Add(OrderStateCancelQuery());
                        break;
                    case OrderState.SellerQaNeverReply:
                        filters.Add(SellerQaNeverReplyQuery());
                        break;
                    case OrderState.BuyerQaNeverReply:
                        filters.Add(BuyerQaNeverReplyQuery());
                        break;
                }
            }

            // v8 語法：將 List<Action> 轉為陣列餵給 Filter
            return q => q.Bool(b => b.Filter(filters.ToArray()));
        }

        // 單一條件範例 (Term)
        public static Action<QueryDescriptor<OrderDocument>> CoomCuamCidQuery(int coomCuamCid)
        {
            return q => q.Term(t => t.Field(f => f.CoomCuamCid).Value(coomCuamCid));
        }

        // 複雜條件範例 (Bool / Must / Should) - 未付款
        public static Action<QueryDescriptor<OrderDocument>> OrderStateUnpaidQuery()
        {
            return q => q.Bool(b => b.Must(
                m => m.Term(t => t.Field(f => f.CoomStatus).Value("10")),
                m => m.Bool(innerB => innerB.MustNot(
                    mn => mn.Exists(e => e.Field(f => f.CoocPaymentPayDatetime)),
                    mn => mn.Term(t => t.Field(f => f.CoocPaymentType).Value("1"))
                ))
            ));
        }

        // 複雜條件範例 (Terms 陣列寫法) - 待出貨
        public static Action<QueryDescriptor<OrderDocument>> OrderStateToshipForBuyerQuery()
        {
            return q => q.Bool(b => b.Must(
                m => m.Terms(t => t
                    .Field(f => f.CoomStatus)
                    .Terms(new TermsQueryField(new FieldValue[] { "10", "20" }))
                ),
                m => m.Bool(innerB => innerB.Should(
                    s => s.Exists(e => e.Field(f => f.CoocPaymentPayDatetime)),
                    s => s.Term(t => t.Field(f => f.CoocPaymentType).Value("1"))
                ).MinimumShouldMatch(1))
            ));
        }
        #endregion



        #region 主呼叫端點 (API Entry Point)

        // 這是要給 BLL 呼叫的主方法，完全非同步化！
        public async Task<AggregateOrderInfoResultModel> AggregateOrderInfoAsync(
    BuyerOverviewAggregateModel[]? buyerOverview,
    BuyerPerformanceAggregateModel[]? buyerPerformance,
    SellerOverviewAggregateModel[]? sellerOverview,
    SellerPerformanceAggregateModel[]? sellerPerformance)
        {
            var buyerOverviewName = (int i) => $"BO_{i}";
            var buyerPerformanceName = (int i) => $"BP_{i}";
            var sellerOverviewName = (int i) => $"SO_{i}";
            var sellerPerformanceName = (int i) => $"SP_{i}";

            // 1. 呼叫 dbSDK 的 AdvancedSearchAsync 進行查詢與聚合掛載
            var searchResponse = await _elasticRepository.AdvancedSearchAsync(s => s
                .Size(0) // 統計不回傳文件內容
                .Aggregations(aggs =>
                {
                    // 將 aggs 字典依序傳遞給我們寫好的 Action 執行
                    if (buyerOverview != null)
                    {
                        for (int i = 0; i < buyerOverview.Length; i++)
                        {
                            BuyerOverviewAggregates(buyerOverviewName(i), buyerOverview[i])(aggs);
                        }
                    }

                    if (buyerPerformance != null)
                    {
                        for (int i = 0; i < buyerPerformance.Length; i++)
                        {
                            BuyerPerformanceAggregates(buyerPerformanceName(i), buyerPerformance[i])(aggs);
                        }
                    }

                    if (sellerOverview != null)
                    {
                        for (int i = 0; i < sellerOverview.Length; i++)
                        {
                            SellerOverviewAggregates(sellerOverviewName(i), sellerOverview[i])(aggs);
                        }
                    }

                    if (sellerPerformance != null)
                    {
                        for (int i = 0; i < sellerPerformance.Length; i++)
                        {
                            SellerPerformanceAggregates(sellerPerformanceName(i), sellerPerformance[i])(aggs);
                        }
                    }
                })
            );

            // 2. 確保有取得回應資料
            if (searchResponse == null || searchResponse.Aggregations == null)
            {
                throw new Exception("Elasticsearch return null or no aggregations.");
            }

            // 3. 解析結果區塊 (邏輯與原本相同，直接取用 searchResponse.Aggregations)
            List<BuyerOverviewAggregateResultModel>? buyerOverviewResult = null;
            List<SellerOverviewAggregateResultModel>? sellerOverviewResult = null;
            List<BuyerPerformanceAggregateResultModel>? buyerPerformanceResult = null;
            List<SellerPerformanceAggregateResultModel>? sellerPerformanceResult = null;

            if (buyerOverview != null)
            {
                buyerOverviewResult = new List<BuyerOverviewAggregateResultModel>();
                for (int i = 0; i < buyerOverview.Length; i++)
                {
                    var result = ParseBuyerOverview(buyerOverviewName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"BuyerOverviewAggregateResultModel not found for key: {buyerOverviewName(i)}");
                    buyerOverviewResult.Add(result);
                }
            }

            if (buyerPerformance != null)
            {
                buyerPerformanceResult = new List<BuyerPerformanceAggregateResultModel>();
                for (int i = 0; i < buyerPerformance.Length; i++)
                {
                    var result = ParseBuyerPerformance(buyerPerformanceName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"BuyerPerformanceAggregateResultModel not found for key: {buyerPerformanceName(i)}");
                    buyerPerformanceResult.Add(result);
                }
            }

            if (sellerOverview != null)
            {
                sellerOverviewResult = new List<SellerOverviewAggregateResultModel>();
                for (int i = 0; i < sellerOverview.Length; i++)
                {
                    var result = ParseSellerOverview(sellerOverviewName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"SellerOverviewAggregateResultModel not found for key: {sellerOverviewName(i)}");
                    sellerOverviewResult.Add(result);
                }
            }

            if (sellerPerformance != null)
            {
                sellerPerformanceResult = new List<SellerPerformanceAggregateResultModel>();
                for (int i = 0; i < sellerPerformance.Length; i++)
                {
                    var result = ParseSellerPerformance(sellerPerformanceName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"SellerPerformanceAggregateResultModel not found for key: {sellerPerformanceName(i)}");
                    sellerPerformanceResult.Add(result);
                }
            }

            // 4. 組合並回傳最終結果
            return new AggregateOrderInfoResultModel
            {
                BuyerOverview = buyerOverviewResult?.ToArray(),
                SellerOverview = sellerOverviewResult?.ToArray(),
                BuyerPerformance = buyerPerformanceResult?.ToArray(),
                SellerPerformance = sellerPerformanceResult?.ToArray(),
                Took = searchResponse.Took, // v8 依然支援 Took 屬性
            };
        }

        /// <summary>
        /// 取得訂單列表 (V8 非同步版)
        /// </summary>
        public async Task<SearchOrderInfoResultModel> SearchOrderInfoAsync(
            int from,
            int size,
            OrderInfoQueryModel query,
            OrderInfoSortModel sort)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // 呼叫 dbSDK，移除 indices 參數 (交由 Repository 內部處理)
            var searchResponse = await _elasticRepository.AdvancedSearchAsync(s => s
                .From(from)
                .Size(size)
                // 1. 直接掛載我們之前寫好的 Query Action
                .Query(OrderInfoQuery(query))

                // 2. 這裡假設你已經把 OrderInfoSort 轉寫為 Action<SortDescriptor<OrderDocument>> 
                .Sort(OrderInfoSort(sort))

                // 3. v8 的 Source 寫法 (加入 Filter 層)
                .Source(src => src.Filter(f => f.Includes(new[] { "coomNo" })))

                // 4. v8 的 TrackTotalHits 寫法 (需要明確使用 TrackHits 物件)
                .TrackTotalHits(new TrackHits(true))
            );

            stopwatch.Stop();

            // 5. 確保防呆，避免 NullReferenceException
            if (searchResponse == null)
            {
                return new SearchOrderInfoResultModel
                {
                    Documents = Array.Empty<OrderDocument>(),
                    Total = 0,
                    Took = 0
                };
            }

            return new SearchOrderInfoResultModel
            {
                Documents = searchResponse.Documents?.ToArray() ?? Array.Empty<OrderDocument>(),
                Total = searchResponse.Total, // 取得符合條件的總筆數
                Took = searchResponse.Took,
            };
        }

        #endregion


        /// <summary>
        /// 取得 App 賣家總覽與表現資訊 (V8 非同步版)
        /// </summary>
        public async Task<AppDashboardAggregateResultModel> AppAggregateOrderInfoAsync(
            AppSellerOverViewAggregateModel[]? appSellerOverview,
            AppSellerPerformanceAggregateModel[]? appSellerPerformance)
        {
            var appSellerOverviewName = (int i) => $"ASO_{i}";
            var appSellerPerformanceName = (int i) => $"ASP_{i}";

            // 1. 執行非同步搜尋與聚合掛載
            var searchResponse = await _elasticRepository.AdvancedSearchAsync(s => s
                .Size(0)
                .Aggregations(aggs =>
                {
                    if (appSellerOverview != null)
                    {
                        for (int i = 0; i < appSellerOverview.Length; i++)
                        {
                            AppSellerOverviewAggregates(appSellerOverviewName(i), appSellerOverview[i])(aggs);
                        }
                    }

                    if (appSellerPerformance != null)
                    {
                        for (int i = 0; i < appSellerPerformance.Length; i++)
                        {
                            AppSellerPerformanceAggregates(appSellerPerformanceName(i), appSellerPerformance[i])(aggs);
                        }
                    }
                })
            );

            if (searchResponse == null || searchResponse.Aggregations == null)
            {
                throw new Exception("Elasticsearch return null or no aggregations.");
            }

            // 2. 解析結果
            List<AppSellerOverViewAggregateResultModel>? appSellerOverviewResult = null;
            List<AppSellerPerformanceAggregateResultModel>? appSellerPerformanceResult = null;

            if (appSellerOverview != null)
            {
                appSellerOverviewResult = new List<AppSellerOverViewAggregateResultModel>();
                for (int i = 0; i < appSellerOverview.Length; i++)
                {
                    var result = ParseAppSellerOverview(appSellerOverviewName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"AppSellerOverViewAggregateResultModel not found for key: {appSellerOverviewName(i)}");
                    appSellerOverviewResult.Add(result);
                }
            }

            if (appSellerPerformance != null)
            {
                appSellerPerformanceResult = new List<AppSellerPerformanceAggregateResultModel>();
                for (int i = 0; i < appSellerPerformance.Length; i++)
                {
                    var result = ParseAppSellerPerformance(appSellerPerformanceName(i), searchResponse.Aggregations);
                    if (result == null) throw new Exception($"AppSellerPerformanceAggregateResultModel not found for key: {appSellerPerformanceName(i)}");
                    appSellerPerformanceResult.Add(result);
                }
            }

            return new AppDashboardAggregateResultModel
            {
                AppDashboard = appSellerOverviewResult?.ToArray(),
                AppPerformance = appSellerPerformanceResult?.ToArray(),
                Took = searchResponse.Took,
            };
        }


        private readonly static string SalesMetricsTotalAmount = "SalesMetricsTotalAmount";
        private readonly static string SalesMetricsTotalOrderCnt = "SalesMetricsTotalOrderCnt";
        private readonly static string SalesMetricsShipmentsCnt = "SalesMetricsShipmentsCnt";

        /// <summary>
        /// 統計 App 銷售指標 (V8 修正版)
        /// </summary>
        public static Action<FluentDictionaryOfStringAggregation<OrderDocument>> AppSalesMetrics(string name, AppSalesMetricsModel model)
        {
            var query = new OrderInfoQueryModel
            {
                CoomCuamCid = model.CuamCid,
                OrderDateStart = model.SearchStartDate,
                OrderDateEnd = model.SearchEndDate
            };

            // 這裡使用 .Add 是為了給最外層的 Filter 起名字
            return aggs => aggs.Add(name, agg => agg
                .Filter(OrderInfoQuery(query))
                .Aggregations(childAggs => childAggs
                    // 子聚合 1：訂單數
                    .Add($"{name}_{SalesMetricsTotalOrderCnt}", fa => fa
                        .Filter(PurchaseOrderQuery())
                        .Aggregations(sa => sa
                            // 銷售額 Sum
                            .Add($"{name}_{SalesMetricsTotalAmount}", sum => sum
                                .Sum(s => s.Field(o => o.CoomRcvTotalAmt))
                            )
                        )
                    )
                    // 子聚合 2：寄件數
                    .Add($"{name}_{SalesMetricsShipmentsCnt}", fa => fa
                        .Filter(SellerOverviewShippingQuery())
                    )
                )
            );
        }

        /// <summary>
        /// 取得 App 銷售指標詳細資訊 (V8 非同步版)
        /// </summary>
        public async Task<AppSalesMetricsResultModel[]> AppSalesMetricsInfoAsync(
            AppSalesMetricsModel[]? appSalesMetricsModel)
        {
            var appSalesMetricsName = (int i) => $"ASM_{i}";
            var appSalesMetricsPoPName = (int i) => $"ASMPOP_{i}";
            var appSalesMetricsTrendName = (int i) => $"ASMT_{i}";
            var appSalesMetricsProductRankingName = (int i) => $"ASMPR_{i}";

            // 1. 執行搜尋與聚合掛載 (dbSDK 對接點)
            var searchResponse = await _elasticRepository.AdvancedSearchAsync(s => s
                .Size(0)
                .Aggregations(aggs =>
                {
                    if (appSalesMetricsModel != null)
                    {
                        for (int i = 0; i < appSalesMetricsModel.Length; i++)
                        {
                            // 依序執行我們之前翻好的 Action
                            AppSalesMetrics(appSalesMetricsName(i), appSalesMetricsModel[i])(aggs);
                            AppSalesMetricsPoP(appSalesMetricsPoPName(i), appSalesMetricsModel[i])(aggs);
                            AppSalesMetricsTrend(appSalesMetricsTrendName(i), appSalesMetricsModel[i])(aggs);
                            AppSalesMetricsProductRanking(appSalesMetricsProductRankingName(i), appSalesMetricsModel[i])(aggs);
                        }
                    }
                })
            );

            if (searchResponse == null || searchResponse.Aggregations == null)
            {
                return Array.Empty<AppSalesMetricsResultModel>();
            }

            // 2. 解析結果
            var results = new List<AppSalesMetricsResultModel>();

            if (appSalesMetricsModel != null)
            {
                for (int i = 0; i < appSalesMetricsModel.Length; i++)
                {
                    var res1 = ParseSalesMetrics(appSalesMetricsName(i), searchResponse.Aggregations);
                    var res2 = ParseSalesMetricsPoP(appSalesMetricsPoPName(i), searchResponse.Aggregations);
                    var res3 = ParseSalesMetricsTrend(appSalesMetricsTrendName(i), appSalesMetricsModel[i].DateRangeType.GetValueOrDefault(), searchResponse.Aggregations);
                    var res4 = ParseSalesMetricsProductRanking(appSalesMetricsProductRankingName(i), searchResponse.Aggregations);

                    // 嚴謹檢查
                    if (res1 == null) throw new Exception($"AppSalesMetricsResultModel (Basic) not found for: {appSalesMetricsName(i)}");
                    if (res2 == null) throw new Exception($"AppSalesMetricsResultModel (PoP) not found for: {appSalesMetricsPoPName(i)}");
                    if (res3 == null) throw new Exception($"OrderTrendData not found for: {appSalesMetricsTrendName(i)}");
                    if (res4 == null) throw new Exception($"ProductSalesRanking not found for: {appSalesMetricsProductRankingName(i)}");

                    // 直接組裝，避免之後再跑一次 Select 迴圈
                    results.Add(new AppSalesMetricsResultModel
                    {
                        TotalAmount = res1.TotalAmount,
                        TotalOrderCnt = res1.TotalOrderCnt,
                        ShipmentsCnt = res1.ShipmentsCnt,
                        TotalAmountPoP = res2.TotalAmountPoP,
                        TotalOrderCntPoP = res2.TotalOrderCntPoP,
                        ShipmentsCntPoP = res2.ShipmentsCntPoP,
                        SalesTrendData = res3.SalesTrendData,
                        OrderTrendData = res3.OrderTrendData,
                        ProductSalesRanking = res4.ProductSalesRanking,
                        Took = searchResponse.Took // 直接把搜尋耗時塞進去
                    });
                }
            }

            return results.ToArray();
        }
    }
}