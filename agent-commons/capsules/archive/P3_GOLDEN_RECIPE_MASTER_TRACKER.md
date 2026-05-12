# Phase 3: GoldenRecipe 像素級對齊與雙引擎整合 (Master Tracker)

> **建立日期**：2026-05-06
> **狀態**：ACTIVE
> **核心戰略**：從「隨機測試數據」全面轉向「以客戶正式 `.txt` 檔案為唯一真理的像素級對齊」，並確保 SDK 能以「零阻力」方式無縫替換現有系統。

## 🎯 階段目標 (Phase Objectives)

1.  **真實基座 (S37)**：導入 `orders-605` 與 Mongo 的真實客戶數據。
2.  **合約對齊 (Phase 2.A 持續)**：SDK 僅回傳 Data，無 Wrapper，屬性強制 camelCase。
3.  **雙引擎模擬 (S38)**：驗證 `Elastic 取 Key ➔ Mongo 取詳細資料` 的工作流。
4.  **邏輯 0 誤差 (S39)**：Search 1 到 7 的聚合結果，必須與 GoldenRecipe `Out` 完全吻合（特別是 S5/S6 補零邏輯）。

## 📋 Sprint 追蹤 (Sprint Tracking)

| Sprint | 任務名稱 | 狀態 | 負責人 | 預期產出 |
| :--- | :--- | :--- | :--- | :--- |
| **S37** | Golden Data 數據導入 (Ingestion) | 🏃 `ACTIVE` | Kiro | Elastic/Mongo 正式測試資料植入 |
| **S38** | 雙引擎鏈路整合模擬 | 📝 `DRAFT` | Kiro | Sandbox 模擬 DDB 換 Mongo 流程 |
| **S39** | Search 1-7 GoldenRecipe 邏輯對齊 | 📝 `DRAFT` | Kiro | Sandbox E2E 測試全數 PASS |

---

## 📚 參考附錄 (Reference Appendix)

*此區塊收錄由使用者提供之原始系統架構資訊，作為開發時之記憶參考（Source of Truth）。*

### 附錄 A：現有 API 呼叫鏈路 (Call Chain)
```text
首頁賣家總覽
PIC.CPF.OrderSDK.API
=>SearchController. GetHomeToDoOverview
=>PIC.CPF.OrderSDK.API.BLL.SearchOPSForAggregateData
=>PIC.CPF.OrderSDK.API.DAL.SearchOPSForAggregateData
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.Services. AggregateOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.BLL.AggregateOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.DAL.AggregateOrderInfo

首頁訂單數據(ES APP API)
PIC.CPF.OrderSDK.API
=>SearchController.GetAppDashboardOverview
=>PIC.CPF.OrderSDK.API.BLL.SearchOPSForAppAggregateData
=>PIC.CPF.OrderSDK.API.DAL.SearchOPSForAppAggregateData
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.Services.AppAggregateOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.BLL.AppAggregateOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.DAL.AppAggregateOrderInfo

賣家訂單查詢(商品名稱)、賣家訂單查詢(全部三個月)
PIC.CPF.OrderSDK.API
=>SearchController.SearchOrderInfoBySellerId
=>PIC.CPF.OrderSDK.API.BLL.SearchOrderInfoBySellerId
=>PIC.CPF.OrderSDK.API.DAL.SearchByOPS
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.Services.SearchOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.BLL.SearchOrderInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.DAL.SearchOrderInfo
=>OPS查詢語句 PIC.CPF.OrderSDK.Biz.Read.OpenSearch.DAL.OrderInfoQuery

銷售數據(ES APP API)(有較複雜查詢)
PIC.CPF.OrderSDK.API
=>SearchController.GetAppSalesMetrics
=>PIC.CPF.OrderSDK.API.BLL.SearchOPSForAppSalesMetricsData
=>PIC.CPF.OrderSDK.API.DAL.SearchOPSForAppSalesMetricsData
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.Services.AppSalesMetricsInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.BLL.AppSalesMetricsInfo
=>PIC.CPF.OrderSDK.Biz.Read.OpenSearch.DAL.AppSalesMetricsInfo
```

### 附錄 B：ES 雙軌獨立版本代碼 (Integration Examples)
*此段代碼展示了舊系統如何進行二次 JSON 轉型，以及 DynamoDB 如何與 Search 串接。我們的目標是消除 JsonSerializer 轉換，並將 DDB 替換為 Mongo。*

```csharp
#region ES Parallel Methods (ES 雙軌獨立版本)

// ==========================================
// // Web 代辦事項總覽
// ==========================================
public async Task<GetHomeToDoOverview> SearchForAggregateDataAsync_ES(AggregateOrderInfoModel model)
{
    var esResult = await _searchDal.SearchForAggregateData_ES(model);

    var jsonString = JsonSerializer.Serialize(esResult);
    var opsResult = JsonSerializer.Deserialize<AggregateOrderInfoResultModel>(jsonString);

    GetHomeToDoOverview result = ConvertToExtension.ConvertToGetHomeToDoOverviewResult(opsResult);
    return result;
}

// ==========================================
// // 賣家訂單搜尋 (結合 DDB)
// ==========================================
public async Task SearchOrderInfoBySellerIdAsync_ES(DDBResultModel result, SearchOrderInfoModel model)
{
    IEnumerable<OrderData> DDBResult = new List<OrderData>();
    var esResult = await _searchDal.SearchByES(model);

    if (esResult.Total > 0)
    {
        result.Total = esResult.Total;
        List<string?> KeyList = esResult.OrderInfos.Select(x => x.CoomNo).ToList();

        var DDBData = _searchDal.SearchByDDB(KeyList);
        result.Data = ConvertToExtension.ConvertToOrderData(DDBData);
    }
}

// ==========================================
// // 買家訂單搜尋 (結合 DDB)
// ==========================================
public async Task SearchOrderInfoByBuyerIdAsync_ES(DDBResultModel result, SearchOrderInfoModel model)
{
    IEnumerable<OrderData> DDBResult = new List<OrderData>();
    var esResult = await _searchDal.SearchByES(model);

    if (esResult.Total > 0)
    {
        result.Total = esResult.Total;
        List<string?> KeyList = esResult.OrderInfos.Select(x => x.CoomNo).ToList();

        var DDBData = _searchDal.SearchByDDB(KeyList);
        result.Data = ConvertToExtension.ConvertToOrderData(DDBData);
    }
}

// ==========================================
// // App 儀表板總覽
// ==========================================
public async Task<GetAppDashboardOverview> SearchForAppAggregateDataAsync_ES(AppAggregateOrderInfoModel model)
{
    var esResult = await _searchDal.SearchForAppAggregateData_ES(model);

    var jsonString = JsonSerializer.Serialize(esResult);
    var opsResult = JsonSerializer.Deserialize<AppDashboardAggregateResultModel>(jsonString);

    GetAppDashboardOverview result = ConvertToExtension.ConvertToGetAppDashboardOverviewResult(opsResult);
    return result;
}

// ==========================================
// // App 銷售指標
// ==========================================
public async Task<GetAppSalesMetrics> SearchForAppSalesMetricsDataAsync_ES(AppSalesMetricsModel[] model)
{
    var esResult = await _searchDal.SearchForAppSalesMetricsData_ES(model);

    var jsonString = JsonSerializer.Serialize(esResult);
    var opsResult = JsonSerializer.Deserialize<AppSalesMetricsResultModel[]>(jsonString);

    GetAppSalesMetrics result = ConvertToExtension.ConvertToGetAppSalesMetricsResult(opsResult);
    return result;
}

#endregion
```
