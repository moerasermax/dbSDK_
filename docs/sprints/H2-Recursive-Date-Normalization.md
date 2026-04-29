# Sprint H2：BsonArray 遞迴日期正規化 (Hotfix)

## 任務目標
修正 `MongoCommandBuilder.Flatten` 邏輯，確保在扁平化過程（Update 操作）中，陣列（BsonArray）內部的日期字串也能被正確識別並轉換為 `BsonDateTime`，維持資料庫內部的型態一致性。

## 需求背景
使用者在執行 `S15 ShippingCompleteScenario` 時發現，初始插入的陣列元素具有 MongoDB `$date` 型態，但透過 Update 追加的陣列元素卻保留為原始 `String`。這將導致：
1. 資料庫查詢（如按時間排序）失效。
2. 反序列化時可能因為型態不一致而崩潰。

## 任務清單
- [ ] **[核心] 修改 `MongoCommandBuilder.Flatten`**：
    - 實作遞迴掃描 `BsonArray` 的邏輯。
    - 當遇到陣列時，遍歷其內部元素：
        - 若元素為 `BsonDocument`，應遞迴執行處理邏輯。
        - 若元素為 `BsonValue` (String)，應執行 `TryConvertToBsonDateTime`。
- [ ] **[驗證] 執行 `CPF.Sandbox` 驗證**：
    - 重新執行 `ShippingCompleteScenario`。
    - 確認 V2 輸出中，`e_shipment_l` 與 `e_shipment_s` 的所有節點均帶有 `{ "$date" : ... }`。

## 檢核點
| 編號 | 檢核項目 | 驗收標準 |
|------|----------|----------|
| 1 | 陣列內部日期轉換 | 模擬輸出中，追加的陣列元素日期必須為 `$date` 型態 |
| 2 | 遞迴深度驗證 | 深層巢狀 Document 中的陣列日期也應被轉換 |
