# Sprint S14：賣家取號業務模擬 (Seller Get Number Simulation)

## 任務目標
在 `CPF.Sandbox` 中模擬「賣家取號 (Status 20)」業務流程，驗證物流模組 (E_Shipment) 的動態掛載與訂單狀態更新邏輯。

## 需求背景
根據「貨態更新_更新付款資訊_資料流」文件，Status 20 是物流生命週期的起點。此時系統需將 Redis 傳入的物流資訊 (esmm, esml, esms) 精確地寫入 MongoDB 原有的訂單 Document 中。

## 任務清單

### 1. 產生器擴充 (Infrastructure)
- [x] 在 `ProductionDataGenerator` 中新增 `GenerateStatus20Patch(coomNo)` 方法。
- [x] 欄位名稱與資料流完全一致：`esmm_ship_no`、`esmm_status`、`esmm_no`、`esml_esmm_status`、`esms_dlv_status_no` 等。

### 2. 實作模擬場景 (Application / Sandbox)
- [x] 新增 `CPF.Sandbox/Scenarios/SellerGetNumberScenario.cs`。
- [x] **步進驗證（6 步驟）**：
    1. **V1 (初始)**：Insert 只有 `c_order_m/c/d` 的訂單，`e_shipment_*` 為 null。
    2. **Read V1**：確認初始狀態無物流模組。
    3. **執行動作**：產生 Status 20 patch，扁平化為點符號路徑 $set 指令（陣列欄位整體替換）。
    4. **Read V2**：讀取更新後狀態。
    5. **對比報告**：8 項驗證，含狀態更新、物流掛載、原始資料保留。
    6. **完整 BsonDocument 前後對比**。

### 3. 沙盒整合 (Presentation)
- [x] 在 `Program.cs` 加入 `SellerGetNumberScenario.RunSellerGetNumberSimulation()`。

## 檢核點
- [x] 成功產出符合資料流規範的 MongoDB 更新指令（點符號路徑 + 陣列整體掛載）。
- [x] 步進報告證明「點符號路徑」與「子物件掛載」並存時的資料安全性。
- [x] 驗證 snake_case 轉換邏輯與正式資料流 100% 契合（`esmm_no`、`esml_esmm_status`、`esms_dlv_status_no`）。

## 完成日期
2026-04-24
