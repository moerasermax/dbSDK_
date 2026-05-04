# Sprint S30：Sandbox 驗證情境（GoldenRecipe 驗收）
tracking_label: P30

## 任務目標
建立 CPF.Sandbox 的 7 個 GoldenRecipe 情境，供 PM 執行端對端驗收。  
此 Sprint 是 S23–S29 所有交付物的統一驗收關卡。

## 需求背景
S23–S29 各 Sprint 均為內部實作，無獨立 PM 可操作驗收點。S30 以 CUN9101 測試資料集搭配 GoldenRecipe `.txt` 進行功能正確性最終確認。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-03
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 建立 `SearchSdkSetup.cs`（服務組裝 Factory）
- [x] 建立 `S23_GetHomeToDoOverViewScenario.cs`
- [x] 建立 `S24_SearchBySellerScenario.cs`
- [x] 建立 `S25_SearchByBuyerScenario.cs`
- [x] 建立 `S26_GetAppDashboardScenario.cs`
- [x] 建立 `S27_GetAppSalesTodayScenario.cs`
- [x] 建立 `S28_GetAppSalesWeekScenario.cs`
- [x] 建立 `S29_GetUserCgdmDataScenario.cs`
- [x] 在 `CPF.Sandbox/Program.cs` 加入 S23–S29 進入點（預設注解，需真實 ES 連線）
- [x] 建立 PM 驗收操作手冊（`agent-commons/pm-validation/V1_CUN9101_SearchSDK.md`）

---

## PM 驗收項目

> **前提**：Docker 全部 Up、ES `green`、`orders-604` 已植入 30 筆 CUN9101 樣本資料。  
> **操作**：在 `CPF.Sandbox/Program.cs` 取消注解 S23–S29 七行後執行，Console 自動印出 `✅ PASS` / `❌ FAIL`。

### 資料植入

| # | 驗證項目 | 驗證方式 | 期望值 |
|---|---------|---------|--------|
| 0-1 | ES 文件數量 | Kibana `GET orders-604/_count` | `"count": 30` |
| 0-2 | 文件格式 | Kibana `GET orders-604/_search`，抽驗 `cood_items` | snake_case，含 `cood_items` 巢狀陣列 |

### S23–S29 情境查核

| # | Sandbox 情境 | GoldenRecipe | 查核欄位 | 期望值 |
|---|-------------|-------------|---------|--------|
| 1-1 | S23 GetHomeToDoOverView | `Search_1_GetHomeToDoOverView.txt` | BuyerPerformance.OrderCount | 27 |
| 1-2 | S23 | 同上 | BuyerPerformance.PickupCount | 1 |
| 1-3 | S23 | 同上 | SellerPerformance.OrderCount | 40 |
| 1-4 | S23 | 同上 | SellerPerformance.SendCount | 11 |
| 1-5 | S23 | 同上 | SellerPerformance.SalesAmt | 475 |
| 2-1 | S24 SearchBySeller | `Search_2_SearchOrderInfoBySellerId.txt` | Total | 1998 |
| 2-2 | S24 | 同上 | 第一筆 coom_no | CM2604290379066 |
| 3-1 | S25 SearchByBuyer | `Search_3_SearchOrderInfoByBuyerId.txt` | Total | 53 |
| 3-2 | S25 | 同上 | 第一筆 coom_no | CM2604290379066 |
| 4-1 | S26 GetAppDashboard | `Search_4_GetAppDashboardOverview.txt` | NewOrderCnt | 15 |
| 4-2 | S26 | 同上 | ShippedCnt | 2 |
| 4-3 | S26 | 同上 | RepliedCnt | 1 |
| 4-4 | S26 | 同上 | SalesAmount | 1241 |
| 4-5 | S26 | 同上 | TotalOrderQty | 8 |
| 5-1 | S27 GetAppSalesToday | `Search_5_GetAppSalesMetrics(Today).txt` | TotalAmount | 88 |
| 5-2 | S27 | 同上 | TotalOrderCnt | 1 |
| 5-3 | S27 | 同上 | SalesTrendData["16"].Value | 88 |
| 5-4 | S27 | 同上 | TopProduct.ProductCgdmid | GM2512170027503 |
| 5-5 | S27 | 同上 | TopProduct.ProductTotalSales | 5 |
| 6-1 | S28 GetAppSalesWeek | `Search_6_GetAppSalesMetrics(Week).txt` | TotalAmount | 176 |
| 6-2 | S28 | 同上 | TotalOrderCnt | 2 |
| 6-3 | S28 | 同上 | SalesTrendData["04/28"].Value | 88 |
| 6-4 | S28 | 同上 | SalesTrendData["04/29"].Value | 88 |
| 6-5 | S28 | 同上 | TopProduct.ProductTotalSales | 10 |
| 7-1 | S29 GetUserCgdmData | `Search_7_GetUserCgdmData.txt` | CuamCid | 528672 |
| 7-2 | S29 | 同上 | Cgdm[0].CgdmId | GM2508260014245 |
| 7-3 | S29 | 同上 | Cgdm[0].CgdmUpdateDatetime | 2026-04-28T14:35:51.775 |
| 7-4 | S29 | 同上 | Cgdm[1].CgdmId | GM2512180014259 |
| 7-5 | S29 | 同上 | Cgdm[1].CgdmUpdateDatetime | 2026-04-28T14:35:36.628 |

### 通過條件
- ✅ 全部 Console 輸出 `PASS`，無任何 `FAIL`
- ❌ 任何 `FAIL` 回報格式：`[FAIL] SXX — 欄位 / 期望值 / 實際值`

### 驗收項目簽核
- [x] **PM 簽核**：確認上列項目合理（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認上列項目可由 Sandbox Console 客觀驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] `CPF.Sandbox` build 0 errors
- [x] `CPF.Sandbox.csproj` 已加入 `PIC.CPF.OrderSDK.Biz.Read.Elastic` 參考
- [x] S23–S29 七個情境檔案均已建立
