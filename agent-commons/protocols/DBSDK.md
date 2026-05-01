---
status: USER-RATIFIED
mutability_default: APPEND-ONLY
created_by: user-ratified-from-ai-draft
created_at: 2026-05-01
---

# dbSDK 開發鐵律 (Domain Axioms) - V1.0

> 版本：v1.0 · USER-RATIFIED · 官方正式版
> 位階：本文件為本專案最高且唯一不可妥協之領域底線。
> 校正紀錄：2026-05-01 由 user 與工程師共同校正完成；5 條大幅改寫 / 1 條新增。

---

## ① Two-Stage Static Initialization (兩階段靜態初始化)

系統啟動時必須嚴格遵守兩階段初始化順序：
1. `MongoSerializationConfig.Register()`：優先註冊全域 BSON 序列化配置。
2. `MongoMap.EnsureClassMapsRegistered()`：註冊實體類別映射（使用 Double-checked locking）。

> **後果**：若順序顛倒或遺漏第一步，將導致 BsonElement 映射異常，甚至引發「Element '_id' does not match any field」的生產事故。

> **驗證**：檢查 `Program.cs` 的 DI 設定，確認 `MongoMap` 已註冊為 singleton（其建構子會自動觸發註冊邏輯），或在 Sandbox 的 `Program.cs` 頂部確認兩行呼叫順序。

## ② Dual-Track Date Normalization (雙軌日期正規化)

日期轉換必須覆蓋「讀取」與「寫入」兩條獨立路徑：
- **讀取路徑 (Deserialization)**：由 `MultiCultureDateTimeSerializer` 處理反序列化，支援「上午/下午」格式。
- **寫入路徑 (Update/Flatten)**：由 `MongoCommandBuilder.TryConvertToBsonDateTime()` 在 Flatten 過程中自動轉換。

> **後果**：僅處理單軌將導致「讀得出來但寫不進去」或「更新時日期變成字串」的資料不一致問題。

> **驗證**：確認 `MongoMap.cs` 與 `MongoCommandBuilder.cs` 同步持有最新的日期格式清單。

## ③ Flattened Patch & Composite Update (扁平化與複合更新)

MongoDB 更新必須遵循以下紀律：
- **基本更新**：強制使用 `MongoCommandBuilder.Flatten` 進行路徑扁平化，防止子欄位覆蓋。
- **複合更新**：當需要同時執行 $set, $unset, $push 時，必須使用 `MongoUpdateOptions` 的 `UpdateData` 重載，嚴禁分拆為多次獨立更新。

> **後果**：分拆更新會產生 Race Condition，且在網路不穩時可能導致原子性失效（部分更新成功、部分失敗）。

> **驗證**：檢查 `Worker.cs` 涉及支付方式變更 (S20) 的邏輯，確認使用了 `MongoUpdateOptions`。

## ④ Immutable Query Boundaries (不可變查詢邊界)

`Core` 與專案自訂的查詢條件（如 `CRUD_Condition_COOM`）應盡可能保持不可變。必須確保查詢主鍵（如 `_coom_no`）在賦值後不被修改。

> **後果**：查詢主鍵的可變性在平行處理 (S21) 中是致命的 Race Condition 來源。

> **驗證**：檢查 `CRUD_Condition_COOM` 等自訂條件類別的屬性，其主鍵欄位應無 `set` 存取子。

## ⑤ Contract-First Worker Pattern & Mapping Debt (契約優先與映射技術債)

交付專案 (Worker) 應經由 `IEngine` 或 `IRepository` 介面操作。目前 `Worker.cs` 中存在約 100 行的手動 Mapping 程式碼（技術債），未來應逐步遷移至 `UniversalMapper` 或 `IDTO` 統一處理。

> **後果**：手動 Mapping 增加維護難度，且易在新增欄位時漏掉對應，導致資料遺失。

> **架構觀察**：目前 `IDTO` 在 Worker 流程中尚未發揮核心轉換作用，此為已知技術債。

## ⑥ Debug-First Command Preview (偵錯優先指令預覽)

在執行複雜更新前，開發者應優先使用 `UpdateInit` 方法預覽生成的 MongoDB 指令 JSON。

> **後果**：盲目執行複雜的 $push/$unset 操作可能導致資料庫索引崩潰或資料結構污染，且事後難以追蹤指令內容。

> **驗證**：在執行複雜更新前，先呼叫 `UpdateInit(condition, data, options)` 並輸出 `result.DataJson`，確認 Filter 與 UpdateDefinition 符合預期後，再替換為 `UpdateData`。
