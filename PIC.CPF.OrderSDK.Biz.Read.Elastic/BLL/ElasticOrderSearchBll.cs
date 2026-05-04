using Microsoft.Extensions.Logging;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension;
using PublicModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using InternalModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

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
        // Search 1：首頁待辦事項總覽（買家 + 賣家雙視角）
        // ==========================================
        public async Task<IResult<PublicModels.AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var start = req.SearchStartDate ?? DateTime.MinValue;
                var end = req.SearchEndDate ?? DateTime.MaxValue;
                var cid = req.CuamCid ?? 0;

                var internalResult = await _dal.AggregateOrderInfoAsync(
                    buyerOverview: [new InternalModels.BuyerOverviewAggregateModel { CoocMemSid = cid, OrderDateStart = start, OrderDateEnd = end }],
                    buyerPerformance: [new InternalModels.BuyerPerformanceAggregateModel { CoocMemSid = cid, OrderDateStart = start, OrderDateEnd = end }],
                    sellerOverview: [new InternalModels.SellerOverviewAggregateModel { CoomCuamCid = cid, OrderDateStart = start, OrderDateEnd = end }],
                    sellerPerformance: [new InternalModels.SellerPerformanceAggregateModel { CoomCuamCid = cid, OrderDateStart = start, OrderDateEnd = end }]
                );
                return Result<PublicModels.AggregateOrderInfoResultModel>.SetResult("成功", internalResult.ConvertToAggregateOrderInfoResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetHomeToDoOverViewAsync));
                return Result<PublicModels.AggregateOrderInfoResultModel>.SetErrorResult(nameof(GetHomeToDoOverViewAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 2：訂單搜尋（賣家視角）
        // ==========================================
        public async Task<IResult<PublicModels.SearchOrderInfoResultModel>> SearchBySellerAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var pageSize = req.PageInfo?.PageSize ?? 10;
                var pageIndex = req.PageInfo?.PageIndex ?? 0;

                var query = new InternalModels.OrderInfoQueryModel
                {
                    CoomCuamCid = req.CuamCid,
                    OrderDateStart = req.SearchStartDate,
                    OrderDateEnd = req.SearchEndDate,
                    CoomNo = req.CoomNo,
                    CoomName = req.CoomName,
                    EsmmShipNo = req.EsmmShipNo,
                    CoodName = req.CoodName,
                    CoocNo = req.CoocNo,
                    OrderState = req.OrderState,
                    DeliverMethodSearchKind = req.DeliverMethodSearchKind,
                    OrdChannelKindSearchKind = req.OrdChannelKindSearchKind,
                    TempTypeSearchKind = req.TempTypeSearchKind,
                    IsQaList = req.IsQaList,
                    BindMembersArray = req.BindMembersArray,
                };
                var sort = new InternalModels.OrderInfoSortModel { OrderSorts = req.Sorts };

                var internalResult = await _dal.SearchOrderInfoAsync(pageIndex * pageSize, pageSize, query, sort);
                return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", internalResult.ConvertToSearchOrderInfoResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchBySellerAsync));
                return Result<PublicModels.SearchOrderInfoResultModel>.SetErrorResult(nameof(SearchBySellerAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 3：訂單搜尋（買家視角）
        // ==========================================
        public async Task<IResult<PublicModels.SearchOrderInfoResultModel>> SearchByBuyerAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var pageSize = req.PageInfo?.PageSize ?? 10;
                var pageIndex = req.PageInfo?.PageIndex ?? 0;

                var query = new InternalModels.OrderInfoQueryModel
                {
                    CoocMemSid = req.MemSid,
                    OrderDateStart = req.SearchStartDate,
                    OrderDateEnd = req.SearchEndDate,
                    CoomNo = req.CoomNo,
                    CoomName = req.CoomName,
                    EsmmShipNo = req.EsmmShipNo,
                    CoodName = req.CoodName,
                    CoocNo = req.CoocNo,
                    OrderState = req.OrderState,
                    DeliverMethodSearchKind = req.DeliverMethodSearchKind,
                    OrdChannelKindSearchKind = req.OrdChannelKindSearchKind,
                    TempTypeSearchKind = req.TempTypeSearchKind,
                    IsQaList = req.IsQaList,
                    BindMembersArray = req.BindMembersArray,
                };
                var sort = new InternalModels.OrderInfoSortModel { OrderSorts = req.Sorts };

                var internalResult = await _dal.SearchOrderInfoAsync(pageIndex * pageSize, pageSize, query, sort);
                return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", internalResult.ConvertToSearchOrderInfoResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchByBuyerAsync));
                return Result<PublicModels.SearchOrderInfoResultModel>.SetErrorResult(nameof(SearchByBuyerAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 4：App 儀表板總覽（固定 90 天區間）
        // ==========================================
        public async Task<IResult<PublicModels.AppDashboardAggregateResultModel>> GetAppDashboardAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var cid = req.CuamCid ?? 0;
                var end = DateTime.UtcNow;
                var start = end.AddDays(-90);

                var internalResult = await _dal.AppAggregateOrderInfoAsync(
                    appSellerOverview: [new InternalModels.AppSellerOverViewAggregateModel { CoomCuamCid = cid, OrderDateStart = start, OrderDateEnd = end }],
                    appSellerPerformance: [new InternalModels.AppSellerPerformanceAggregateModel { CoomCuamCid = cid, OrderDateStart = start, OrderDateEnd = end }]
                );
                return Result<PublicModels.AppDashboardAggregateResultModel>.SetResult("成功", internalResult.ConvertToAppDashboardAggregateResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppDashboardAsync));
                return Result<PublicModels.AppDashboardAggregateResultModel>.SetErrorResult(nameof(GetAppDashboardAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 5：App 銷售指標（本日）
        // ==========================================
        public async Task<IResult<PublicModels.AppSalesMetricsResultModel[]>> GetAppSalesTodayAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var model = new InternalModels.AppSalesMetricsModel
                {
                    CuamCid = req.CuamCid ?? 0,
                    SearchStartDate = req.SearchStartDate,
                    SearchEndDate = req.SearchEndDate,
                    StartDatePoP = req.DateStartPoP,
                    EndDatePoP = req.DateEndPoP,
                    DateRangeType = req.DateRangeType,
                };
                var internalResults = await _dal.AppSalesMetricsInfoAsync([model]);
                return Result<PublicModels.AppSalesMetricsResultModel[]>.SetResult("成功", internalResults.ConvertToAppSalesMetricsResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesTodayAsync));
                return Result<PublicModels.AppSalesMetricsResultModel[]>.SetErrorResult(nameof(GetAppSalesTodayAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 6：App 銷售指標（本週）
        // ==========================================
        public async Task<IResult<PublicModels.AppSalesMetricsResultModel[]>> GetAppSalesWeekAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var model = new InternalModels.AppSalesMetricsModel
                {
                    CuamCid = req.CuamCid ?? 0,
                    SearchStartDate = req.SearchStartDate,
                    SearchEndDate = req.SearchEndDate,
                    StartDatePoP = req.DateStartPoP,
                    EndDatePoP = req.DateEndPoP,
                    DateRangeType = req.DateRangeType,
                };
                var internalResults = await _dal.AppSalesMetricsInfoAsync([model]);
                return Result<PublicModels.AppSalesMetricsResultModel[]>.SetResult("成功", internalResults.ConvertToAppSalesMetricsResultModel());
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesWeekAsync));
                return Result<PublicModels.AppSalesMetricsResultModel[]>.SetErrorResult(nameof(GetAppSalesWeekAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 7：取得賣家 cgdm 資料
        // ==========================================
        public async Task<IResult<PublicModels.UserCgdmDataResultModel>> GetUserCgdmDataAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var cid = req.CuamCid ?? 0;
                var internalResults = await _dal.GetUserCgdmDataAsync(cid);
                return Result<PublicModels.UserCgdmDataResultModel>.SetResult("成功", internalResults.ConvertToUserCgdmDataResultModel(cid));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetUserCgdmDataAsync));
                return Result<PublicModels.UserCgdmDataResultModel>.SetErrorResult(nameof(GetUserCgdmDataAsync), ex.Message);
            }
        }
    }
}
