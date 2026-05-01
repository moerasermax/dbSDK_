---
status: USER-RATIFIED
mutability_default: APPEND-ONLY
created_by: user-ratified-from-ai-draft
created_at: 2026-05-01
---

# dbSDK 開發鐵律 (Domain Axioms) - V0.5

> 版本：v0.5 · USER-RATIFIED · 融合架構基礎與 Code Style 規範
> 位階：本文件為本專案最高且唯一不可妥協之領域底線。
> 結構：架構基礎 / Part I 鐵律（不可妥協）/ Part II 紀律（應遵守）/ 開發建議

---

## 架構基礎（Architecture Foundation）

本專案採用 **Clean Architecture + DDD + Design Patterns** 作為統一開發模式。所有程式碼修改、新功能開發、Code Review 均以此為判斷依據。

### 層次結構與依賴方向

```
Core              ──→  零外部依賴（不得引用 MongoDB.Driver、Elastic、Redis、ASP.NET）
  ↑
Infrastructure    ──→  實作 Core 介面（Driver、Repository、Serializer、Mapper）
  ↑
Application       ──→  業務流程、DTO、Sample（組合 Core + Infrastructure）
  ↑
Delivery (Worker) ──→  BackgroundService，依賴介面不依賴具體類別
```

依賴方向只能向上，**禁止反向或跨層**。

### DDD Tactical Patterns 對應

| Pattern | 本專案對應類別 | 說明 |
|---------|--------------|------|
| Entity / Aggregate Root | `Orders`、`OrderSummary` | 具業務主鍵（PK），有生命週期 |
| Value Object | `Condition`、`Result` | 不可變，以工廠方法建立 |
| Repository | `IRepository<T>`、`IMongoDBRepository<T>` | 抽象資料存取，不暴露底層 Driver |
| DTO | `IDTO` | 外部請求轉換為 Core 領域模型的邊界 |

### Design Patterns 對應

| Pattern | 本專案對應實作 | 用途 |
|---------|--------------|------|
| Repository | `IRepository<T>` 系列 | 抽象三種資料庫的 CRUD |
| Factory Method | `Result.SetResult()`、`Result.SetErrorResult()` | 禁止外部 `new Result()`，統一建立方式 |
| Singleton | `MongoMap`、所有 Driver（DI + Double-checked locking）| 確保初始化只跑一次 |
| Facade | `IEngine<T>`（Insert/Update/Read/Remove）| 簡化 Worker 對 Repository 的操作 |
| Decorator | `IMongoDBRepository<T>` 繼承 `IRepository<T>` | 在基本 CRUD 上疊加進階能力 |
| Adapter | `IDTO` | 外部 Query Model → Core Domain Model 的轉換邊界 |
| Strategy | Mongo / Elastic / Redis 三種 `IRepository<T>` 實作 | DI 決定注入哪個，Worker 不感知底層 |
| Builder | `MongoCommandBuilder` | 建構複雜 BsonDocument 指令 |
| Template Method | `dbDriver` 抽象基礎類別 | 定義 Driver 骨架，子類填入具體實作 |
| Value Object | `Condition`、`Result` | 不可變資料結構，保證執行緒安全 |

---

## Part I — 鐵律（不可妥協底線）

違反任一條 → 生產環境立即崩潰、資料永久損壞、或架構根基崩解。

---

### A. ClassMap 靜態初始化完整性

系統啟動時必須確保 BSON ClassMap 與 DateTime Serializer 都已正確註冊。交付專案透過 `new MongoMap()` 注入為 singleton（建構子自動觸發兩階段），Sandbox 則必須在頂部按序呼叫：

1. `MongoSerializationConfig.Register()`
2. `MongoMap.EnsureClassMapsRegistered()`

兩個環境最終生效的 Serializer 必須是同一個（詳見 Part II P1）。

> **後果**：ClassMap 未正確載入 → `Element '_id' does not match any field`，訂單寫入全面失敗。Serializer 不一致 → 測試環境通過但正線邊緣格式爆炸。

> **驗證**：交付專案檢查 `Program.cs` 是否有 `AddSingleton(new MongoMap())`（`new` 關鍵字確保建構子在 DI 容器啟動前立即執行）；Sandbox 檢查 `Program.cs` 頂部兩行順序是否正確。

---

### B. 日期讀寫完整性

系統所有日期必須滿足兩個不可分割的條件：

- **讀取**：任何來源格式（含 CSV「下午」字串、Redis 數字格式 `yyyyMMddHHmmss`）都必須可解析，不得拋出 `FormatException`。
- **寫入**：序列化時必須強制轉換為 UTC 儲存，不得保留 Local 時間。

讀取路徑由 `MultiCultureDateTimeSerializer`（反序列化）負責；寫入路徑由 `MongoCommandBuilder.TryConvertToBsonDateTime()`（Flatten 過程中）負責。兩條路徑都必須覆蓋到。

> **後果**：僅修讀取路徑 → 更新時日期變成字串，查詢失效。序列化不轉 UTC → 時區偏移導致時間欄位查詢結果錯誤。

> **驗證**：確認 `MultiCultureDateTimeSerializer.Serialize()` 有 `ToUniversalTime()` 判斷；確認 `MongoCommandBuilder.Flatten()` 呼叫了 `TryConvertToBsonDateTime()`。

---

### C. 扁平化 $set 更新

MongoDB 的所有更新操作必須預設使用「扁平化 $set」策略。物件在更新前必須經由 `MongoCommandBuilder.Flatten()` 轉換為點符號路徑，且 null 欄位必須被忽略（不覆蓋資料庫舊值）。

> **後果**：直接傳入整層物件 → MongoDB 覆蓋掉資料庫中已存在的其他子欄位，資料**靜默遺失**，無 exception，難追蹤。

> **驗證**：`MongoRepository.cs` 的 `UpdateData` 實作確認呼叫了 `FlattenBsonDocument()`；確認 `MongoCommandBuilder.Flatten()` 中 `IsBsonNull` 時執行 `continue`。

---

### D. 複合操作原子性

當業務操作需要同時執行「更新欄位 + 刪除欄位 + 追加陣列」（$set + $unset + $push）時，**必須使用 `MongoUpdateOptions` 的 `UpdateData` 重載一次執行**，嚴禁拆分為多次獨立呼叫。

> **後果**：拆分呼叫在網路不穩定時產生部分成功，訂單狀態永久錯誤且無法回溯。Race Condition 在高並發下加劇。

> **驗證**：檢查 `Worker.cs` 涉及支付方式變更（`UpdateChangePayTypeEvent`）的邏輯，確認使用了含 `UnsetFields` 的 `MongoUpdateOptions`。

---

### E. 查詢條件不可變性

所有查詢條件（`Core.Models.Condition`、`CRUD_Condition_COOM`、`CRUD_Condition_COOC`）在建構後必須保持不可變。查詢主鍵欄位只能有 `get` 存取子，不得有 `set`。

> **後果**：條件物件在平行處理（S21）中可被修改 → Race Condition，不同執行緒互相干擾查詢範圍，查到錯誤訂單資料。

> **驗證**：`CRUD_Condition_COOM._coom_no`、`CRUD_Condition_COOC.cooc_no`、`Condition._coom_no` 均應無 `set` 存取子。

---

### F. 層次邊界不可越

Clean Architecture 的依賴方向必須嚴格遵守，任何反向或跨層引用都是架構性違反：

- `Core` 層不得引用 `MongoDB.Driver`、`Elastic.Clients`、`StackExchange.Redis`、任何 Infrastructure 具體類別
- `Infrastructure` 層不得引用 `Application` 層
- `Worker`（Delivery）層不得繞過介面直接操作 `IMongoCollection`、`IElasticClient`、`IDatabase`

> **後果**：Core 一旦依賴外部 Driver → 抽象層崩解，無法替換資料庫、無法單元測試；Worker 直接操作 Driver → Strategy Pattern 失效，未來換資料庫需全面重寫。

> **驗證**：檢查 `Core/` 資料夾下的 `.csproj` 不應有 MongoDB.Driver、Elastic、Redis 的 NuGet 引用；Worker 的 using 不應出現 `MongoDB.Bson` 或 `MongoDB.Driver`。

---

## Part II — 紀律（應遵守、有已知缺口）

違反 → 技術債累積、邊緣行為不一致、未來維護成本上升。不會立即崩潰，但必須納入改善計畫。

---

### P1. Sandbox 與交付專案使用相同 Serializer 實作

Sandbox（`MongoSerializationConfig.Register()`）與交付專案（`new MongoMap()`）目前分別生效不同的實作：

| 環境 | 生效的 Serializer | UTC 轉換 | 支援格式數 |
|------|-----------------|---------|-----------|
| Sandbox | `MongoDateTimeSerializer` | ❌ 未判斷 DateTimeKind | 11 個 |
| 交付（MongoDB）| `MultiCultureDateTimeSerializer` | ✅ 正確 | 19 個 |

> **目標**：統一使用 `MultiCultureDateTimeSerializer`，廢棄或對齊 `MongoDateTimeSerializer`。
> **現況**：已知不一致，尚未整合。`MongoSerializationConfig` 是較舊的實作。

---

### P2. 三個日期格式清單保持同步

系統中有三處獨立維護的日期格式清單：
1. `MongoDateTimeSerializer.TryParseMultiCultureDateTime()`（11 個格式）
2. `MultiCultureDateTimeSerializer.SupportedFormats`（19 個格式）
3. `MongoCommandBuilder.TryParseMultiCultureDateTime()`（12 個格式）

新增日期格式支援時，必須同步更新以上三個位置。

> **目標**：將格式清單抽為共用靜態常數，三處引用同一份。
> **現況**：三份獨立，已知不同步。

---

### P3. Worker 不直接依賴資料庫 Driver

交付專案（`CPF.Service.*`）必須透過 `IRepository<T>` 或 `IMongoDBRepository<T>` 介面操作，嚴禁在 Worker 層直接 `using MongoDB.Driver` 並操作 `IMongoCollection`。（對應架構基礎 Strategy Pattern 的正確使用方式）

> **後果**：直接依賴 Driver → 未來切換資料庫時所有 Worker 需大規模重寫。
> **現況**：目前兩個 Worker 均透過介面操作，符合本紀律。

---

### P4. 跨模型轉換責任歸屬（技術債）

跨模型轉換（Redis Query Model → Core OrderModel）應由 `IDTO` 或 `UniversalMapper` 統一處理（對應架構基礎 Adapter Pattern），Worker 不應直接逐欄位手動複製。

目前兩個 Worker 均存在此技術債：
- `CPF.Service.SendDataToMongoDB/Worker.cs`：約 230 行手動 mapping，`IDTO` 注入但未使用
- `CPF.Service.SendDataToElasticCloud/Worker.cs`：約 90 行手動 mapping，`IDTO` 注入但未使用

> **後果**：新增欄位時容易遺漏，多處需同步修改。Core 實體模型與交付本地模型欄位已產生分叉。
> **現況**：已知技術債，列入未來決議事項。

---

### P5. DIP 完整性（Dependency Inversion Principle）

Repository 層應依賴 Driver 的抽象介面（`IdbDriver`），而非具體類別（`MongoDBDriver`）。Driver 在 DI 容器中也應以介面類型登記。

目前違反點：
- `MongoRepository` 建構子參數為 `MongoDBDriver`（具體），應改為 `IdbDriver`
- `Program.cs` 以具體類型 `AddSingleton<MongoDBDriver>` 登記，未同時登記 `IdbDriver`
- `MongoMap` 無對應介面，直接以具體類型注入

> **後果**：無法抽換 Driver 實作（如 Mock Driver 做單元測試）；違反 SOLID D 原則，架構可替換性喪失。
> **現況**：已知缺口，尚未補完。

---

### P6. Code Style 一致性

#### DI 登記規範
- Driver 必須同時登記具體類型與介面（補完 P5 前至少確保具體類型存在）
- Collection Name 必須從 `appsettings.json` 讀取，不得硬編碼在 `Program.cs` 的工廠 lambda 中
- 新服務一律 `AddSingleton`，除非有明確的 scope 需求

#### 命名規範
- 條件類別統一命名格式：`CRUD_Condition_<TABLE_ABBREV>`（如 `CRUD_Condition_COOM`）
- 主鍵欄位統一命名為 `_<table>_no`（如 `_coom_no`、`_cooc_no`，不得混用 `cooc_no`）
- Result 工廠方法：成功用 `SetResult`，失敗用 `SetErrorResult`，禁止 `new Result()`

#### 新 Entity 新增 Checklist
新增實體類別時必須依序完成：
1. 在 `Core/Entity/` 定義 Entity（屬性全部 `{ get; set; }` 可為 null）
2. 在 `Infrastructure/Persistence/Mongo/MongoMap.cs` 的 `EnsureClassMapsRegistered()` 新增 ClassMap
3. 若有 `id` 屬性，必須加 `cm.SetIdMember(null)` 防止 AutoMap 誤判為 `_id`
4. 確認 `SetIgnoreExtraElements(true)` 以防讀取時欄位不符 crash

> **現況**：命名不一致（`CRUD_Condition_COOC` 有程式碼內注解提示改名但尚未執行）。

---

## 開發建議

### UpdateInit 指令預覽

執行複雜更新（含 $unset / $push）前，建議先呼叫 `UpdateInit(condition, data, options)` 預覽生成的 MongoDB 指令 JSON，確認 Filter 與 UpdateDefinition 符合預期後，再替換為 `UpdateData` 正式執行。

```csharp
// 1. 先預覽
var preview = await _mongoRepo.UpdateInit(condition, data, options);
Console.WriteLine(preview.DataJson); // 確認指令正確

// 2. 確認無誤後執行
var result = await _mongoRepo.UpdateData(condition, data, options);
```

> **現況**：`UpdateInit` 已實裝於 `MongoRepository.cs`，並在 `ShippingSyncService.cs` 有使用範例。所有交付 Worker 目前均未呼叫，為已知缺口。
