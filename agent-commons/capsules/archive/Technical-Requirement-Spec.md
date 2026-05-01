# 技術需求規格與 Kiro 評估指南

## 1. 資料流轉定義 (Data Flow Reference)
開發者必須參考 `.gemini/Sample_Data/` 下的以下檔案：
- `新增功能_貨態更新_資料流.txt` (Status 20)
- `新增功能_活態更新_寄貨_資料流.txt` (Status 30)

## 2. 核心技術規範 (Mandatory Rules)
- **禁止造輪子**：嚴禁重新編寫日期解析、BSON 扁平化或物件映射邏輯。
- **活用既有函數**：
    - 日期解析：必須透過 `MultiCultureDateTimeSerializer`。
    - 扁平化更新：必須透過 `FlattenBsonDocument` 並在此基礎上擴充。
    - 物件映射：必須透過 `UniversalMapper` (IUniversalMapper)。
- **原子化更新**：使用 MongoDB 的 `$set` 與 `$push` 確保更新不會覆蓋其他欄位。

## 3. Kiro 評估回報要求
在開始實作前，Kiro 必須針對以下技術問題回報其執行計畫：
1. **陣列追加策略**：目前的 `FlattenBsonDocument` 是針對物件屬性做 `$set`，你打算如何低侵入式地擴充它，讓它在偵測到 `List<T>` 且符合特定條件時改用 `$push` 或執行合併操作？
2. **型別一致性**：如何確保從 Redis 來的扁平化 `Args` 能透過 `UniversalMapper` 精準地轉換為 `OrderModel` 及其子模型？
3. **Mock 實作路徑**：如何利用 `IResult<T>` 在不啟動 DB 的情況下，模擬並驗證「貨態 1 到 貨態 2」的歷程追加結果？

## 4. 交付標準
- 代碼需通過 `TestFlow_Mock` 驗證。
- 符合「開發憲章」中的分層原則。
