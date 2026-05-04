# Sprint S28：BLL 入口點大重構
tracking_label: P28

## 任務目標
將 `ElasticOrderSearchBll` 的所有公開方法重構為統一入口模式，所有入參改用 `OrderSearchRequest`。

## 需求背景
確保 BLL 為唯一的 `IResult<T>` 邊界，對外不洩漏內部查詢模型。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-01
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 所有 7 個 BLL 方法入參統一改為 `OrderSearchRequest`
- [x] 所有方法回傳 `IResult<T>`，BLL 內部 try/catch 捕捉例外
- [x] 包含：`GetHomeToDoOverView`, `SearchBySeller`, `SearchByBuyer`, `GetAppDashboard`, `GetAppSalesToday`, `GetAppSalesWeek`, `GetUserCgdmData`

---

## PM 驗收項目

本 Sprint 為 BLL 介面重構，無獨立 PM 可操作的驗收項目。  
7 個方法的功能正確性統一在 **S30** 以 GoldenRecipe 端對端驗收。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認 BLL 正確性由 S30 全部情境驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] BLL 介面無任何 `string ConditionData_Json` 洩漏
- [x] 所有方法回傳 `IResult<T>`
