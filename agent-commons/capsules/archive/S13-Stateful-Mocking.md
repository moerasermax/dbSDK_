# Sprint S13：Mock 狀態模擬與步進驗證 (Stateful Mocking)

## 任務目標
建立具備狀態記憶能力的 Mock 環境，執行「Insert -> Read -> Update -> Read」全生命週期驗證，確保資料演變邏輯 100% 正確。

## 需求背景
僅驗證指令 JSON 無法得知資料套用後的真實結果。透過在 Mock 層實作步進驗證，我們可以確認 SDK 的扁平化更新邏輯是否能在保留舊資料的同時，正確合併新變更。

## 任務清單

### 1. 實作內存資料庫 (Infrastructure)
- [x] 在 `MockOrderRepository` 中實作 `ConcurrentDictionary<string, BsonDocument>` 存儲。
- [x] **實作 Insert**：將 BsonDocument 以 `coom_no` 為 key 存入字典。
- [x] **實作 Update**：`ApplyDotNotationSet()` 逐層解析點符號路徑，null 值自動跳過不覆蓋。
- [x] **實作 GetData**：解析 Filter JSON 的 `coom_no` 欄位，從字典中檢索文件。

### 2. 實作步進驗證流程 (Application / Sandbox)
- [x] 在 `CPF.Sandbox/Scenarios/StatefulComparisonScenario.cs` 實作 `RunStepByStepVerification()`：
    1. **Insert**：寫入隨機生產級資料。
    2. **Read V1**：印出完整初始 BsonDocument。
    3. **Update**：混入 null 值（`coom_name`、`coom_cgdm_id` 設為 null），驗證排除邏輯。
    4. **Read V2**：印出更新後 BsonDocument。
    5. **對比報告**：表格清楚標示應變更欄位、應保留欄位與驗證結果。

## 檢核點
- [x] Console 輸出清晰展示了 V1/V2 兩次 `GetData` 的資料差異。
- [x] 證明 Update 操作後，未指定的欄位依然完整存在（No Data Loss）。
- [x] 證明點符號路徑 (Dot Notation) 成功作用於子物件內部。

## 完成日期
2026-04-24
