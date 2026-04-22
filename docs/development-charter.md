# dbSDK 開發憲章

> 專案：NO3._dbSDK_Imporve  
> 更新日期：2026-04-22  
> 開發模式：單人作業

---

## 1. Git 分支策略

```
main (生產環境)
  │
  └── AI_Dev (開發分支)
        │
        └── feature/* (功能開發)
        └── bugfix/* (錯誤修復)
```

### 分支命名規則

| 類型 | 命名格式 | 範例 |
|------|----------|------|
| 功能開發 | `feature/功能描述` | `feature/elastic-strong-type` |
| 錯誤修復 | `bugfix/問題描述` | `bugfix/mongo-update-async` |
| 重構 | `refactor/重構描述` | `refactor/repository-composition` |
| 緊急修復 | `hotfix/問題描述` | `hotfix/connection-leak` |

### 合併流程

1. `feature/*` → `AI_Dev` (開發完成)
2. `AI_Dev` → `main` (發布)

---

## 2. 架構設計原則

### Clean Architecture 分層

```
┌─────────────────────────────────────────────────────────┐
│                    Infrastructure                        │
│  (Driver, Persistence, External Services)               │
│  ┌───────────────────────────────────────────────────┐  │
│  │                   Application                      │  │
│  │  (Use Cases, Application Services)                │  │
│  │  ┌─────────────────────────────────────────────┐  │  │
│  │  │                  Domain                      │  │  │
│  │  │  (Entities, Value Objects, Domain Events)   │  │  │
│  │  │  (Repository Interfaces, Domain Services)   │  │  │
│  │  └─────────────────────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘

依賴方向：外層 → 內層（只能向內依賴）
```

### 各層職責

| 層級 | 目錄 | 職責 | 允許依賴 |
|------|------|------|----------|
| Domain | `Core/Entity`, `Core/Interface` | 領域模型、倉儲介面、領域服務 | 無 |
| Application | `Application/` | Use Case、應用服務、DTO | Domain |
| Infrastructure | `Infrastructure/` | 資料庫實作、外部服務、Driver | Domain, Application |
| Presentation | `Program.cs`, API | 入口點、DI 組裝 | 全部 |

### DDD 戰術設計

#### Entity（實體）

- 具有唯一識別符
- 生命週期內屬性可變
- 封裝業務邏輯與不變條件

```csharp
// 正確範例
public class Order : IEntity
{
    public string Id { get; private set; }
    public OrderStatus Status { get; private set; }
    
    public void Cancel()  // 業務邏輯封裝在 Entity 內
    {
        if (Status == OrderStatus.Shipped)
            throw new InvalidOperationException("已出貨無法取消");
        Status = OrderStatus.Cancelled;
    }
}
```

#### Value Object（值物件）

- 無唯一識別符，以屬性值判斷相等
- 不可變
- 可自由替換

```csharp
// 正確範例
public class Condition : IValueObject
{
    public string Id { get; }  // 只有 getter，不可變
    
    public Condition(string id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
    }
    
    public override bool Equals(object? obj) => 
        obj is Condition other && Id == other.Id;
    
    public override int GetHashCode() => Id.GetHashCode();
}
```

#### Repository Interface（倉儲介面）

- 定義在 Domain 層
- 只包含聚合根所需的操作
- 不暴露底層技術細節

```csharp
// 正確範例：介面在 Domain 層
public interface IRepository<T> where T : IEntity
{
    Task<IResult> GetByIdAsync(string id);
    Task<IResult> AddAsync(T entity);
    Task<IResult> UpdateAsync(T entity);
    Task<IResult> DeleteAsync(string id);
}

// 錯誤範例：暴露技術細節
public interface IRepository<T>
{
    Task<IResult> GetData(string ConditionData_Json);  // ❌ JSON 是技術細節
}
```

#### Aggregate Root（聚合根）

- 聚合的入口點
- 外部只能透過聚合根存取聚合內物件
- 確保聚合內的一致性

```csharp
// Order 是聚合根，OrderItem 是聚合內物件
public class Order : IAggregateRoot
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(OrderItem item)
    {
        // 業務規則：同一商品最多 10 件
        if (_items.Count(x => x.ProductId == item.ProductId) >= 10)
            throw new DomainException("同一商品最多 10 件");
        _items.Add(item);
    }
}
```

### 依賴反轉原則（DIP）

```
┌──────────────────┐     ┌──────────────────┐
│   Application    │     │  Infrastructure  │
│                  │     │                  │
│  DbSDKEngine<T>  │     │  MongoRepository │
│        │         │     │        │         │
│        ▼         │     │        ▼         │
│  IRepository<T> ◄├─────┤implements        │
│  (介面定義)      │     │  (具體實作)      │
└──────────────────┘     └──────────────────┘

高層模組不依賴低層模組，兩者皆依賴抽象
```

### 禁止事項

- ❌ Domain 層引用任何外部套件（MongoDB.Driver, Elastic.Clients 等）
- ❌ Domain 層定義技術相關介面（如 `IMongoRepository`）
- ❌ Application 層直接實例化 Infrastructure 類別
- ❌ Entity 暴露 public setter，應透過方法封裝業務邏輯
- ❌ Value Object 有可變狀態
- ❌ Repository 介面暴露 SQL、JSON 等技術細節

### 強制事項

- ✅ 所有領域邏輯封裝在 Entity 或 Domain Service
- ✅ Repository 介面定義在 Domain 層，實作在 Infrastructure 層
- ✅ Use Case（Application Service）協調領域物件完成業務流程
- ✅ 透過 DI 注入依賴，禁止 `new` 具體實作
- ✅ 聚合根確保聚合內一致性邊界

---

## 3. 程式碼規範

### 分層依賴原則

```
Core ← Application ← Infrastructure
  ↑         ↑            ↑
  │         │            │
  └─────────┴────────────┘
       只能依賴 Core 介面
```

### 禁止事項

- ❌ Application 層直接引用 Infrastructure 具體類別
- ❌ Core 層引用任何外部 NuGet 套件
- ❌ Repository 拋出 Exception，必須回傳 `IResult`
- ❌ 硬編碼連線字串或憑證

### 強制事項

- ✅ 所有公開 API 必須透過介面
- ✅ 錯誤處理統一使用 `Result.SetErrorResult()`
- ✅ 非同步方法需支援 `CancellationToken`
- ✅ 敏感資料使用環境變數

---

## 4. 命名慣例

| 類型 | 規則 | 範例 |
|------|------|------|
| 介面 | `I + 名詞` | `IRepository<T>`, `IEngine<T>` |
| 實作類別 | `介面名稱去掉I` | `Repository<T>`, `Engine<T>` |
| Repository | `實體名 + Repository` | `OrderRepository` |
| Driver | `資料庫名 + Driver` | `MongoDBDriver` |
| Model | `名詞 + Model` | `OrderInfoModel` |
| 私有欄位 | `_ + camelCase` | `_client`, `_indexName` |
| Entity | `名詞` | `Order`, `EventGift` |
| Value Object | `名詞` | `Condition`, `Money`, `Address` |
| Domain Service | `名詞 + Service` | `OrderPricingService` |
| Application Service | `動詞 + UseCase` | `CreateOrderUseCase` |

---

## 5. 提交訊息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type 類型

| 類型 | 說明 |
|------|------|
| `feat` | 新功能 |
| `fix` | 錯誤修復 |
| `refactor` | 重構（不影響功能） |
| `docs` | 文件更新 |
| `test` | 測試相關 |
| `chore` | 建置、套件更新 |

### 範例

```
feat(elastic): add strong-typed filter support

- Add Expression-based field name resolution
- Support generic type constraint
- Deprecate magic string usage
```

---

## 6. 程式碼審查清單

提交前自我檢查：

- [ ] 是否符合 Clean Architecture 分層原則
- [ ] Domain 層是否零外部依賴
- [ ] Entity 是否封裝業務邏輯
- [ ] Value Object 是否不可變
- [ ] Repository 介面是否定義在 Domain 層
- [ ] 是否有硬編碼的敏感資料
- [ ] 錯誤處理是否使用 `IResult`
- [ ] 非同步方法是否有 `await`
- [ ] 是否有適當的註解與文件

---

## 7. 環境配置

### 必要環境變數

```
MONGO_URI=
MONGO_USER=
MONGO_PASSWORD=
ELASTIC_ENDPOINT=
ELASTIC_API_KEY=
REDIS_ENDPOINT=
REDIS_PORT=
REDIS_USER=
REDIS_PASSWORD=
```

### 本機開發

- 使用 `appsettings.Development.json` 覆蓋預設值
- 禁止將真實憑證提交至版本控制

---

## 8. 發布流程

1. **交付前確認**：Kiro 必須確保程式碼可建置（Build Success）且無已知 Bug，始得進行交付。
2. 確保 `AI_Dev` 所有測試通過
3. 合併至 `main`
4. 打上版本標籤 `v1.x.x`
5. 更新 `docs/refactor_history.md`

---

### 10. MongoDB 進階更新規範

為了支援局部更新與動態結構調整，MongoDB 的更新必須遵循以下原則：

#### 局部更新 ($set) 與扁平化
- **強制原則**：更新必須使用 `FlattenBsonDocument` 工具，將巢狀物件轉換為「點符號路徑」（例如 `c_order_c.status`）。
- **忽略 Null**：為了防止無意中覆蓋舊資料，扁平化過程必須忽略 `null` 值。

#### 欄位移除 ($unset)
- **觸發條件**：當業務邏輯需要「徹底移除」資料庫中的某個欄位時（例如支付方式變更導致特定欄位失效），必須透過 `MongoUpdateOptions.UnsetFields` 傳遞路徑。
- **指令格式**：SDK 必須生成 `{"$unset": {"欄位路徑": ""}}` 指令。

#### 指令合併邏輯
- **並行執行**：在 `UpdateInit` 中，必須將生成的 `$set` 內容與 `$unset` 內容合併成同一個 `BsonDocument` 指令，確保資料庫操作的原子性。
- **範例指令**：
  ```json
  {
    "$set": { "c_order_c.payment_type": "1" },
    "$unset": { "c_order_c.payment_dueday": "" }
  }
  ```

---

## 11. 待辦事項優先級

| 優先級 | 項目 | 狀態 |
|--------|------|------|
| P0 | DI 改用 IHost Builder | 待處理 |
| P0 | Repository 改組合模式 | 待處理 |
| P0 | IResult 泛型化 | 待處理 |
| P1 | ElasticFilter 強型別 | 待處理 |
| P1 | 憑證環境變數化 | 待處理 |
| P1 | CancellationToken 支援 | 待處理 |
