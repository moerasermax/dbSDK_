using NO3._dbSDK_Imporve.Core.Interface;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    /// <summary>
    /// 訂單查詢 SDK 對外介面 — 7 個 Search、6 個 Public Input Model。
    /// Model 命名對齊客戶 Controller 端、客戶 wrapper (xxxMongo / xxxElastic / xxxBoth) 可直接 pass-through。
    /// </summary>
    public interface IElasticOrderSearchService
    {
        // Search 1: 首頁待辦事項總覽
        Task<IResult<AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(GetHomeToDoOverviewModel model);

        // Search 2: 賣家視角訂單搜尋
        Task<IResult<SearchOrderInfoResultModel>> SearchBySellerAsync(SearchOrderInfoBySellerIdModel model);

        // Search 3: 買家視角訂單搜尋
        Task<IResult<SearchOrderInfoResultModel>> SearchByBuyerAsync(SearchOrderInfoByBuyerIdModel model);

        // Search 4: App 儀表板總覽
        Task<IResult<AppDashboardAggregateResultModel>> GetAppDashboardAsync(GetAppDashboardOverviewModel model);

        // Search 5: App 銷售指標(本日) — dateRangeType=Today
        Task<IResult<AppSalesMetricsResultModel>> GetAppSalesTodayAsync(GetAppSalesMetricsModel model);

        // Search 6: App 銷售指標(本週) — dateRangeType=SetWeek 等
        Task<IResult<AppSalesMetricsResultModel>> GetAppSalesWeekAsync(GetAppSalesMetricsModel model);

        // Search 7: 取得賣家 cgdm 資料
        Task<IResult<UserCgdmDataResultModel>> GetUserCgdmDataAsync(SearchUserCGoodsMModel model);
    }
}
