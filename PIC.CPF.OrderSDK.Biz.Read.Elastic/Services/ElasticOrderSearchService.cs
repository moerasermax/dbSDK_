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

        public Task<IResult<AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(OrderSearchRequest req)
            => _orderSearchBll.GetHomeToDoOverViewAsync(req);

        public Task<IResult<SearchOrderInfoResultModel>> SearchBySellerAsync(OrderSearchRequest req)
            => _orderSearchBll.SearchBySellerAsync(req);

        public Task<IResult<SearchOrderInfoResultModel>> SearchByBuyerAsync(OrderSearchRequest req)
            => _orderSearchBll.SearchByBuyerAsync(req);

        public Task<IResult<AppDashboardAggregateResultModel>> GetAppDashboardAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppDashboardAsync(req);

        public Task<IResult<AppSalesMetricsResultModel[]>> GetAppSalesTodayAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppSalesTodayAsync(req);

        public Task<IResult<AppSalesMetricsResultModel[]>> GetAppSalesWeekAsync(OrderSearchRequest req)
            => _orderSearchBll.GetAppSalesWeekAsync(req);

        public Task<IResult<UserCgdmDataResultModel>> GetUserCgdmDataAsync(OrderSearchRequest req)
            => _orderSearchBll.GetUserCgdmDataAsync(req);
    }
}
