# dbSDK 快速整合指南

本文件協助您在 5 分鐘內完成 dbSDK 的串接。分為「全局初始化」、「查詢服務 (Search)」、「寫入服務 (Write)」、「雙引擎查詢模式」、「MongoDB 局部更新指南」五大段。

> **S45 升級重點**:DI 註冊從散亂 `AddSingleton(...)` 改為標準擴充方法 `services.AddDbSdk(IConfiguration)` + `IOptions<ConnectionSettings>` 模式。下游程式碼用 `IOptions<ConnectionSettings>` 注入、不再直接吃 `ConnectionSettings` 實體。

---

## 0. 全局初始化 (必做)

### 0.1 兩階段靜態註冊

在應用程式啟動時 (例如 `builder.Build()` 之前),必須執行兩階段靜態註冊、確保日期序列化與 ClassMap 配置生效:

```csharp
// 建議放在 Program.cs 頂部
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();
```

### 0.2 載入 Configuration (含環境變數覆蓋)

接著建立 `IConfiguration`、含 `appsettings.json` + 環境變數 `DBSDK_` 前綴覆蓋。模組 A 與 B 都會共用同一個 `IConfiguration`:

```csharp
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .AddEnvironmentVariables(prefix: "DBSDK_")   // 環境變數可覆蓋 appsettings 設定
    .Build();
```

### 0.3 `appsettings.json` 結構

對應 `ConnectionSettings` 類別:

```json
{
  "ConnectionSettings": {
    "Mongo":   { "Uri": "...", "User": "...", "Password": "..." },
    "Elastic": { "EndPoint": "...", "ApiKey": "..." },
    "Redis":   { "EndPoint": "...", "Port": 6379, "User": "...", "Password": "..." }
  }
}
```

> ⚠️ `appsettings.json` 需在 .csproj 設為「複製到輸出目錄」:
> ```xml
> <ItemGroup>
>   <None Update="appsettings.json">
>     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
>   </None>
> </ItemGroup>
> ```

### 0.4 環境變數覆蓋範例

部署時可用環境變數覆蓋敏感欄位、不需動 `appsettings.json`:

```powershell
# PowerShell
$env:DBSDK_ConnectionSettings__Mongo__Uri      = "cluster0.prod.mongodb.net/CpfOrderDb"
$env:DBSDK_ConnectionSettings__Mongo__User     = "prod_user"
$env:DBSDK_ConnectionSettings__Mongo__Password = "***"
```

```bash
# Bash / CI
export DBSDK_ConnectionSettings__Mongo__Uri="cluster0.prod.mongodb.net/CpfOrderDb"
export DBSDK_ConnectionSettings__Mongo__User="prod_user"
export DBSDK_ConnectionSettings__Mongo__Password="***"
```

> 雙底線 `__` 是 .NET 對 JSON 階層的標準對應(`__` ↔ `:`)、不要寫成 `_`。

---

## 模組 A:訂單查詢 SDK (Search)
**適用於**:Search 1~7 業務查詢 (Seller / Buyer / App Dashboard)

### A.1 DI 服務註冊 (S45 新模式)

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

services.AddDbSdk(configuration);                                       // SDK 基礎元件(Drivers + Maps + IDTO + IOptions<ConnectionSettings>)
services.AddSingleton<IElasticOrderSearchService>(BuildSearchService);   // App-specific:Search BLL stack
```

`BuildSearchService` factory(從 DI 容器取 `IOptions<ConnectionSettings>` + `ElasticDriver`):

```csharp
private static IElasticOrderSearchService BuildSearchService(IServiceProvider sp)
{
    var settings = sp.GetRequiredService<IOptions<ConnectionSettings>>().Value;
    var esDriver = sp.GetRequiredService<ElasticDriver>();

    // ES 端 stack
    var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");
    var esDal = new OrderSearchDal(esRepo);

    // Mongo 端 stack (Dual Engine — Search 2/3/7 補資料)
    var mongoConnStr = $"mongodb://{settings.Mongo.User}:{settings.Mongo.Password}@{settings.Mongo.Uri}";
    var db = new MongoClient(mongoConnStr).GetDatabase("CpfOrderDb");
    var mongoDal = new MongoSearchDal(
        db.GetCollection<MongoOrder>("Orders"),
        db.GetCollection<MongoUser>("Users"),
        new MongoMap());

    // BLL + Service
    var bll = new ElasticOrderSearchBll(esDal, mongoDal, null, null);
    return new ElasticOrderSearchService(bll, null);
}
```

### A.2 使用範例

```csharp
// 在 Controller 注入後呼叫
public async Task<IResult<SearchOrderInfoResultModel>> GetOrders(int cid)
{
    var req = new SearchOrderInfoBySellerIdModel { CuamCid = cid };
    return await _sdk.SearchBySellerAsync(req);
}
```

---

## 模組 B:貨態更新服務 (Write)
**適用於**:更新訂單狀態、物流取號、寄貨資訊同步

### B.1 DI 服務註冊 (S45 新模式)

S45 提供 `AddDbSdkMongoRepository<TModel>(collectionName)` 擴充、把「entity ↔ collection 名稱綁定」這條 boilerplate 收進去:

```csharp
using Microsoft.Extensions.DependencyInjection;

services.AddDbSdk(configuration);                          // SDK 基礎元件
services.AddDbSdkMongoRepository<OrderModel>("Order");    // entity-specific 註冊
```

### B.2 使用範例 (局部更新範式)

**重點**:`$set` (變更)、`$unset` (移除)、`$push` (追加) 必須在同一次呼叫中完成 — 對齊 DBSDK Part I §D 原子性。

```csharp
public async Task UpdateStatus(string coomNo)
{
    // 1. 建立 Patch Model (僅填入變動欄位、其餘留 null 不會覆寫舊值)
    var patch = new OrderModel {
        PK = coomNo,
        C_Order_M = new C_Order_M_Model { CoomStatus = "30" }
    };

    // 2. 配置複合操作
    var options = new MongoUpdateOptions {
        UnsetFields = new List<string> { "c_order_m.coom_cancel_reason" },  // 移除欄位
        PushFields = new Dictionary<string, BsonValue> {
            ["e_shipment_l"] = new BsonDocument { { "esml_esmm_status", "10" } }   // 追加歷程
        }
    };

    // 3. 執行 (Filter 可使用字串 JSON)
    await _repo.UpdateData($"{{\"coom_no\": \"{coomNo}\"}}", patch, options);
}
```

---

## 雙引擎 (Dual Engine) 查詢模式

dbSDK 的 Search 1-7 採 ES + MongoDB 雙引擎協作:

| Search 編號 | 主引擎 | 副引擎 | 資料流向 |
|---|---|---|---|
| **Search 1** GetHomeToDoOverview     | ES         | —       | 全 ES 聚合、無 Mongo |
| **Search 2** SearchBySellerId        | ES (主查詢)| Mongo (補欄位) | ES 找 hit → Mongo 補商品中繼資料 |
| **Search 3** SearchByBuyerId         | ES (主查詢)| Mongo (補欄位) | 同 Search 2 |
| **Search 4** GetAppDashboardOverview | ES         | —       | 全 ES 聚合 |
| **Search 5** GetAppSalesToday        | ES         | —       | 全 ES 聚合(hour bucket) |
| **Search 6** GetAppSalesWeek         | ES         | —       | 全 ES 聚合(day bucket) |
| **Search 7** GetUserCgdmData         | —          | Mongo (主)    | 直查 Mongo Users collection |

**設計理由**:
- ES 適合「跨大量訂單做聚合 / 篩選 / 統計」(Dashboard / 趨勢圖 / 條件搜尋)
- Mongo 適合「按主鍵單筆撈取明細 + 跨 collection JOIN 補資料」(User / GoodsMaster)
- 兩者透過 `ElasticOrderSearchBll` 統一介面組合、caller 無需感知

**對外介面鎖定**:`IElasticOrderSearchService` 介面簽章不隨引擎切換變動、變更僅在 BLL 內部實作。

---

## MongoDB 局部更新指南 ($set / $unset / $push)

### 概念對應

| 業務操作 | MongoDB 算子 | dbSDK 表達方式 |
|---|---|---|
| **變更欄位** | `$set`   | OrderModel patch 內非 null 欄位 → 自動 Flatten 為點符號路徑 |
| **移除欄位** | `$unset` | `MongoUpdateOptions.UnsetFields` 列點符號路徑清單 |
| **追加陣列元素** | `$push` | `MongoUpdateOptions.PushFields` 提供 key + BsonValue |

### 範例:$set + $unset 複合操作

業務情境:訂單從「待支付」變為「已支付」、同時清除已過期的「逾期提醒」欄位:

```csharp
var patch = new OrderModel {
    PK = "CM2604160395986",
    C_Order_M = new C_Order_M_Model {
        CoomStatus = "20",                                         // $set 狀態
        CoomSellerMemo = $"付款已確認 {DateTime.UtcNow:yyyy-MM-dd}"  // $set 備註
        // 其他欄位留 null、Flatten 會跳過、不覆蓋舊值(DBSDK Part I §C)
    }
};

var options = new MongoUpdateOptions {
    IsUpsert = false,
    UnsetFields = new List<string> {
        "c_order_m.coom_payment_due_reminder"   // $unset 過期欄位
    }
};

var result = await _repo.UpdateData(
    "{ \"coom_no\": \"CM2604160395986\" }",
    patch,
    options);
```

### Flatten 紀律 (DBSDK Part I §C)

- null 欄位**必定**被忽略、不會覆蓋舊值
- 巢狀 BsonDocument 自動轉點符號路徑(`C_Order_M.CoomStatus` → `c_order_m.coom_status`)
- 詳細範例見 `CPF.Sandbox/Scenarios/AdvancedUpdateScenario.cs`(S42)

### 完整可跑範例

```bash
# 在 dev 環境跑 dry-run 預覽 (placeholder 模式自動偵測、不需真實 Mongo)
dotnet run --project CPF.Sandbox -- advanced-update

# 完整教學流程 (§0 → 模組 A → 模組 B)
dotnet run --project CPF.Sandbox -- teaching
```

互動式選單 (`dotnet run --project CPF.Sandbox` 無參數)按鍵:
- `T` — QuickStart 教學
- `U` — 進階更新範例 ($set + $unset 複合操作)

---

## 技術要點備註

1. **安全性**:production 環境建議用環境變數(`AddEnvironmentVariables(prefix: "DBSDK_")`)覆寫 `appsettings.json` 中的密碼 / ApiKey、原檔僅留佔位符。
2. **自動扁平化**:SDK 內部呼叫 `FlattenBsonDocument`、自動將 `C_Order_M.CoomStatus` 轉為 Mongo 的 `c_order_m.coom_status` 路徑。
3. **複合操作原子性**:$set + $unset + $push 必須在同一次 `UpdateData` 呼叫合併執行、嚴禁拆分為多次 SDK 呼叫(DBSDK Part I §D)。
4. **查詢預覽**:若需在 Console 預覽 BSON 指令、請參考 `CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs` 與 `AdvancedUpdateScenario.cs`。
5. **IOptions 模式**:SDK 內部走 `IOptions<ConnectionSettings>` 注入、caller 用 `services.AddDbSdk(configuration)` 一次完成綁定;若需手動取 `ConnectionSettings`、用 `sp.GetRequiredService<IOptions<ConnectionSettings>>().Value`。
