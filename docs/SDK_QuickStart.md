# dbSDK 快速整合指南

本文件協助您在 5 分鐘內完成 dbSDK 的串接。分為「全局初始化」、「查詢服務 (Search)」與「寫入服務 (Write)」三大模組。

---

## 0. 全局初始化 (必做)

在應用程式啟動時 (例如 `builder.Build()` 之前)，必須執行兩階段靜態註冊，確保日期序列化與地圖配置生效：

```csharp
// 建議放在 Program.cs 頂部
MongoSerializationConfig.Register();
MongoMap.EnsureClassMapsRegistered();
```

---

## 模組 A：訂單查詢 SDK (Search)
**適用於**：Search 1~7 業務查詢 (Seller/Buyer/App Dashboard)

### 1. DI 服務註冊
```csharp
services.AddSingleton<IElasticOrderSearchService>(sp =>
{
    // A1. 配置 ElasticSearch Driver
    var conn = new ConnectionSettings { 
        Elastic = new DbDetail { EndPoint = "http://localhost:9200" } 
    };
    var esDriver = new ElasticDriver("Elastic", conn);
    var esRepo = new ElasticRepository<OrderDocument>(esDriver, new ElasticMap(), "orders-*");
    
    // A2. 配置 MongoDB 查詢層 (SearchDal)
    var mongoClient = new MongoClient("mongodb://root:example@localhost:27017");
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
```csharp
services.AddSingleton<IMongoDBRepository<OrderModel>>(sp =>
{
    var driver = new MongoDBDriver("MongoDB", new ConnectionSettings {
        Mongo = new DbDetail { Uri = "mongodb://..." }
    });
    return new MongoRepository<OrderModel>(driver, new MongoMap(), new DTO(), "Order");
});
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

## 技術要點備註

1. **安全性**：`ConnectionSettings` 建議從環境變數讀取。
2. **自動扁平化**：SDK 內部呼叫 `FlattenBsonDocument`，會自動將 `C_Order_M.CoomStatus` 轉為 Mongo 的 `c_order_m.coom_status` 路徑。
3. **查詢預覽**：若需在 Console 預覽指令，請參考 `CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs`。
