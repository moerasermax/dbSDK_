using Microsoft.Extensions.Logging;
using NO3._dbSDK_Imporve.Core.Interface;
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

        public Task<IResult<AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(GetHomeToDoOverviewModel model)
            => _orderSearchBll.GetHomeToDoOverViewAsync(model);

        public Task<IResult<SearchOrderInfoResultModel>> SearchBySellerAsync(SearchOrderInfoBySellerIdModel model)
            => _orderSearchBll.SearchBySellerAsync(model);

        public Task<IResult<SearchOrderInfoResultModel>> SearchByBuyerAsync(SearchOrderInfoByBuyerIdModel model)
            => _orderSearchBll.SearchByBuyerAsync(model);

        public Task<IResult<AppDashboardAggregateResultModel>> GetAppDashboardAsync(GetAppDashboardOverviewModel model)
            => _orderSearchBll.GetAppDashboardAsync(model);

        public Task<IResult<AppSalesMetricsResultModel>> GetAppSalesTodayAsync(GetAppSalesMetricsModel model)
            => _orderSearchBll.GetAppSalesTodayAsync(model);

        public Task<IResult<AppSalesMetricsResultModel>> GetAppSalesWeekAsync(GetAppSalesMetricsModel model)
            => _orderSearchBll.GetAppSalesWeekAsync(model);

        public Task<IResult<UserCgdmDataResultModel>> GetUserCgdmDataAsync(SearchUserCGoodsMModel model)
            => _orderSearchBll.GetUserCgdmDataAsync(model);
    }
}
