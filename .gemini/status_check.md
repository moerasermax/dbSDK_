# Gemini 知識清單 & 狀態核對表

## 1. 核心實體變數定義 (Entity Definition)
*   **EventGiftModel**
    *   `id`: `string` (對應 MongoDB `_id`)
    *   `event_id`: `string` (業務識別碼)
    *   `show_type`, `run_type`, `inventory`: `int` (強型別)
    *   **日期欄位**: 全部為 `string` (例如 `event_date_s`)
        *   *注意*: 需透過 `FlattenBsonDocument` 在寫入前轉為 BsonDateTime。

## 2. 基礎模型定義 (Models)
*   **Result**
    *   `IsSuccess`: `bool`
    *   `Msg`: `string`
    *   `DataJson`: `string` (當前是非泛型實作)
*   **Condition**
    *   `_coom_no`: `string` (封裝查詢條件)

## 3. 架構層級現狀 (Architecture Status)
*   **IEngine<T>**: Application 層的 Use Case 入口。
*   **IRepository<T>**: Core 層介面，回傳 `Task<IResult>`。
*   **Sample Layer (DIP 違反)**: `OrderRepository_Mongo` 等範例直接繼承了 `Infrastructure` 的具體實作。
*   **Dependency Chain**: `Program.cs` -> `DbSDKEngine` -> `Repository (Sample)` -> `Repository (Infrastructure)`。

## 4. 當前待解 P0 任務
1.  **倉儲解耦**: 將 Sample Repository 改為組合模式。
2.  **Result 泛型化**: `IResult<T>` 實作以提升型別安全性。
