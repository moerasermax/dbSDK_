# dbSDK QuickStart

5 分鐘接入指南。客戶端工程師看完即可整合。

---

## 環境

- .NET 8.0
- MongoDB（生產:Atlas SRV / dev:localhost:27017）
- ElasticSearch 8.x

---

## 安裝

Project Reference 加三個:

- `NO3._dbSDK_Imporve`
- `PIC.CPF.OrderSDK.Biz.Read.Elastic`
- `CPF.Service.SendDataToMongoDB`

NuGet:

- `MongoDB.Driver`
- `Microsoft.Extensions.DependencyInjection`

---

## Step 1 — Program.cs 啟動初始化

在 `builder.Build()` 之前加兩行:

```csharp
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();
```

> 整個 process 跑一次。重複呼叫安全(內部有 lock + flag)。

---

## Step 2 — 註冊查詢服務

```csharp
services.AddSingleton<IElasticOrderSearchService>(sp =>
{
    var conn = new ConnectionSettings
    {
        Elastic = new DbDetail { EndPoint = "http://localhost:9200" }
    };
    var esDriver = new ElasticDriver("Elastic", conn);
    var esMap = new ElasticMap();
    var esRepo = new ElasticRepository<OrderDocument>(esDriver, esMap, "orders-*");
    var esDal = new OrderSearchDal(esRepo);

    var mongoMap = new MongoMap();
    var mongoClient = new MongoClient("mongodb://root:example@localhost:27017");
    var db = mongoClient.GetDatabase("CpfOrderDb");
    var orderColl = db.GetCollection<MongoOrder>("Orders");
    var userColl = db.GetCollection<MongoUser>("Users");
    var mongoDal = new MongoSearchDal(orderColl, userColl, mongoMap);

    var bll = new ElasticOrderSearchBll(esDal, mongoDal, null, null);
    return new ElasticOrderSearchService(bll, null);
});
```

### using 清單

```csharp
using MongoDB.Driver;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Elastic;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.BLL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.DAL;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Models.Internal;
using PIC.CPF.OrderSDK.Biz.Read.Elastic.Services;
```

---

## Step 3 — 在 Controller 注入並呼叫查詢

```csharp
public class OrderController(IElasticOrderSearchService sdk)
{
    [HttpPost("order/buyer")]
    public Task<IResult<SearchOrderInfoResultModel>> SearchByBuyer(
        SearchOrderInfoByBuyerIdModel m) => sdk.SearchByBuyerAsync(m);
}
```

### 對外 7 個 method

| Method | 用途 |
|---|---|
| `GetHomeToDoOverViewAsync` | Search 1 首頁待辦總覽 |
| `SearchBySellerAsync` | Search 2 賣家訂單搜尋 |
| `SearchByBuyerAsync` | Search 3 買家訂單搜尋 |
| `GetAppDashboardAsync` | Search 4 App 儀表板 |
| `GetAppSalesTodayAsync` | Search 5 App 銷售指標(本日) |
| `GetAppSalesWeekAsync` | Search 6 App 銷售指標(本週) |
| `GetUserCgdmDataAsync` | Search 7 取賣家 cgdm 資料 |

---

## Step 4 — 註冊 Mongo 寫入 Repository

```csharp
services.AddSingleton<MongoDBDriver>(_ =>
{
    var settings = new ConnectionSettings
    {
        Mongo = new DbDetail
        {
            User = "root",
            Password = "example",
            Uri = "cluster0.example.mongodb.net/CpfOrderDb"
        }
    };
    return new MongoDBDriver("MongoDB", settings);
});
services.AddSingleton<MongoMap>();
services.AddSingleton<IDTO, DTO>();
services.AddSingleton<IMongoDBRepository<OrderModel>>(sp =>
    new MongoRepository<OrderModel>(
        sp.GetRequiredService<MongoDBDriver>(),
        sp.GetRequiredService<MongoMap>(),
        sp.GetRequiredService<IDTO>(),
        "Order"));
```

### using 清單

```csharp
using CPF.Service.SendDataToMongoDB.Model.Order;
using NO3._dbSDK_Imporve.Core.Interface;
using NO3._dbSDK_Imporve.Core.Models;
using NO3._dbSDK_Imporve.Infrastructure.Driver;
using NO3._dbSDK_Imporve.Infrastructure.DTO;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Interfaces;
using NO3._dbSDK_Imporve.Infrastructure.Persistence.Mongo.Models;
```

> **MongoDBDriver** 預設組 `mongodb+srv://` 連線串、需 Atlas SRV DNS。
> dev 環境 localhost 請改直接注入 `IMongoCollection<OrderModel>` 並用 `new MongoClient("mongodb://...")` 繞過 driver。

---

## Step 5 — 注入並執行更新

`$set` + `$unset` + `$push` 必在**同一次** `UpdateData` 呼叫:

```csharp
public class ShippingWorker(IMongoDBRepository<OrderModel> repo)
{
    public async Task MarkShippedAsync(string coomNo)
    {
        string filter = $"{{ \"coom_no\": \"{coomNo}\" }}";

        var patch = new OrderModel
        {
            PK = coomNo,
            C_Order_M = new C_Order_M_Model
            {
                CoomStatus = "30",
                CoomSellerMemo = "已寄出"
            }
        };

        var options = new MongoUpdateOptions
        {
            IsUpsert = false,
            UnsetFields = new List<string>
            {
                "c_order_m.coom_cancel_reason"
            },
            PushFields = new Dictionary<string, BsonValue>
            {
                ["e_shipment_l"] = new BsonDocument
                {
                    ["esml_esmm_status"] = "10",
                    ["esml_status_datetime"] = DateTime.UtcNow
                }
            }
        };

        var result = await repo.UpdateData(filter, patch, options);
        if (!result.IsSuccess) throw new Exception(result.Msg);
    }
}
```

### 加入 using

```csharp
using MongoDB.Bson;
```

---

## 三條重點

| 重點 | 說明 |
|---|---|
| 1. **三種 update 必合併** | `$set` / `$unset` / `$push` 必須一次 `UpdateData` 呼叫;拆三次會破壞原子性 |
| 2. **null 欄位自動忽略** | `patch` model 沒填的欄位不會覆寫舊值(局部更新、無破壞性) |
| 3. **nested 路徑自動扁平化** | `C_Order_M.CoomStatus` 會自動展為 `c_order_m.coom_status`(`[BsonElement]` 控制名稱) |

---

## 預覽指令(dev 偵錯)

想看實際送 Mongo 的 BSON 指令、不執行 I/O:

```csharp
var rawPatch = patch.ToBsonDocument();
var flatSet = MongoRepository<OrderModel>.FlattenBsonDocument(rawPatch);

var cmd = new BsonDocument { ["$set"] = flatSet };
if (options.UnsetFields?.Count > 0)
{
    var unset = new BsonDocument();
    foreach (var f in options.UnsetFields) unset.Add(f, "");
    cmd["$unset"] = unset;
}
if (options.PushFields?.Count > 0)
{
    var push = new BsonDocument();
    foreach (var p in options.PushFields)
        push.Add(p.Key, new BsonDocument("$each", new BsonArray { p.Value }));
    cmd["$push"] = push;
}

Console.WriteLine(cmd.ToJson(new JsonWriterSettings { Indent = true }));
```

輸出範例:

```json
{
  "$set" : {
    "coom_no" : "CM2604160395986",
    "c_order_m.coom_status" : "30",
    "c_order_m.coom_seller_memo" : "已寄出"
  },
  "$unset" : {
    "c_order_m.coom_cancel_reason" : ""
  },
  "$push" : {
    "e_shipment_l" : {
      "$each" : [{
          "esml_esmm_status" : "10",
          "esml_status_datetime" : { "$date" : "2026-05-13T06:17:33.363Z" }
        }]
    }
  }
}
```

---

## 試跑

在 `CPF.Sandbox` 跑互動選單按字母 `T`、或 CMD:

```bash
dotnet run --project CPF.Sandbox -- teaching
```

對應原始碼:`CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs`
