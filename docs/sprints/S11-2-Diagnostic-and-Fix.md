# Sprint S11.2：非同步例外診斷與修復 (Diagnostic & Fix)

## 任務目標
揭露 `MongoRepository.GetData` 在執行 `Read` 操作時拋出的具體例外原因，並針對性地修復資料映射衝突。

## 需求背景
目前系統回報「函式名稱 MoveNext」的模糊錯誤，這是由於非同步狀態機封裝了原始錯誤。我們需要擴充診斷邏輯，獲取完整的 `StackTrace` 與 `InnerException` 以定位發生衝突的具體欄位（推測為日期格式或數值型別）。

## 任務清單

### 1. 強化診斷邏輯 (Infrastructure / Persistence)
- [ ] **修改 MongoRepository.cs**：
    - 在 `GetData` 方法的 `catch` 區塊中，將 `ex.Message` 調整為 `ex.ToString()`。
    - 範例：`return Result.SetErrorResult(MethodBase.GetCurrentMethod()?.Name, ex.ToString());`

### 2. 獲取診斷日誌 (Presentation)
- [ ] **執行測試流程**：
    - 運行 `TestFlows.RunMongoFlow()`。
    - 獲取詳細報錯資訊，特別是涉及 `BsonSerializationException` 的部分。

### 3. 根因分析與修復 (Core / Infrastructure)
- [ ] **根據日誌修復**：
    - 若為日期格式問題，調整 `MongoMap` 中的序列化配置。
    - 若為型別不匹配 (如 Int32 讀取為 Int64)，調整類別屬性型別。

## 檢核點
- [ ] 成功獲取詳細錯誤日誌。
- [ ] 修正後 `Read` 操作不再拋出異常。
- [ ] 全流程測試通過。

## 完成日期
2026-04-24
