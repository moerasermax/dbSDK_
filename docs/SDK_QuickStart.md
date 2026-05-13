# dbSDK 快速整合指南

本文件協助您在 5 分鐘內完成 dbSDK 的串接。分為「全局初始化」、「查詢服務 (Search)」、「寫入服務 (Write)」與「背景同步服務 (Sync)」四大模組。

---

## 0. 全局初始化 (必做)

在應用程式啟動時 (例如 `builder.Build()` 之前)，必須執行兩階段靜態註冊，確保日期序列化與地圖配置生效：

```csharp
// 建議放在 Program.cs 頂部
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();
```

接著從 `appsettings.json` 讀取連線設定並綁定到 `ConnectionSettings`，後面模組 A / B 都會用到：

```csharp
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = new ConnectionSettings();
configuration.GetSection("ConnectionSettings").Bind(settings);
```

`appsettings.json` 結構（對應 `ConnectionSettings` 類別）：

```json
{
  "ConnectionSettings": {
    "Mongo":   { "Uri": "...", "User": "...", "Password": "..." },
    "Elastic": { "EndPoint": "...", "ApiKey": "..." },
    "Redis":   { "EndPoint": "...", "Port": 6379, "User": "...", "Password": "..." }
  }
}
```

> ⚠️ `appsettings.json` 需在 .csproj 設為「複製到輸出目錄」：
> ```xml
> <ItemGroup>
>   <None Update="appsettings.json">
>     <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
>   </None>
> </ItemGroup>
> ```

---

## 模組 A：訂單查詢 SDK (Search)
**適用於**：Search 1~7 業務查詢 (Seller/Buyer/App Dashboard)

### 1. DI 服務註冊
連線參數一律從 §0 載入的 `settings` 取、不再 hardcode IP：

```csharp
services.AddSingleton(settings);
services.AddSingleton<IElasticOrderSearchService>(sp =>
{
    var s = sp.GetRequiredService<ConnectionSettings>();

    // A1. 配置 ElasticSearch Driver (端點來自 settings.Elastic)
    var esDriver = new ElasticDriver("Elastic", s);
    var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");

    // A2. 配置 MongoDB 查詢層 (連線字串來自 settings.Mongo)
    var mongoConnStr = $"mongodb://{s.Mongo.User}:{s.Mongo.Password}@{s.Mongo.Uri}";
    var mongoClient = new MongoClient(mongoConnStr);
    var db = mongoClient.GetDatabase("CpfOrderDb");
    var mongoDal = new MongoSearchDal(
        db.GetCollection<MongoOrder>("Orders"),
        db.GetCollection<MongoUser>("Users"),
        new MongoMap());

    // A3. 組裝 BLL 與 Service
    var bll = new ElasticOrderSearchBll(new OrderSearchDal(esRepo), mongoDal, null, null);
    return new ElasticOrderSearchService(bll, null);
});
```

### 2. 使用範例
```csharp
// 在 Controller 注入後呼叫
public async Task<IResult<SearchOrderInfoResultModel>> GetOrders(int cid)
{
    var req = new SearchOrderInfoBySellerIdModel { CuamCid = cid };
    return await _sdk.SearchBySellerAsync(req);
}
```

---

## 模組 B：貨態更新服務 (Write)
**適用於**：更新訂單狀態、物流取號、寄貨資訊同步

### 1. DI 服務註冊
連線參數一律從 §0 載入的 `settings` 取、不再 hardcode：

```csharp
services.AddSingleton(settings);
services.AddSingleton<MongoDBDriver>(sp =>
    new MongoDBDriver("MongoDB", sp.GetRequiredService<ConnectionSettings>()));
services.AddSingleton<MongoMap>();
services.AddSingleton<IDTO, DTO>();
services.AddSingleton<IMongoDBRepository<OrderModel>>(sp =>
    new MongoRepository<OrderModel>(
        sp.GetRequiredService<MongoDBDriver>(),
        sp.GetRequiredService<MongoMap>(),
        sp.GetRequiredService<IDTO>(),
        "Order"));
```

### 2. 使用範例 (局部更新範式)
**重點**：`$set` (變更)、`$unset` (移除)、`$push` (追加) 必須在同一次呼叫中完成。

```csharp
public async Task UpdateStatus(string coomNo)
{
    // 1. 建立 Patch Model (僅填入變動欄位，其餘留 null 不會覆寫舊值)
    var patch = new OrderModel {
        PK = coomNo,
        C_Order_M = new C_Order_M_Model { CoomStatus = "30" }
    };

    // 2. 配置複合操作
    var options = new MongoUpdateOptions {
        UnsetFields = new List<string> { "c_order_m.cancel_reason" }, // 移除欄位
        PushFields = new Dictionary<string, BsonValue> {
            ["e_shipment_l"] = new BsonDocument { { "status", "10" } } // 追加歷程
        }
    };

    // 3. 執行 (Filter 可使用字串 JSON)
    await _repo.UpdateData($"{{\"coom_no\": \"{coomNo}\"}}", patch, options);
}
```

---

## 模組 C：背景同步服務 (Sync Services)
**適用於**：把 Redis 上的事件（新增訂單 / 貨態變更 / 付款更新…）非同步落地到 MongoDB 與 ElasticSearch。

### 1. 資料流總覽

```
[Producer]  CPF.Services.Redis.Post / Worker_ForCPF
            ↓ OrderRepository_Redis.InsertData(event)
            ↓
      ┌─────────────────────────────────┐
      │  Redis List (FIFO Queue)        │
      │  ├─ "Request_MongoDB"           │
      │  └─ "Request_Elastic"           │
      └─────────────────────────────────┘
            ↓                       ↓
[Consumer A]                  [Consumer B]
CPF.Service.SendDataToMongoDB CPF.Service.SendDataToElasticCloud
.Worker.ExecuteAsync          .Worker.ExecuteAsync
            ↓                       ↓
IMongoDBRepository            IRepository<OrderSummary>
<OrderModel>.UpdateData       .InsertData / UpdateData
```

Producer 對同一筆事件**同時 Push 兩份**（分別進 `Request_MongoDB` / `Request_Elastic` 兩條 List）；兩個 Consumer 用不同 Key 各自 `ListLeftPopAsync` 拉資料、分頭落地。

### 2. DI 服務註冊
延續 §0 的 `settings`、註冊三件套：`RedisDriver` + `IRepository<Query>`（Redis 事件源）+ 已在模組 A/B 註冊好的 `IMongoDBRepository<OrderModel>` 或 ES Repository。

```csharp
services.AddSingleton(settings);
services.AddSingleton<RedisDriver>(sp =>
    new RedisDriver("Redis", sp.GetRequiredService<ConnectionSettings>()));
services.AddSingleton<IRepository<Query>, OrderRepository_Redis>();
services.AddSingleton<IDTO, DTO>();

// 同時要有模組 B 的 IMongoDBRepository<OrderModel> 或對應 ES Repository
// (見模組 B § 1 的註冊步驟)

services.AddHostedService<MongoSyncWorker>();  // 對應 CPF.Service.SendDataToMongoDB.Worker
services.AddHostedService<ElasticSyncWorker>(); // 對應 CPF.Service.SendDataToElasticCloud.Worker
```

### 3. BackgroundService 樣板

```csharp
public class MongoSyncWorker(
    IRepository<Query> redis,
    IMongoDBRepository<OrderModel> mongoRepo,
    ILogger<MongoSyncWorker> logger) : BackgroundService
{
    private const string Key = "Request_MongoDB";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await redis.GetData(Key) as Result;
            if (result?.DataJson is { } json)
            {
                await HandleAsync(json);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleAsync(string json)
    {
        var query = JsonSerializer.Deserialize<MongoDBAddOrder>(json);
        switch (query?.Name)
        {
            case "AddOrderEvent":
                var patch = MapToOrderModel(query.Args);
                var filter = $"{{\"coom_no\": \"{patch.PK}\"}}";
                await mongoRepo.UpdateData(filter, patch, new MongoUpdateOptions { IsUpsert = true });
                break;
            // 其他事件...
        }
    }
}
```

ES 端 Worker 同模式、只差 `Key = "Request_Elastic"` 與走 `IRepository<OrderSummary>`。

### 4. 試跑

跑 `dotnet run --project CPF.Sandbox -- teaching`、看「模組 C」段；完整模擬 (in-memory Queue、不真連 Redis) 程式碼:`CPF.Sandbox/Scenarios/BackgroundServiceScenario.cs`。

---

## 技術要點備註

1. **安全性**：production 環境建議用 `appsettings.Production.json` 或環境變數（`AddEnvironmentVariables()`）覆寫 `appsettings.json` 中的密碼/ApiKey、原檔僅留佔位符。
2. **自動扁平化**：SDK 內部呼叫 `FlattenBsonDocument`，會自動將 `C_Order_M.CoomStatus` 轉為 Mongo 的 `c_order_m.coom_status` 路徑。
3. **查詢預覽**：若需在 Console 預覽指令，請參考 `CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs`。
