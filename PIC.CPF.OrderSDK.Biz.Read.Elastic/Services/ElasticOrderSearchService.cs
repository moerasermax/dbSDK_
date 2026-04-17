using Microsoft.Extensions.Logging;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension;

//using PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension; // 假設擴充方法也搬來了
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
//using PIC.CPF.OrderSDK.Biz.Read.Elastic.Validators; // 假設驗證器也搬來了

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    // 實作我們剛剛定義好、跟舊版長得一模一樣的 Interface
    public class ElasticOrderSearchService : IElasticOrderSearchService
    {
        private readonly ILogger<IElasticOrderSearchService>? _logger;
        private readonly ElasticOrderSearchBll _orderSearchBll;

        // 建議這裡順應現代架構，直接把 BLL 透過 DI 注入進來
        public ElasticOrderSearchService(ElasticOrderSearchBll orderSearchBll, ILogger<IElasticOrderSearchService>? logger)
        {
            _logger = logger;
            _orderSearchBll = orderSearchBll;
        }

        // Flow 2: 一般搜尋 (加上 async Task)
        public async Task<SearchOrderInfoResultModel> SearchOrderInfoAsync(SearchOrderInfoModel model)
        {
            // 1. 驗證 (完全照抄舊版)
            //SearchOrderInfoModelValidator.ValidatePageIndex(model.PageIndex);
            //SearchOrderInfoModelValidator.ValidatePageSize(model.PageSize);
            // ... (其他 Validate 省略，直接貼上舊版的即可)

            // 2. DTO 轉換 (大 Model 轉內部 Query Model)
            var pageSize = model.PageSize!.Value;
            var pageIndex = model.PageIndex!.Value;
            var query = model.ConverToOrderInfoQueryModel();
            var sort = model.ConvertToOrderInfoSortModel();

            // 3. 呼叫底層 BLL (唯一不同：加上 await 和 Async)
            var result = await _orderSearchBll.SearchOrderInfoAsync(pageIndex, pageSize, query, sort);

            // 4. 結果轉換回傳
            return result.ConvertToSearchOrderInfoResultModel();
        }

        // Flow 1: Web 統計
        public async Task<AggregateOrderInfoResultModel> AggregateOrderInfoAsync(AggregateOrderInfoModel model)
        {
            var buyerOverview = model.ConvertToBuyerOverviewAggregateModel();
            var buyerPerformance = model.ConvertToBuyerPerformanceAggregateModel();
            var sellerOverview = model.ConvertToSellerOverviewAggregateModel();
            var sellerPerformance = model.ConvertToSellerPerformanceAggregateModel();

            // 加上 await
            var result = await _orderSearchBll.AggregateOrderInfoAsync(buyerOverview, buyerPerformance, sellerOverview, sellerPerformance);

            return result.ConvertToAggregateOrderInfoResultModel();
        }

        #region ESAPP
        // Flow 3: App 統計
        public async Task<AppDashboardAggregateResultModel> AppAggregateOrderInfoAsync(AppAggregateOrderInfoModel model)
        {
            var appSellerOverview = model.ConvertToAppSellerOverviewAggregateModel();
            var appSellerPerformance = model.ConvertToAppSellerPerformanceAggregateModel();

            var result = await _orderSearchBll.AppAggregateOrderInfoAsync(appSellerOverview, appSellerPerformance);

            return result.ConvertToAppDashboardAggregateResultModel();
        }

        public async Task<AppSalesMetricsResultModel[]> AppSalesMetricsInfoAsync(AppSalesMetricsModel[] model)
        {
            var appSalesMetricsModel = model.ConvertToAppSalesMetricsModel();

            var result = await _orderSearchBll.AppSalesMetricsInfoAsync(appSalesMetricsModel);

            return result.ConvertToAppSalesMetricsResultModel();
        }
        #endregion
    }
}