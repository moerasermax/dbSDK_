# DRAFT_CONTEXT

> 最後更新：2026-05-06
> 當前任務：S37 Golden Data 數據導入

---

## 當前進度

### S37：Golden Data 數據導入 (Ingestion) — 進行中

**狀態**：🛠 實作中（Build 成功，待 Docker 環境驗證）

**已完成**：
1. ✅ Co-sign 簽署膠囊（2026-05-06）
2. ✅ 建立 `CPF.Sandbox/IntegrationTests/PipelineSeeders/GoldenSeeder.cs`
3. ✅ 更新 `Program.cs` 加入 `seed-golden` 命令入口
4. ✅ Build 成功（0 錯誤）

**待完成**：
- [ ] Docker 環境相容性：確認連線設定是否需要調整
- [ ] 執行 `dotnet run --project CPF.Sandbox seed-golden` 驗證
- [ ] VCP 驗收：
  - `GET orders-605/_count` 確認 Elastic 數量（預期 24 筆）
  - MongoDB `Orders` Collection count 確認（預期 24 筆）

---

## 技術備忘

### GoldenSeeder 實作細節
- **Elastic 植入**：解析 Bulk 格式（跳過 `{ "index": ... }` metadata 行），寫入 `orders-605` 索引
- **Mongo 植入**：解析 JSON 陣列，寫入 `CpfOrderDb.Orders` Collection
- **連線方式**：比照 ElasticSeeder 使用 `ConnectionSettings` + `DbDetail`

### 測試資料位置
- `.gemini/Sample_Data/Elastic_Search/測試資料_Elastic.txt`（Bulk 格式，24 筆）
- `.gemini/Sample_Data/Elastic_Search/測試資料_Mongo.txt`（JSON 陣列，24 筆）

### 相關檔案
- 任務膠囊：`agent-commons/capsules/S37_GoldenData_Ingestion.md`
- 參考實作：`CPF.Sandbox/IntegrationTests/PipelineSeeders/ElasticSeeder.cs`
- OrderDocument：`PIC.CPF.OrderSDK.Biz.Read.Elastic/Models/Internal/OrderDocument.cs`

---

## Phase 3 總覽

| Sprint | 任務名稱 | 狀態 |
|--------|----------|------|
| S37 | Golden Data 數據導入 | 🛠 進行中 |
| S38 | 雙引擎鏈路整合模擬 | 📝 DRAFT |
| S39 | Search 1-7 GoldenRecipe 邏輯對齊 | 📝 DRAFT |

---

## 下一步（續接 Session）

1. 確認 Docker 環境 Elastic/Mongo 連線設定
2. 執行 `seed-golden` 植入測試資料
3. 執行 VCP 驗收
4. 完成 S37 後進入 S38（雙引擎整合）
