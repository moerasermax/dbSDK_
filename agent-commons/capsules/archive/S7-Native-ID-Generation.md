# Sprint S7：切換 MongoDB 原生 ID 產生模式 (Native ObjectId)

## 任務目標
將 MongoDB 的 `_id` 產生邏輯回歸原生模式。不再強制將業務主鍵（`PK` / `id`）映射為資料庫的 `_id` 欄位。

## 需求背景
客戶需求變更：希望解耦資料庫技術主鍵（_id）與業務主鍵（PK）。這能確保資料庫層級的唯一性由 MongoDB 原生 ObjectId 保證，而業務層級的編號（如 `coom_no`）僅作為一般索引欄位存在。

## 任務清單

### 1. Infrastructure 層：修改 MongoMap.cs
- [x] 修改 `Infrastructure/Persistence/Mongo/MongoMap.cs` 中的 `EnsureClassMapsRegistered()`。
- [x] **移除強制 ID 映射**：針對 `Orders`、`OrderSummary`、`EventGiftModel`、`EventGiftSummaryModel`，將原本的 `MapIdMember` 改回一般的 `MapMember`（或直接使用 `AutoMap` 預設行為）。
- [x] **確保欄位型別**：雖然不再是 `_id`，但 `PK` 或 `id` 欄位仍應維持 `StringSerializer(BsonType.String)` 序列化設定，以確保業務編號儲存格式正確。
- [x] **檢查扁平化邏輯**：確保 `ToPatchDocument` 或相關更新邏輯不會無意中嘗試修改 `_id` 或排除必要的業務主鍵。

### 2. Core 層：檢查實體定義
- [x] 確認 `Orders.cs` 中的 `PK` 屬性僅作為普通屬性，不帶有強制對應 `_id` 的特性（除非客戶 JSON 指定）。

### 3. 驗證任務
- [x] 執行 `dotnet build` 驗證全案（排除 `Read.Elastic`）編譯狀態。
- [ ] **冒煙測試**：執行 `InsertData`，確認資料庫產生的 Document 包含：
    - 一個自動產生的 `_id` (型別為 `ObjectId`)。
    - 一個名為 `PK` (或客戶指定的名稱) 的字串欄位，儲存業務編號。

## 檢核點
- [ ] 存入 MongoDB 的資料，其 `_id` 欄位為 24 位元十六進制的 `ObjectId`。
- [ ] 業務主鍵欄位正常存在於 BsonDocument 中，不再與 `_id` 重疊。
- [ ] 系統不強制修改查詢條件 (Condition)，保留解析彈性。

## 完成日期
2026-04-24
