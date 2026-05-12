using Microsoft.Extensions.Logging;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Extension;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;
using System.Text.Json;
using PublicModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
using InternalModels = PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL
{
    /// <summary>
    /// Order Search BLL — 7 個 Search method、各自接 6 個對齊客戶 Controller 命名的 Public Input Model。
    /// Clean Architecture 分層:
    ///   Public Input (客戶端傳入) → BLL (本層) → Internal Aggregate Model → DAL → ES/Mongo
    ///   業務規則 (PoP defaulting、fallback 區間、Dual Engine flow) 收斂於本層、DAL 保持 dumb。
    /// 依賴注入:
    ///   IClock 用於 Search 4 fallback 區間計算 (today / mondayDate / endDate)、利於測試注入固定時間。
    /// </summary>
    public class ElasticOrderSearchBll
    {
        private readonly OrderSearchDal _dal;
        private readonly MongoSearchDal _mongoSearchDal;
        private readonly ILogger<ElasticOrderSearchBll>? _logger;
        private readonly IClock _clock;

        public ElasticOrderSearchBll(
            OrderSearchDal dal,
            MongoSearchDal mongoSearchDal,
            ILogger<ElasticOrderSearchBll>? logger,
            IClock? clock = null)
        {
            _dal = dal;
            _mongoSearchDal = mongoSearchDal;
            _logger = logger;
            _clock = clock ?? new SystemClock();
        }

        // ==========================================
        // Search 1：首頁待辦事項總覽（買家 + 賣家雙視角）
        // 客戶 Controller: GetHomeToDoOverview(GetHomeToDoOverviewModel)
        // ==========================================
        public async Task<IResult<PublicModels.AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(
            PublicModels.GetHomeToDoOverviewModel model)
        {
            try
            {
                var start = model.SearchStartDate ?? DateTime.MinValue;
                var end = model.SearchEndDate ?? DateTime.MaxValue;
                var cid = model.CuamCid ?? 0;

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
        // 客戶 Controller: SearchOrderInfoBySellerId(SearchOrderInfoBySellerIdModel)
        // ==========================================
        public async Task<IResult<PublicModels.SearchOrderInfoResultModel>> SearchBySellerAsync(
            PublicModels.SearchOrderInfoBySellerIdModel model)
        {
            try
            {
                var pageSize = model.PageSize ?? 10;
                var pageIndex = model.PageIndex ?? 0;

                var query = new InternalModels.OrderInfoQueryModel
                {
                    CoomCuamCid = model.CuamCid,
                    OrderDateStart = model.OrderDateStart,
                    OrderDateEnd = model.OrderDateEnd,
                    CoomNo = model.CoomNo,
                    CoomName = model.CoomName,
                    EsmmShipNo = model.EsmmShipNo,
                    CoodName = model.CoodName,
                    CoocNo = model.CoocNo,
                    OrderState = model.OrderState,
                    DeliverMethodSearchKind = model.DeliverMethodSearchKind,
                    OrdChannelKindSearchKind = model.OrdChannelKindSearchKind,
                    TempTypeSearchKind = model.TempTypeSearchKind,
                    IsQaList = model.IsQaList,
                };
                var sort = new InternalModels.OrderInfoSortModel { OrderSorts = model.OrderSorts };

                // mirror 客戶原 SDK Dual Engine: ① OPS 取 Total + KeyList ② Mongo 取明細 (客戶端是 DDB/DynamoDB,dbSDK 用 MongoDB) ③ 轉 Public Model
                var OPSResult = await _dal.SearchOrderInfoAsync(pageIndex * pageSize, pageSize, query, sort);
                if (OPSResult.Total <= 0)
                {
                    return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", new PublicModels.SearchOrderInfoResultModel { Total = 0, OrderInfos = null });
                }

                var KeyList = OPSResult.Documents
                    .Select(je => je.ValueKind == JsonValueKind.Object && je.TryGetProperty("coom_no", out var v) && v.ValueKind == JsonValueKind.String
                        ? v.GetString()
                        : null)
                    .ToList();

                var MongoData = await _mongoSearchDal.SearchByMongoAsync(KeyList);

                return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", new PublicModels.SearchOrderInfoResultModel
                {
                    Total = OPSResult.Total,
                    OrderInfos = MongoData.ConvertToOrderData(),
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchBySellerAsync));
                return Result<PublicModels.SearchOrderInfoResultModel>.SetErrorResult(nameof(SearchBySellerAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 3：訂單搜尋（買家視角）
        // 客戶 Controller: SearchOrderInfoByBuyerId(SearchOrderInfoByBuyerIdModel)
        // ==========================================
        public async Task<IResult<PublicModels.SearchOrderInfoResultModel>> SearchByBuyerAsync(
            PublicModels.SearchOrderInfoByBuyerIdModel model)
        {
            try
            {
                var pageSize = model.PageSize ?? 10;
                var pageIndex = model.PageIndex ?? 0;

                var query = new InternalModels.OrderInfoQueryModel
                {
                    CoocMemSid = model.MemSid,
                    OrderDateStart = model.OrderDateStart,
                    OrderDateEnd = model.OrderDateEnd,
                    CoomNo = model.CoomNo,
                    CoocNo = model.CoocNo,
                    OrderState = model.OrderState,
                    IsQaList = model.IsQaList,
                    BindMembersArray = model.BindMembersArray,
                };
                var sort = new InternalModels.OrderInfoSortModel { OrderSorts = model.OrderSorts };

                // mirror 客戶原 SDK Dual Engine
                var OPSResult = await _dal.SearchOrderInfoAsync(pageIndex * pageSize, pageSize, query, sort);
                if (OPSResult.Total <= 0)
                {
                    return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", new PublicModels.SearchOrderInfoResultModel { Total = 0, OrderInfos = null });
                }

                var KeyList = OPSResult.Documents
                    .Select(je => je.ValueKind == JsonValueKind.Object && je.TryGetProperty("coom_no", out var v) && v.ValueKind == JsonValueKind.String
                        ? v.GetString()
                        : null)
                    .ToList();

                var MongoData = await _mongoSearchDal.SearchByMongoAsync(KeyList);

                return Result<PublicModels.SearchOrderInfoResultModel>.SetResult("成功", new PublicModels.SearchOrderInfoResultModel
                {
                    Total = OPSResult.Total,
                    OrderInfos = MongoData.ConvertToOrderData(),
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(SearchByBuyerAsync));
                return Result<PublicModels.SearchOrderInfoResultModel>.SetErrorResult(nameof(SearchByBuyerAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 4：App 儀表板總覽
        // 客戶 Controller: GetAppDashboardOverview(GetAppDashboardOverviewModel)
        // S41-K:對齊客戶原 SDK BLL 邏輯、Overview 與 Performance 用「不同」預設區間:
        //   - AppSellerOverview:    today.AddDays(-90) ~ today (過去 90 天)
        //   - AppSellerPerformance: 本週一 mondayDate ~ today+1 endDate (本週累計)
        // Caller 傳入 SearchStartDate/EndDate 時兩段共用 (供測試或自訂區間覆寫)
        // ==========================================
        public async Task<IResult<PublicModels.AppDashboardAggregateResultModel>> GetAppDashboardAsync(
            PublicModels.GetAppDashboardOverviewModel model)
        {
            try
            {
                var cid = model.CuamCid ?? 0;

                // 客戶原邏輯:today = DateTime.Now (本地)、轉 UTC 後傳入 ES
                // 注入 IClock 抽象、生產用 SystemClock、測試可注入 FixedClock
                var today = _clock.Now;
                var daysFromMonday = ((int)today.DayOfWeek == 0 ? 7 : (int)today.DayOfWeek) - 1;
                var mondayDate = today.AddDays(-daysFromMonday).Date;
                var endDate = today.Date.AddDays(1); // +1 含今天

                // Overview 區間:過去 90 天 (對齊客戶原 SDK BLL 邏輯)
                var overviewStart = today.AddDays(-90).ToUniversalTime();
                var overviewEnd = today.ToUniversalTime();

                // Performance 區間:本週一 ~ today+1
                var perfStart = mondayDate.ToUniversalTime();
                var perfEnd = endDate.ToUniversalTime();

                var internalResult = await _dal.AppAggregateOrderInfoAsync(
                    appSellerOverview: [new InternalModels.AppSellerOverViewAggregateModel { CoomCuamCid = cid, OrderDateStart = overviewStart, OrderDateEnd = overviewEnd }],
                    appSellerPerformance: [new InternalModels.AppSellerPerformanceAggregateModel { CoomCuamCid = cid, OrderDateStart = perfStart, OrderDateEnd = perfEnd }]
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
        // 客戶 Controller: GetAppSalesMetrics(GetAppSalesMetricsModel) — dateRangeType=Today
        // ==========================================
        public async Task<IResult<PublicModels.AppSalesMetricsResultModel>> GetAppSalesTodayAsync(
            PublicModels.GetAppSalesMetricsModel model)
        {
            try
            {
                var (startPoP, endPoP, validationError) = ResolvePoPRange(model);
                if (validationError != null)
                    return Result<PublicModels.AppSalesMetricsResultModel>.SetErrorResult(nameof(GetAppSalesTodayAsync), validationError);

                var internalModel = new InternalModels.AppSalesMetricsModel
                {
                    CuamCid = model.CuamCid ?? 0,
                    SearchStartDate = model.SearchStartDate,
                    SearchEndDate = model.SearchEndDate,
                    StartDatePoP = startPoP,
                    EndDatePoP = endPoP,
                    DateRangeType = model.DateRangeType,
                };
                var internalResults = await _dal.AppSalesMetricsInfoAsync([internalModel]);
                var resultArray = internalResults.ConvertToAppSalesMetricsResultModel();
                var result = resultArray.Length > 0 ? resultArray[0] : new PublicModels.AppSalesMetricsResultModel();

                // 套用趨勢資料補零 (S41-F: Daily 路徑改以 req.SearchStart/End 為區間、不再用 DateTime.Now)
                result = result.ApplyZeroPadding(model.DateRangeType, model.SearchStartDate, model.SearchEndDate);

                return Result<PublicModels.AppSalesMetricsResultModel>.SetResult("成功", result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesTodayAsync));
                return Result<PublicModels.AppSalesMetricsResultModel>.SetErrorResult(nameof(GetAppSalesTodayAsync), ex.Message);
            }
        }

        // ==========================================
        // Search 6：App 銷售指標（本週）
        // 客戶 Controller: GetAppSalesMetrics(GetAppSalesMetricsModel) — dateRangeType=SetWeek 等
        // ==========================================
        public async Task<IResult<PublicModels.AppSalesMetricsResultModel>> GetAppSalesWeekAsync(
            PublicModels.GetAppSalesMetricsModel model)
        {
            try
            {
                var (startPoP, endPoP, validationError) = ResolvePoPRange(model);
                if (validationError != null)
                    return Result<PublicModels.AppSalesMetricsResultModel>.SetErrorResult(nameof(GetAppSalesWeekAsync), validationError);

                var internalModel = new InternalModels.AppSalesMetricsModel
                {
                    CuamCid = model.CuamCid ?? 0,
                    SearchStartDate = model.SearchStartDate,
                    SearchEndDate = model.SearchEndDate,
                    StartDatePoP = startPoP,
                    EndDatePoP = endPoP,
                    DateRangeType = model.DateRangeType,
                };
                var internalResults = await _dal.AppSalesMetricsInfoAsync([internalModel]);
                var resultArray = internalResults.ConvertToAppSalesMetricsResultModel();
                var result = resultArray.Length > 0 ? resultArray[0] : new PublicModels.AppSalesMetricsResultModel();

                // 套用趨勢資料補零 (S41-F: Daily 路徑改以 req.SearchStart/End 為區間)
                result = result.ApplyZeroPadding(model.DateRangeType, model.SearchStartDate, model.SearchEndDate);

                return Result<PublicModels.AppSalesMetricsResultModel>.SetResult("成功", result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetAppSalesWeekAsync));
                return Result<PublicModels.AppSalesMetricsResultModel>.SetErrorResult(nameof(GetAppSalesWeekAsync), ex.Message);
            }
        }

        // Application layer 業務規則:Period-over-Period (PoP) 區間預設策略 (S41-H)
        // 規則:(a) 若 caller 未指定 PoP (both null):預設等於 main range
        //       (b) 半套指定 (只傳一邊):invalid input、回 validationError
        private static (DateTime? Start, DateTime? End, string? Error) ResolvePoPRange(
            PublicModels.GetAppSalesMetricsModel model)
        {
            if (model.DateStartPoP.HasValue != model.DateEndPoP.HasValue)
                return (null, null, "DateStartPoP 與 DateEndPoP 必須同時提供、或同時為 null");

            return (model.DateStartPoP ?? model.SearchStartDate, model.DateEndPoP ?? model.SearchEndDate, null);
        }

        // ==========================================
        // Search 7：取得賣家 cgdm 資料 (S41-E)
        // 客戶 Controller: GetUserCgdmData(SearchUserCGoodsMModel)
        // mirror 客戶原 SDK: _searchDal.GetUserByCuamCidFromDDB(CuamCid) — 客戶 DDB=DynamoDB,dbSDK 用 MongoDB Users collection
        // 棄用原 ES 聚合 (Nested→Terms→ReverseNested→Max _ord_modify_date) — 測試資料 ES 無 _ord_modify_date 來源
        // ==========================================
        public async Task<IResult<PublicModels.UserCgdmDataResultModel>> GetUserCgdmDataAsync(
            PublicModels.SearchUserCGoodsMModel model)
        {
            try
            {
                var cid = model.CuamCid ?? 0;
                var mongoUser = await _mongoSearchDal.GetUserByCuamCidFromMongoAsync(cid);
                return Result<PublicModels.UserCgdmDataResultModel>.SetResult("成功", mongoUser.ConvertToUserCgdmDataResultModel(cid));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, nameof(GetUserCgdmDataAsync));
                return Result<PublicModels.UserCgdmDataResultModel>.SetErrorResult(nameof(GetUserCgdmDataAsync), ex.Message);
            }
        }
    }
}
