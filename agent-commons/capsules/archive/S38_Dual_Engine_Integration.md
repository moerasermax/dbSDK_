# Sprint S38：雙引擎鏈路整合模擬 (Dual-Engine Simulation)
tracking_label: P3-2

## 任務目標
在 Sandbox 環境中模擬現有系統的 `SearchOrderInfoBySellerIdAsync_ES` 運作流程，驗證「ElasticSearch 負責搜尋輸出 KeyList ➔ MongoDB 負責取回詳細資料」的雙引擎協作能力。

## 需求背景
現有系統採用 CQRS 架構（OpenSearch 搜尋 + DynamoDB 儲存）。我們的 SDK 必須實現無縫替換，改為（ElasticSearch + MongoDB）。在 S37 導入真實數據後，我們需要一條端對端的模擬測試鏈路，確保這兩個組件的協作不需要中介的二次 JSON 轉型（Zero-Cost Mapping）。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：符合雙引擎架構設計與熱插拔目標 (已簽署 2026-05-06)
- [ ] **Engineer**：待工程師審核實作可行性
- **核准日期**: 2026-05-06
- **狀態**: `DRAFT`

---

## 任務清單
- [ ] **實作 MongoDB 批量查詢**：確保 `MongoRepository` 或對應的 BLL 具備透過 `CoomNo` 列表（`List<string>`）高效取回多筆 `OrderData` 的能力。
- [ ] **建立模擬場景**：在 `CPF.Sandbox/Scenarios/` 建立 `S38_DualEngineSimulationScenario.cs`。
    - **Step 1**：呼叫 `ElasticOrderSearchBll.SearchBySellerAsync`，傳入 `Search_2` 的查詢條件。
    - **Step 2**：從回傳的 Data 中提取 `OrderInfos.CoomNo` 組成 `KeyList`。
    - **Step 3**：將 `KeyList` 傳入 MongoDB 取回詳細訂單資料。
- [ ] **介面對齊校驗**：確保整個過程中，不使用 `JsonSerializer.Serialize/Deserialize` 進行不必要的轉型。

---

## PM 驗收項目 (VCP)

### 1. 鏈路暢通性驗證
- 執行 `dotnet run --project CPF.Sandbox run-s38`。
- 驗證：能順利列印出從 MongoDB 取回的訂單詳細資料（如 `CoomSellerGoodsTotalAmt`），且無 mapping 報錯。

### 2. 架構邊界驗證
- 檢查代碼，確認 Elastic BLL 僅負責輸出條件與 Key，詳細業務欄位的組裝由 MongoDB 資料完成。

---

## 技術檢核點
- [ ] MongoDB 查詢應使用 `$in` 運算子批量查詢，避免 N+1 問題。
- [ ] 此場景主要是驗證「整合通順」，具體的數值 0 誤差對齊將在 S39 進行。
