# Sprint S3：Repository 架構優化與 Result 泛型化

## 任務目標
優化 Repository 依賴模式（改用組合模式）並實作 `IResult<T>` 泛型化，提升架構解耦度與型別安全性。

## 需求背景
1. **DIP 違反修正**：`Application/Sample` 內的 Repository 目前繼承了 `Infrastructure` 的具體實作，需改為組合模式。
2. **型別安全**：`IResult` 目前回傳 JSON 字串，需改為 `IResult<T>` 直接回傳強型別物件，減少重複的反序列化邏輯。

## 任務清單
- [x] **Core 層介面升級**
    - [x] 定義 `IResult<out T> : IResult`，增加 `T Data { get; }` 屬性。
    - [ ] 更新 `IRepository<T>` 簽章：`GetData` 回傳 `IResult<IEnumerable<T>>`，`Insert/Update` 回傳 `IResult<T>`。（暫緩，保持向下相容）
- [x] **Infrastructure 層實作**
    - [x] 實作 `Result<T>` 泛型類別，提供工廠方法。
- [x] **Application 層重構 (關鍵)**
    - [x] 重構 `OrderRepository_Mongo`：改為實作 `IRepository<Orders>` 並注入底層 Repository。
- [x] **Mock 驗證環境**
    - [x] 在 `Program.cs` 中實作 `TestFlow_Mock` 離線驗證機制。

## 檢核點
1. ✅ `Sample` 層不再引用 `MongoDB.Driver` 或 `Elastic.Clients`（透過介面解耦）。
2. ✅ 呼叫端可直接存取 `result.Data` 而非解析 `DataJson`。
3. ✅ 成功執行 `Program.cs` 中的 Mock 測試流程。

## 完成日期
2026-04-23
