# Sprint S15.1：SDK 原生邏輯鏈結 (SDK Native Fidelity)

## 任務目標
打破測試代碼與生產代碼的牆，確保 `CPF.Sandbox` 與 `MockOrderRepository` 直接呼叫 `MongoRepository` 類別內的原生函式進行邏輯驗證。

## 需求背景
目前沙盒中存在重複的扁平化邏輯，這會導致驗證結果與真實 SDK 行為脫節。我們必須落實「如實呼叫」，將 `MongoRepository` 的核心處理能力開放給測試體系使用。

## 任務清單

### 1. 開放 SDK 核心能力 (Infrastructure / Persistence)
- [x] **存取權限調整**：在 `MongoRepository.cs` 中，將以下方法從 `private` 提升為 `public static`：
    - `FlattenBsonDocument`（委派 `MongoCommandBuilder.Flatten`）
    - `TryConvertToBsonDateTime`（委派 `MongoCommandBuilder.TryConvertToBsonDateTime`）
- [x] **確保穩定性**：不更動任何業務邏輯，僅修改存取修飾詞。

### 2. 重構 Mock 鏈結 (Application / Sample)
- [x] **升級 MockOrderRepository**：
    - `Update` 方法直接呼叫 `MongoRepository<BsonDocument>.FlattenBsonDocument()` 進行扁平化，再套用至 in-memory store。
    - 移除 `MongoCommandBuilder` using，改為 `MongoRepository` using。
- [x] **委派原生執行**：Mock 的 `Update` 路徑與生產環境 `UpdateData` 的扁平化路徑完全一致。

### 3. 全面清理沙盒 (CPF.Sandbox)
- [x] **移除冗餘**：所有 Scenario 移除 `MongoCommandBuilder` using。
- [x] **同步更新**：
    - `SandboxRunner`：直接呼叫 `MongoRepository<BsonDocument>.FlattenBsonDocument()`。
    - `SellerGetNumberScenario`（S14）：直接傳入原始 BsonDocument，由 `MockOrderRepository.Update` 呼叫 SDK 扁平化。
    - `ShippingCompleteScenario`（S15）：同上。
    - `StatefulComparisonScenario`（S13）：移除 `MongoCommandBuilder` using。

## 檢核點
- [x] `CPF.Sandbox` 成功編譯（0 錯誤），輸出指令經過 `MongoRepository.FlattenBsonDocument()` 原生邏輯處理。
- [x] 程式碼庫中不再存在重複的「BSON 扁平化」實作（`grep MongoCommandBuilder` 在沙盒中無結果）。
- [x] 驗證結果與之前一致，但具備了 100% 的技術真實性。

## 完成日期
2026-04-24
