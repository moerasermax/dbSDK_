# Sprint S6：系統穩定性與相容性驗證

## 任務目標
確保 SDK 重構後，除了特定排除的專案外，其餘所有相關專案均能正常編譯並執行，且 DI 注入邏輯正確。

## 任務清單

### 1. 修正編譯錯誤 (Compile-time Fixes)
- [x] **修正 `CPF.Service.MongoElastic.UnitTest` 引用**（部分完成，測試邏輯待調整）
    - [x] 將 `Core.DTO` 命名空間替換為 `Infrastructure.DTO` (檔案：`ElasticTest.cs`, `MongoDB.cs`)。
    - [x] 將 `Order` 型別名稱更正為 `Orders` (檔案：`MongoDB.cs`)。
    - [ ] 測試程式碼需改用 `TestQuery` 類別（暫不處理）。
- [x] **全域編譯驗證**：執行 `dotnet build` 排除 `Read.Elastic`。

### 2. 執行期 DI 註冊校對 (Runtime Verification)
- [x] **檢查 `CPF.Service.SendDataToMongoDB`**
    - [x] 驗證 `Program.cs` 的 DI 註冊。
    - [x] 修正 `OrderRepository_Mongo` 同時實作 `IRepository<Orders>` 與 `IMongoDBRepository<Orders>`。
- [x] **檢查 `CPF.Service.SendDataToElasticCloud`**
    - [x] 驗證 Worker 的 SDK 呼叫點。
- [x] **檢查 `CPF.Services.Redis.Post`**
    - [x] 驗證隨機資料產生器整合。
    - [x] 修正命名空間衝突 (`UpdateCoomSellerMemo.cs`)。

### 3. 執行期錯誤修復 (Runtime Bug Fixes)
- [ ] **修正 PK 序列化異常 (BsonSerializationException)**
    - **問題描述**：執行 `Insert` 時，MongoDB Driver 將 `PK` 或 `id` 誤判為 `ObjectId`，導致 `... is not a valid 24 digit hex string` 錯誤。
    - **修復規格**：
        - 修改 `Infrastructure/Persistence/Mongo/MongoMap.cs`。
        - 對於 `Orders` 與 `OrderSummary`：使用 `cm.MapIdMember(c => c.PK)` 並設定 `StringSerializer(BsonType.String)`。
        - 對於 `EventGiftModel` 與 `EventGiftSummaryModel`：分別對 `id` 與 `Id` 欄位套用相同的 `MapIdMember` 配置。
        - 確保所有模型均已透過 `BsonClassMap.RegisterClassMap` 註冊。

### 4. 整合與冒煙測試 (Integration & Smoke Test)
- [ ] **SDK 實體連通測試**：驗證實體 DB 指令生成。
- [ ] **服務啟動驗證**：確保各服務 `Host` 啟動成功不崩潰。

## 檢核標準
1. ✅ 除了 `PIC.CPF.OrderSDK.Biz.Read.Elastic` 以外，所有專案 `dotnet build` 成功。
2. ⏳ `CPF.Service.MongoElastic.UnitTest` 測試案例可執行（測試邏輯待調整）。
3. ⏳ 各服務專案可正常啟動。

