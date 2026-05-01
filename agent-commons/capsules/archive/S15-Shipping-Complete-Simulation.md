# Sprint S15：寄貨完成與陣列追加模擬 (Shipping Complete)

## 任務目標
在 `CPF.Sandbox` 中模擬「寄貨完成 (Status 30)」流程，驗證 MongoDB 陣列追加 ($push) 邏輯，確保貨態歷程完整保留。

## 需求背景
根據「貨態更新_寄貨_資料流」文件，Status 30 涉及對現有物流陣列的增量更新。我們必須驗證 SDK 在處理點符號路徑 $set (狀態變更) 與 $push (歷程追加) 並行時的正確性。

## 任務清單

### 1. 產生器與 Mock 強化 (Infrastructure)
- [x] 在 `ProductionDataGenerator` 中新增 `GenerateStatus30Patch(coomNo)` 方法，回傳 `Status30Patch`（含 `SetFields`、`PushEsml`、`PushEsms`）。
- [x] 在 `MockOrderRepository` 中新增 `Push(filterJson, pushDoc)` 方法，模擬 `$push` 行為：對陣列欄位執行 `Add` 而非覆蓋。

### 2. 實作步進場景 (Application / Sandbox)
- [x] 新增 `CPF.Sandbox/Scenarios/ShippingCompleteScenario.cs`。
- [x] **驗證流程（6 步驟）**：
    1. **初始化**：Insert 基礎訂單 + 套用 S14 Status 20 物流掛載（e_shipment_l 筆數=1）。
    2. **Read V1**：記錄 Status 20 狀態。
    3. **Status 30 更新**：先 `$set`（coom_status=30、esmm_status=10），再 `$push`（追加 esml/esms 各一筆）。
    4. **Read V2**：獲取最終狀態。
    5. **對比報告**：10 項驗證，含狀態切換、陣列筆數、歷史記錄保留。
    6. **完整 BsonDocument 前後對比**。

### 3. 沙盒整合 (Presentation)
- [x] 在 `Program.cs` 加入 `ShippingCompleteScenario.RunShippingCompleteSimulation()`。

## 檢核點
- [x] 產出的指令正確包含 `$set` 與 `$push` 兩個運算子（分兩步執行）。
- [x] 對比報告證明「陣列增量掛載」成功，esml 同時保留 "01" 與新增 "10"，無資料遺失。
- [x] 日期格式以 ISO 8601 字串儲存，追加後格式一致。

## 完成日期
2026-04-24
