# Sprint S18：實際測試環境場景集成 (Real Environment Integration)

## 任務目標
將 S17 驗證通過的貨態更新與付款流程，正式集成至 `Redis.Post`, `SendDataToMongoDB`, `SendDataToElasticCloud` 三大服務中，確保在實際運行環境下能透過按鍵觸發完整流程。

## 需求背景
客戶需要能在測試環境中透過模擬操作（如按鍵觸發 Redis 事件）來觀測資料在庫存與檢索端的異動。

## 任務清單

### 1. Redis 事件源重構 (CPF.Services.Redis.Post)
- [ ] **擴充選單**：在 `ExecuteAsync` 加入【4】取號、【5】寄貨、【6】付款更新選項。
- [ ] **實作發送 Flow**：
    - [ ] `UpdateCargoDynamics01Flow()`：發送取號事件。
    - [ ] `UpdateCargoDynamics02Flow()`：發送寄貨事件。
    - [ ] `UpdatePaymentUpdateFlow()`：發送付款更新事件。

### 2. MongoDB 處理器擴充 (CPF.Service.SendDataToMongoDB)
- [ ] **增加 Switch Case**：
    - [ ] `UpdateSellerGetNumberEvent`：實作取號邏輯，初始化 `E_Shipment_M/L/S`。
    - [ ] `Delivery_CargoDynamics_02`：實作寄貨邏輯，使用 `$set` 狀態並 `$push` 新的配送紀錄。
    - [ ] `UpdatePaymentUpdateEvent`：更新付款狀態。

### 3. Elastic 處理器同步 (CPF.Service.SendDataToElasticCloud)
- [ ] **同步貨態欄位**：
    - [ ] 增加對應 Case，確保 Elastic 中的 `coom_status`, `esmm_status`, `esmm_ship_no` 等欄位同步更新。

### 4. 資料產生器補強 (Model / Generator)
- [ ] 在 `AddOrderEventRandomDataGenerator` 中補齊對應的 Get 方法，回傳符合 S17 格式的 Request 物件。

## 檢核點
- [ ] 啟動三個服務後，按下 Redis.Post 的按鍵，能在 MongoDB/Elastic 日誌中看見正確的異動紀錄。
- [ ] 程式碼風格與現有的 `Worker_ForCPF` 保持 100% 一致。
- [ ] 所有的日期處理符合 `BsonDateTime` 規範。

## 完成日期
2026-04-28
