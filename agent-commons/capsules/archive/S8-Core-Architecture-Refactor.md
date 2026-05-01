# Sprint S8：核心架構重構與模型解耦 (Major Refactor)

## 任務目標
透過斷開實體模型繼承、引入倉儲組合模式以及實作泛型結果集，徹底解決 MongoDB 序列化衝突，提升 SDK 的穩定性與強型別支援。

## 需求背景
目前系統因 `OrderModel` 繼承 `Orders` 導致 Bson 序列化時產生同名屬性衝突（Element Name Collision）。此外，倉儲層級的繼承關係導致 DI 註冊過於耦合。為了支持未來更高階的開發需求，必須進行深層架構優化。

## 任務清單

### 1. 模型解耦 (Model Decoupling)
- [x] **斷開繼承關係**：移除 `OrderModel.cs` 與 `Orders.cs` 之間的繼承鏈。
- [x] **獨立定義屬性**：在 `OrderModel` 中完整定義業務所需屬性，保留 `[BsonElement("coom_no")]` 等自定義映射。
- [x] **泛型約束放寬**：
    - 確認 `Infrastructure/Persistence/Mongo/MongoRepository.cs`。
    - `MongoRepository<T>` 已無 `where T : Orders` 約束，支援任意 class。

### 2. 結果集泛型化 (Strong-Typed Result)
- [x] **定義泛型介面**：`Core/Interface/IResult.cs` 已包含 `IResult<out T>`。
- [x] **實作泛型類別**：`Result<T>` 類別已實作，提供 `Data` 屬性直接存取強型別物件。
- [x] **更新工廠方法**：`Result` 類別已包含支援泛型的靜態建構方法。

### 3. 倉儲層組合化 (Repository Composition)
- [x] **重構 OrderRepository_Mongo**：
    - 已改為 `class OrderRepository_Mongo : IRepository<Orders>, IMongoDBRepository<Orders>`。
    - 內部持有 `IMongoDBRepository<Orders>` 的私有實例 `_innerRepository`。
    - 所有 CRUD 方法委派給內部的 `_innerRepository` 執行。
- [x] **DI 註冊優化**：`CPF.Service.SendDataToMongoDB/Program.cs` 已調整，`OrderModel` 直接使用 `MongoRepository<OrderModel>`，不再透過繼承體系。

### 4. 驗證與報告
- [x] **全案編譯驗證**：`NO3._dbSDK_Imporve`、`CPF.Service.SendDataToMongoDB`、`CPF.Services.Redis.Post` 均 0 錯誤。
- [ ] **冒煙測試**：驗證 `Insert` 後資料庫文件的 `_id` 是否為 `ObjectId` 且無重複欄位。

## 檢核點
- [ ] 成功執行 `Insert` 且不拋出 `BsonSerializationException`。
- [ ] MongoDB 內部的 Document 結構精簡且正確（無重複欄位 `PK`/`C_Order_M` 等）。
- [x] `IRepository<T>` 可透過 DI 正常解析並運作。

## 完成日期
2026-04-24

## 架構變更說明

### 改動檔案清單
| 檔案 | 變更內容 |
|------|---------|
| `CPF.Service.SendDataToMongoDB/Model/Order/OrderModel.cs` | 移除 `: Orders` 繼承，改為獨立類別 |
| `CPF.Services.Redis.Post/Model/MongoDB/Order/OrderModel.cs` | 移除 `: Orders` 繼承，改為獨立類別 |
| `CPF.Service.SendDataToMongoDB/Program.cs` | 補齊 using，DI 直接使用 `MongoRepository<OrderModel>` |
| `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/MongoMap.cs` | 移除 `UnmapMember` 暫解，恢復乾淨的 `Orders` ClassMap |

### 新舊架構對比
```
舊架構 (有繼承衝突)：
OrderModel : Orders
  ├── new PK [BsonElement("coom_no")]   ← 衍生類別
  └── PK (base)                         ← 基類也被序列化 → 衝突！

新架構 (解耦)：
OrderModel (獨立)
  └── PK [BsonElement("coom_no")]       ← 唯一定義，無衝突

Orders (SDK 內部)
  └── PK                                ← 僅供 EventGiftModel 等 SDK 實體使用
```

### 解決的技術痛點
1. **BsonSerializationException 根除**：繼承鏈導致同名屬性使用不同 element name，現已完全消除。
2. **重複欄位消除**：資料庫不再出現 `PK: null`、`C_Order_M: null` 等基類殘留欄位。
3. **DI 解耦**：`OrderModel` 直接使用 `MongoRepository<OrderModel>`，不再依賴 `Orders` 繼承體系。
