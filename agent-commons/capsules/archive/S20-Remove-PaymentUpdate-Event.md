# Sprint S20: 移除付款更新事件 (Remove PaymentUpdateEvent)

## 1. 描述 (Description)
因應業務邏輯調整，決定移除「付款更新 (UpdatePaymentUpdateEvent)」的所有模擬與實作內容。

## 2. 工程師 (Kiro) 執行清單 (Decommission List)

### 🧹 檔案刪除
- [ ] 刪除 `CPF.Services.Redis.Post/Model/QueryModel/MongoDB/UpdatePaymentUpdateEvent.cs`

### ✂️ 代碼清理
- [ ] **Redis.Post 專案**：
    - `Worker_ForCPF.cs`: 移除選單【6】及 `UpdatePaymentUpdateFlow()` 方法。
    - `AddOrderEventRandomDataGenerator.cs`: 移除 `GetUpdatePaymentUpdateEventObject()` 與 `GetElasticUpdatePaymentUpdateEventObject()`。
- [ ] **Mongo.Worker 專案**：
    - `Worker.cs`: 移除 `case "UpdatePaymentUpdateEvent"` 及其處理邏輯。
- [ ] **Elastic.Worker 專案**：
    - `Worker.cs`: 移除 `case "UpdatePaymentUpdateEvent"` 及其處理邏輯。

## 3. 驗收標準 (Acceptance Criteria)
- [ ] 專案編譯成功 (Build Success)。
- [ ] `Redis.Post` 啟動後，按鍵選單不再出現編號【6】。
- [ ] 搜尋全專案不再出現 `UpdatePaymentUpdateEvent` 字樣（除本文件外）。
