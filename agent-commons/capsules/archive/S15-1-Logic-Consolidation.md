# Sprint S15.1：邏輯收攏與真實性強化 (SDK Fidelity)

## 任務目標
徹底消除沙盒與 SDK 之間的邏輯重複，確保所有 Mock 驗證均基於 `MongoRepository` 的真實代碼實作。

## 需求背景
為了確保「沙盒通過即代表生產環境通過」，必須將 `MongoRepository` 的核心邏輯抽離。目前沙盒中自定義的 `FlattenNonNull` 函數可能與 SDK 實際行為產生偏離，必須予以根除並收攏。

## 任務清單

### 1. 建立 SDK 核心指令工具 (Infrastructure)
- [x] 建立 `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/Utils/MongoCommandBuilder.cs`。
- [x] 遷移邏輯：`Flatten()`（含點符號路徑、Null 排除、_id 忽略、陣列保留）與 `TryConvertToBsonDateTime()`（多語系日期轉換）。
- [x] **SDK 同步**：重構 `MongoRepository.cs`，`FlattenBsonDocument` 與 `TryConvertToBsonDateTime` 均委派給 `MongoCommandBuilder`，移除重複實作。

### 2. 升級 Mock 體系 (Application / Sandbox)
- [x] **重構 MockOrderRepository**：加入 `MongoCommandBuilder` using，Insert 說明文件更新。
- [x] **清理沙盒專案**：
    - 刪除 `SandboxRunner` 的 `FlattenNonNull()` 私有方法。
    - 刪除 `StatefulComparisonScenario` 的 `FlattenNonNull()` 私有方法。
    - 刪除 `SellerGetNumberScenario` 的 `FlattenNonNull()` 私有方法。
    - 刪除 `ShippingCompleteScenario` 的 `FlattenNonNullWithArrays()` 私有方法。
    - 所有場景統一改為呼叫 `MongoCommandBuilder.Flatten()`。

### 3. 全面更新舊有場景
- [x] **S13 (Stateful)**：對比報告使用 SDK Builder 扁平化結果。
- [x] **S14 (Status 20)**：物流模組掛載指令由 `MongoCommandBuilder.Flatten()` 產出。
- [x] **S15 (Status 30)**：`$set` 指令由 `MongoCommandBuilder.Flatten()` 產出。

## 檢核點
- [x] 全案編譯成功（0 錯誤）。
- [x] 沙盒輸出的指令 JSON 格式與 SDK `MongoCommandBuilder.Flatten()` 輸出完全對齊。
- [x] 沙盒代碼中不再含有任何自定義的 BSON 扁平化處理邏輯（`grep private.*Flatten` 無結果）。

## 完成日期
2026-04-24
