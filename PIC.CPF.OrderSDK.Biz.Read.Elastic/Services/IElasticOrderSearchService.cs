using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;
// ⚠️ 記得引用正確的 Models 命名空間
// 如果你決定共用舊版的 Model，這裡就是 using PIC.CPF.OrderSDK.Biz.Read.OpenSearch.Models;
// 如果你把 Model 搬到新專案了，這裡就是 using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models;

namespace PIC.CPF.OrderSDK.Biz.Read.Elastic.Services
{
    public interface IElasticOrderSearchService
    {
        // Flow 1: Web 網頁端代辦事項總覽
        Task<AggregateOrderInfoResultModel> AggregateOrderInfoAsync(AggregateOrderInfoModel model);

        // Flow 2: 一般賣家訂單搜尋
        Task<SearchOrderInfoResultModel> SearchOrderInfoAsync(SearchOrderInfoModel model);

        // Flow 3: App 儀表板總覽
        Task<AppDashboardAggregateResultModel> AppAggregateOrderInfoAsync(AppAggregateOrderInfoModel model);

        // Flow 3: App 銷售指標
        Task<AppSalesMetricsResultModel[]> AppSalesMetricsInfoAsync(AppSalesMetricsModel[] model);
    }
}