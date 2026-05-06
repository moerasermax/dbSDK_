# Sprint S39：GoldenRecipe 邏輯 0 誤差校準 (Pixel-Perfect Alignment)
tracking_label: P3-3

## 任務目標
以客戶提供的 GoldenRecipe `.txt` 檔案為唯一真理，對齊 `Search_1` 到 `Search_7` 的輸入參數、預期結果、以及 SDK 的屬性命名格式（camelCase），達成聚合邏輯與物理形狀的「0 誤差」。

## 需求背景
這是 Phase 3 的最終決戰點。SDK 不僅要算出正確的數字，其回傳的物件實體在序列化後，長相必須與客戶的舊系統完全一致，才能實現直接接上 Controller 的「熱插拔」。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：符合 API 合約簡化（無 Wrapper、純 Data、camelCase）方針 (已簽署 2026-05-06)
- [ ] **Engineer**：待工程師審核實作可行性
- **核准日期**: 2026-05-06
- **狀態**: `DRAFT`

---

## 任務清單
- [ ] **API 物理形狀對齊 (Casing)**：
    - 檢查 `PublicModels`，確保所有對外屬性均使用 `[JsonPropertyName]` 標註為 **camelCase**（例如 `orderCount`、`salesTrendData`）。
    - 確認 SDK **不包含** `ApiResponseWrapper`，僅回傳 `Data` 實體。
- [ ] **Sandbox 腳本全面升級**：
    - 更新 `P2_SearchScenarioSuite.cs`。
    - 將 `RunSearch1Async` 到 `RunSearch7Async` 的輸入參數 (`In`) 與 `Check` 預期值 (`Out`)，**完全替換**為 `.gemini/Sample_Data/Elastic_Search/` 下的對應數據。
- [ ] **聚合邏輯精修 (尤其是 S5/S6)**：
    - 調整 `ElasticOrderSearchBll` 內 `GetAppSalesTodayAsync` (S5) 與 `GetAppSalesWeekAsync` (S6) 的「補零邏輯 (ZeroPadding)」與「同期比 (PoP)」算法，使其與 GoldenRecipe 產出的趨勢圖完全一致。

---

## PM 驗收項目 (VCP)

### 1. 屬性命名驗證
- 驗證：隨機 Dump 出來的 SDK 回傳 JSON，其欄位名稱必須是 `buyerOverView` 而非 `BuyerOverView`。

### 2. 邏輯 0 誤差驗證
- 執行 `P2_SearchScenarioSuite` 進行全量測試。
- 驗證：Console 必須針對 S1-S7 全數輸出 `✅ PASS`。不允許任何 `+/- 1` 的容差，必須精確命中 Golden Data。

---

## 技術檢核點
- [ ] 注意 S5/S6 趨勢圖中，客戶使用的時間標籤格式（如 `HH` 還是 `MM/dd`），補零時必須完全對齊。
- [ ] S1/S4 回傳 Object，S2/S3/S7 回傳 Array，需確保 SDK 的返回型別正確映射。
