# Sprint S16：Elastic 體系強化與強型別濾鏡 (Elastic Enhancement)

## 任務目標
提升 Elasticsearch 的開發體驗與驗證能力。透過強型別濾鏡減少硬編碼字串錯誤，並建立 Mock 體系使沙盒能驗證 Elastic 指令產生邏輯。

## 需求背景
目前 `ElasticFilter` 雖然已有初步的強型別方法，但：
1. **解析不夠穩定**：僅使用 `ToLower()` 處理欄位名稱，未考慮屬性對應屬性標籤（如 `JsonPropertyName`）。
2. **缺乏巢狀支援**：無法解析 `m => m.Logistics.Status` 這種點符號路徑。
3. **驗證斷層**：沙盒目前專注於 MongoDB/Redis，缺乏對 Elastic 指令的自動化斷言（Assertion）機制。

## 任務清單

### 1. 強型別濾鏡重構 (Infrastructure)
- [x] **優化 `GetFieldName<T>`**：
    - [x] 支援 `[JsonPropertyName]` 或 `[DataMember]` 屬性讀取。
    - [x] 支援巢狀屬性解析，將 `m => m.Inner.Field` 轉為 `inner.field`。
    - [x] 支援自動後置處理：針對 `string` 類型自動追加 `.keyword`（可選）。
- [x] **完善 `ElasticFilter` 方法**：
    - [x] `Range(Gte/Lte)`、`In`、`Exists`、`Wildcard` 等常用操作。

### 2. 建立 Elastic Mock 體系 (Application / Sandbox)
- [x] **建立 `MockElasticRepository<T>`**：
    - [x] 模擬 `GetData`, `InsertData`, `UpdateData` 行為。
    - [x] **核心功能**：攔截 `ElasticFilter` 產出的 `QueryDescriptor` 並將其序列化為 JSON，以便沙盒對比。
- [x] **建立 `ElasticCommandBuilder` (如有必要)**：收攏 Elastic 指令生成邏輯。

### 3. 沙盒驗證場景 (Presentation / Sandbox)
- [x] **新增 `ElasticSearchScenario`**：
    - [x] 模擬從 Condition JSON 轉為 Elastic 強型別 Filter。
    - [x] 驗證生成的 Elastic DSL JSON 是否符合預期（如：必須包含 `bool` -> `filter` 結構）。
    - [x] 驗證巢狀欄位與關鍵字（.keyword）的轉換正確性。

## 檢核點
- [x] `ElasticFilter` 能正確將 `m => m.CoomNo` 轉為對應的資料庫欄位名。
- [x] 沙盒能印出完整的 Elastic DSL JSON 指令。
- [x] 全案編譯通過，且不破壞現有的 `ElasticRepository` 邏輯。

## 完成日期
2026-04-27
