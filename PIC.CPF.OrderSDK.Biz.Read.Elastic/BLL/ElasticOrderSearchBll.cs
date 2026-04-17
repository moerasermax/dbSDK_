using Microsoft.Extensions.Logging;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
// 這裡引用的是內部專用的 Model
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL
{
    public class ElasticOrderSearchBll
    {
        private readonly OrderSearchDal _dal;
        private readonly ILogger<ElasticOrderSearchBll>? _logger;

        public ElasticOrderSearchBll(OrderSearchDal dal, ILogger<ElasticOrderSearchBll>? logger)
        {
            _dal = dal;
            _logger = logger;
        }

        // ==========================================
        // Flow 1: Web 統計
        // ==========================================
        public async Task<AggregateOrderInfoResultModel> AggregateOrderInfoAsync(
            BuyerOverviewAggregateModel[]? buyerOverview,
            BuyerPerformanceAggregateModel[]? buyerPerformance,
            SellerOverviewAggregateModel[]? sellerOverview,
            SellerPerformanceAggregateModel[]? sellerPerformance)
        {
            // BLL 作為橋樑，直接呼叫 DAL 的聚合方法
            return await _dal.AggregateOrderInfoAsync(
                buyerOverview,
                buyerPerformance,
                sellerOverview,
                sellerPerformance);
        }

        // ==========================================
        // Flow 2: 一般搜尋
        // ==========================================
        public async Task<SearchOrderInfoResultModel> SearchOrderInfoAsync(
            int from,
            int size,
            OrderInfoQueryModel query,
            OrderInfoSortModel sort)
        {
            // 在這裡如果有特定的 BLL 邏輯可以加，例如：
            // _logger?.LogInformation($"開始執行 SearchOrderInfoAsync, Query: {query.CoomNo}");

            // 直接呼叫我們寫好的 V9 DAL
            return await _dal.SearchOrderInfoAsync(from, size, query, sort);
        }

        #region ESAPP
        // ==========================================
        // Flow 3: App 統計相關
        // ==========================================
        public async Task<AppDashboardAggregateResultModel> AppAggregateOrderInfoAsync(
            AppSellerOverViewAggregateModel[]? appSellerOverview,
            AppSellerPerformanceAggregateModel[]? appSellerPerformance)
        {
            return await _dal.AppAggregateOrderInfoAsync(
                appSellerOverview,
                appSellerPerformance);
        }

        public async Task<AppSalesMetricsResultModel[]> AppSalesMetricsInfoAsync(
            AppSalesMetricsModel[]? appSalesMetricsModel)
        {
            return await _dal.AppSalesMetricsInfoAsync(appSalesMetricsModel);
        }
        #endregion
    }
}