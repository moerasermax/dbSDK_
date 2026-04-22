# Sprint S2：MongoDB 進階更新與欄位移除 ($unset) 支援

## 任務目標
在 `MongoRepository<T>` 中實作 `UpdateInit` 與帶 Options 的 `UpdateData` 重載，支援透過 `MongoUpdateOptions` 進行動態的 `$unset` (欄位移除) 操作，並維持點符號扁平化的局部更新特性。

## 需求背景
目前 `UpdateData` 預設只支援 `$set` 與 Upsert。由於扁平化工具會忽略 `null` 值以保護舊資料，這導致無法透過傳入實體來「移除」資料庫中的欄位。
在特定業務情境（如支付方式變更為 1 需移除到期日 `cooc_payment_dueday`）下，需要一種機制能動態指定要移除的欄位路徑。

## 任務清單
- [ ] **實作 `UpdateInit`**：
    - 接收 `ConditionData_Json`, `T UpdateData`, 與 `MongoUpdateOptions options`。
    - 調用 `FlattenBsonDocument` 產生點符號格式的 `$set` 內容。
    - 根據 `options.UnsetFields` 產出 `$unset` 指令結構。
    - **核心邏輯**：將 `$set` 與 `$unset` 合併成同一個 MongoDB 更新指令（BsonDocument）。
    - 轉換 `MongoUpdateOptions` 為 MongoDB Driver 的 `FindOneAndUpdateOptions<T>`。
- [ ] **實作 `UpdateData` 重載**：
    - `Task<IResult> UpdateData(string ConditionData_Json, T UpdateData, MongoUpdateOptions options)`。
    - 呼叫 `UpdateInit` 取得指令與 Options 後執行 `FindOneAndUpdateAsync`。
- [ ] **遵循規範**：確保實作符合《開發憲章》第 10 點：MongoDB 進階更新規範。

## 檢核點
| 編號 | 檢核項目 | 驗收標準 |
|------|----------|----------|
| 1 | **$unset 指令生成** | 當 `UnsetFields` 有值時，生成的指令必須包含 `{"$unset": {"欄位路徑": ""}}`。 |
| 2 | **欄位移除實測** | 執行更新後，資料庫中指定的欄位必須徹底消失（而非變為 null）。 |
| 3 | **局部更新完整性** | 其他未被移除的欄位必須正確執行點符號 `$set` 更新，不覆蓋整個巢狀物件。 |
| 4 | **Upsert 預設行為** | 當查無資料且 `IsUpsert=true` 時，必須能成功建立包含 `$set` 內容的新文件。 |
| 5 | **向下相容性** | 原有不帶 Options 的 `UpdateData` 方法邏輯不得有任何變動。 |
| 6 | **交付品質確認** | **Kiro 必須確保程式碼可建置且無 Bug。** |

## 預估影響範圍
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/MongoRepository.cs`

## 技術限制
- 必須維持 `IsUpsert = true` 作為預設行為。
- 欄位路徑必須支援點符號（例如 `c_order_c.cooc_payment_dueday`）。

## 風寫評估
| 風險 | 影響程度 | 因應對策 |
|------|----------|----------|
| 指令合併邏輯錯誤導致更新失敗 | 高 | 嚴格測試 `BsonDocument` 的層級結構，確保 `$set` 與 `$unset` 為同級根鍵。 |
| 傳入 null options 導致崩潰 | 中 | 在 `UpdateInit` 入口處進行 null 檢查並給予預設物件。 |
