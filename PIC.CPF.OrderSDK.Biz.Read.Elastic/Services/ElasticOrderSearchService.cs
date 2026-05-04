using Microsoft.Extensions.Logging;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    public class ElasticOrderSearchService : IElasticOrderSearchService
    {
        private readonly ILogger<IElasticOrderSearchService>? _logger;
        private readonly ElasticOrderSearchBll _orderSearchBll;

        public ElasticOrderSearchService(ElasticOrderSearchBll orderSearchBll, ILogger<IElasticOrderSearchService>? logger)
        {
            _logger = logger;
            _orderSearchBll = orderSearchBll;
        }

        public Task<ApiResponseWrapper<AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(OrderSearchRequest req)
            => _orderSearchBll.GetHomeToDoOverViewAsync(req);

        public Task<ApiResponseWrapper<SearchOrderInfoResultModel>> SearchBySellerAsync(OrderSearchRequest req)
            => _orderSearchBll.SearchBySellerAsync(req);

        public Task<ApiResponseWrapper<SearchOrderInfoResultModel>> SearchByBuyerAsync(OrderSearchRequest req)
            => _orderSearchBll.SearchByBuyerAsync(req);

        public Task<ApiResponseWrapper<AppDashboardAggregateResultModel>> GetAppDashboardAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppDashboardAsync(req);

        public Task<ApiResponseWrapper<AppSalesMetricsResultModel[]>> GetAppSalesTodayAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppSalesTodayAsync(req);

        public Task<ApiResponseWrapper<AppSalesMetricsResultModel[]>> GetAppSalesWeekAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppSalesWeekAsync(req);

        public Task<ApiResponseWrapper<UserCgdmDataResultModel>> GetUserCgdmDataAsync(OrderSearchRequest req)
            => _orderSearchBll.GetUserCgdmDataAsync(req);
    }
}
