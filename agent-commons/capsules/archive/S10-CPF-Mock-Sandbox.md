# Sprint S10：CPF 業務模擬沙盒 (CPF Mock Sandbox)

## 任務目標
在 `CPF.Services.Redis.Post` 專案中建立一個模擬測試環境，利用現有的隨機資料產生器，驗證從 Redis 接收資料到產出 MongoDB 指令的全流程邏輯。

## 需求背景
為了確保 `CPF.Services.Redis.Post` 在處理各類 Redis 事件時產出的資料庫指令正確無誤，我們需要結合 `AddOrderEventRandomDataGenerator` 與重構後的 `MockOrderRepository`，實現一個不依賴環境的「業務邏輯驗證沙盒」。

## 任務清單

### 1. 建立模擬測試入口 (Presentation)
- [x] 在 `CPF.Services.Redis.Post` 專案中新增目錄 `Sandbox`。
- [x] 新增 `Sandbox/CpfMockSandbox.cs` 靜態類別。
- [x] 實作 `RunSimulation()` 方法，依序執行三個場景。

### 2. 模擬多重事件場景 (Application)
- [x] **新增訂單事件 (AddOrder)**：驗證 coom、cooc、cood、cgdi 完整轉換為 BsonDocument。
- [x] **變更支付方式 (ChangePayType)**：模擬 `$set` 指令，驗證 Filter 以 `cooc_no` 為條件。
- [x] **更新備註 (UpdateSellerMemo)**：模擬局部 `$set`，驗證點符號路徑 `c_order_m.coom_seller_memo`。

### 3. SDK 工具整合 (Infrastructure)
- [x] 直接使用 `ToBsonDocument()` 與自訂 `RemoveNullFields()` 實現局部更新邏輯。
- [x] 不依賴 SDK MockOrderRepository，沙盒自行實作輕量版指令產生器。

## 檢核點
- [x] 模擬程式能成功執行且不拋出異常（編譯 0 錯誤）。
- [x] Console 輸出的指令 JSON 格式正確，Filter 條件正確對應 CoomNo / CoocNo。
- [x] `$set` 指令中排除掉了 null 值，符合「局部更新」規範。

## 完成日期
2026-04-24
