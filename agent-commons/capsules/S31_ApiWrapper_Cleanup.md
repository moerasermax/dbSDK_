# Sprint S31：基礎信封建立、全局清理與偵錯工具恢復
tracking_label: P2-1

## 任務目標
建立 `ApiResponseWrapper<T>` 標準回傳結構，徹底移除 `Took` 欄位，並恢復 Sandbox 的 `dump-sN` 偵錯命令。

## 需求背景
依據 GoldenRecipe，所有 API 輸出必須包裹在一個含有 `data`, `code`, `message`, `total` 的信封內。此外，為了後續 VCP 驗證，需恢復之前被移除的 `dump-sN` 命令。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## 任務清單
- [ ] 在 `PIC.CPF.OrderSDK.Biz.Read.Elastic.Models` 建立 `ApiResponseWrapper.cs` (泛型)
- [ ] 修改 `ElasticOrderSearchBll.cs` 的所有方法，將回傳值改為 `Task<ApiResponseWrapper<T>>`
- [ ] 從以下 4 個 ResultModel 中徹底移除 `Took` 屬性：
    - `AggregateOrderInfoResultModel` (S1)
    - `AppDashboardAggregateResultModel` (S4)
    - `SearchOrderInfoResultModel` (S2/3)
    - `AppSalesMetricsResultModel` (S5/6)
- [ ] 在 `CPF.Sandbox/Program.cs` 恢復 `dump-s1` 到 `dump-s7` 的入口命令，使其能輸出對應 Search 的 JSON 到 Console

---

## PM 驗收項目 (VCP)

### 1. JSON 物理形狀驗證
- 執行 `dotnet run --project CPF.Sandbox -- dump-s7`
- 驗證外層 Key：`jq -r 'keys | join(",")'` 應包含 `data,code,message,errorMsg,total`
- 驗證無 `Took` 欄位：`jq 'has("took") or has("Took")'` 應為 `false`

### 2. 偵錯工具可用性驗證
- 確保 `dotnet run --project CPF.Sandbox -- dump-s1` 到 `dump-s7` 均能正常執行並輸出 JSON

---

## 技術檢核點
- [ ] 程式碼編譯通過（0 errors）
- [ ] `Took` 欄位應從 Model 定義中完全刪除，而非僅在序列化時忽略
