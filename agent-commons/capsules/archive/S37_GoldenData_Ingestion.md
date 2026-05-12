# Sprint S37：Golden Data 數據導入 (Ingestion)
tracking_label: P3-1

## 任務目標
將客戶提供的正式測試資料（`.gemini/Sample_Data/Elastic_Search/測試資料_Elastic.txt` 與 `測試資料_Mongo.txt`）導入本地開發環境，建立 GoldenRecipe 驗收的真實數據基座。

## 需求背景
為了執行高保真（High Fidelity）驗收並確保 Search 1-7 的聚合邏輯「0 誤差」，必須摒棄先前的隨機產生器（`ElasticSeeder`），改用客戶提供的真實 Bulk JSON。這也是後續驗證「雙引擎鏈路（ES 取 Key，Mongo 取詳細資料）」的必要前置作業。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：方案符合 GoldenRecipe 對齊戰略，為必備基礎建設 (已簽署 2026-05-06)
- [x] **Engineer**：技術可行性審核通過 (已簽署 2026-05-06)
    - Elastic Bulk 格式解析：可行（跳過 index metadata 行）
    - MongoDB JSON 植入：可行（標準 JSON 陣列格式）
    - CLI 入口整合：參考現有 seed-* 模式
- **核准日期**: 2026-05-06
- **狀態**: `ACTIVE`

---

## 任務清單
- [ ] **建立 GoldenSeeder**：在 `CPF.Sandbox/IntegrationTests/PipelineSeeders/` 下建立 `GoldenSeeder.cs`。
    - **Elastic 植入**：讀取 `.gemini/Sample_Data/Elastic_Search/測試資料_Elastic.txt`，解析其 Bulk 格式並將 Document 寫入 **`orders-605`** 索引。
    - **Mongo 植入**：讀取 `.gemini/Sample_Data/Elastic_Search/測試資料_Mongo.txt`，寫入 MongoDB `CpfOrderDb` 的 `Orders` Collection。
- [ ] **入口集成**：
    - 更新 `CPF.Sandbox/Program.cs`，新增 CLI 參數或互動選項 `seed-golden` 來觸發 `GoldenSeeder.SeedAsync()`。

---

## PM 驗收項目 (VCP)

### 1. 雙引擎數據量對齊
- 執行 `dotnet run --project CPF.Sandbox seed-golden`。
- 驗證 ElasticSearch：`GET orders-605/_count` 的回傳數值應與 `.txt` 檔案內的有效 Document 數量一致。
- 驗證 MongoDB：`Orders` Collection 內的 Document 數量應與對應的 `.txt` 一致。

---

## 技術檢核點
- [ ] 解析 Elastic Bulk 格式時，請注意跳過 `{ "index": { "_id": "..." } }` 行，只反序列化 Data 行。
- [ ] 建立 Elastic 索引時，請務必保留與生產環境一致的 Explicit Mapping（可參考原有的 `ElasticSeeder`）。
