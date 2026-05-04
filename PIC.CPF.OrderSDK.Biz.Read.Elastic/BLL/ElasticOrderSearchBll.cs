using Microsoft.Extensions.Logging;
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
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.AggregateOrderInfoResultModel> { Data = internalResult.ConvertToAggregateOrderInfoResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetHomeToDoOverViewAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.AggregateOrderInfoResultModel> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 2：訂單搜尋（賣家視角）
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel>> SearchBySellerAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel> { Data = internalResult.ConvertToSearchOrderInfoResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchBySellerAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 3:訂單搜尋（買家視角）
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel>> SearchByBuyerAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel> { Data = internalResult.ConvertToSearchOrderInfoResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchByBuyerAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.SearchOrderInfoResultModel> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 4：App 儀表板總覽（固定 90 天區間）
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.AppDashboardAggregateResultModel>> GetAppDashboardAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.AppDashboardAggregateResultModel> { Data = internalResult.ConvertToAppDashboardAggregateResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppDashboardAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.AppDashboardAggregateResultModel> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 5：App 銷售指標（本日）
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]>> GetAppSalesTodayAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]> { Data = internalResults.ConvertToAppSalesMetricsResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesTodayAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 6：App 銷售指標（本週）
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]>> GetAppSalesWeekAsync(
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
                return new PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]> { Data = internalResults.ConvertToAppSalesMetricsResultModel() };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesWeekAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.AppSalesMetricsResultModel[]> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }

        // ==========================================
        // Search 7：取得賣家 cgdm 資料
        // ==========================================
        public async Task<PublicModels.ApiResponseWrapper<PublicModels.UserCgdmDataResultModel>> GetUserCgdmDataAsync(
            PublicModels.OrderSearchRequest req)
        {
            try
            {
                var cid = req.CuamCid ?? 0;
                var internalResults = await _dal.GetUserCgdmDataAsync(cid);
                return new PublicModels.ApiResponseWrapper<PublicModels.UserCgdmDataResultModel> { Data = internalResults.ConvertToUserCgdmDataResultModel(cid) };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetUserCgdmDataAsync));
                return new PublicModels.ApiResponseWrapper<PublicModels.UserCgdmDataResultModel> { Code = "99", Message = "失敗", ErrorMsg = ex.Message };
            }
        }
    }
}
