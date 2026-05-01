# Sprint H1：MongoDB 資料強健性優化 (日期格式正規化)

## 任務目標
強化 `MongoRepository` 的寫入引擎與讀取序列化邏輯，自動識別各種日期格式（包含中文語系字串）並正規化為 MongoDB 原生 `BsonDateTime` 類型，從根本解決 `System.FormatException`。

## 需求背景
目前資料庫中存在大量以「本地字串」存儲的日期資料（如 `2026/4/13 下午 01:46:15`）。
這導致：
1. **讀取崩潰**：標準 .NET DateTime 反序列化器無法識別「下午」等字眼。
2. **資料庫髒化**：資料以 String 存儲而非 Date，導致無法進行高效的日期區間查詢。
3. **SDK 脆弱性**：SDK 缺乏對非標準輸入的容錯與正規化能力。

## 任務清單
- [ ] **優化 `FlattenBsonDocument` 工具**：
    - 在扁平化處理時，偵測欄位值是否為 `DateTime` 或符合日期格式的 `String`。
    - **自動轉型**：強制將識別出的日期內容封裝為 `MongoDB.Bson.BsonDateTime`。
- [ ] **實作 `DateTime` 智能解析器**：
    - 支援多種語系格式解析（包含 `yyyy/MM/dd tt hh:mm:ss` 這種帶有上下午標記的格式）。
- [ ] **全域註冊容錯序列化器**：
    - 在 SDK 初始化時註冊 `BsonSerializer.RegisterSerializer(new DateTimeSerializer(...))`，確保讀取舊有髒資料時不報錯。
- [ ] **相容性測試**：
    - 確保 `DateTime?` (Nullable) 類型在 Null 時依然能正確觸發 `[BsonIgnoreIfNull]`。

## 檢核點
| 編號 | 檢核項目 | 驗收標準 |
|------|----------|----------|
| 1 | **日期自動正規化** | 透過 SDK 寫入 `2026/4/13 下午 01:46:15` 字串後，在 MongoDB Compass 查看該欄位型別必須為 `Date`。 |
| 2 | **舊資料讀取成功** | 讀取資料庫中現有的「下午...」格式字串時，SDK 不再拋出 `FormatException`，且能正確解析為 DateTime。 |
| 3 | **點符號更新一致性** | 正規化邏輯不得破壞 `$set` 的點符號路徑（例如 `c_order_m.coom_create_datetime`）。 |
| 4 | **交付品質確認** | **Kiro 必須確保程式碼可建置且無 Bug。** |

## 預估影響範圍
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/MongoRepository.cs`
- `NO3._dbSDK_Imporve/Program.cs` (全域序列化器註冊)

## 風險評估
| 風險 | 影響程度 | 因應對策 |
|------|----------|----------|
| 誤判非日期字串為日期 | 中 | 使用嚴格的 `DateTime.TryParse` 並配合特定格式清單。 |
| 全域序列化器衝突 | 低 | 檢查是否已有其他套件註冊過序列化器，避免重複註冊。 |
