using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    public interface IElasticOrderSearchService
    {
        // Search 1: 首頁待辦事項總覽
        Task<ApiResponseWrapper<AggregateOrderInfoResultModel>> GetHomeToDoOverViewAsync(OrderSearchRequest req);

        // Search 2: 賣家視角訂單搜尋
        Task<ApiResponseWrapper<SearchOrderInfoResultModel>> SearchBySellerAsync(OrderSearchRequest req);

        // Search 3: 買家視角訂單搜尋
        Task<ApiResponseWrapper<SearchOrderInfoResultModel>> SearchByBuyerAsync(OrderSearchRequest req);

        // Search 4: App 儀表板總覽
        Task<ApiResponseWrapper<AppDashboardAggregateResultModel>> GetAppDashboardAsync(OrderSearchRequest req);

        // Search 5: App 銷售指標（本日）
        Task<ApiResponseWrapper<AppSalesMetricsResultModel[]>> GetAppSalesTodayAsync(OrderSearchRequest req);

        // Search 6: App 銷售指標（本週）
        Task<ApiResponseWrapper<AppSalesMetricsResultModel[]>> GetAppSalesWeekAsync(OrderSearchRequest req);

        // Search 7: 取得賣家 cgdm 資料
        Task<ApiResponseWrapper<UserCgdmDataResultModel>> GetUserCgdmDataAsync(OrderSearchRequest req);
    }
}
