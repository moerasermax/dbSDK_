# 專案狀態 Checkpoint — dbSDK 重構歷程（Gemini 用）

## 專案概述
- 名稱：NO3._dbSDK_Imporve（.NET 8）
- 目標：建立支援 MongoDB / Elasticsearch / Redis 的通用資料庫 SDK
- 目前解耦評分：7 / 10
- 溝通語言：繁體中文，程式碼英文命名

---

## 目前目錄結構

```
Application/
  dbSDKEngine.cs                    ← Use Case 協調層，只依賴 Core 介面
  Sample/
    Mongo/   IOrderRepository_Mongo / OrderRepository_Mongo
    Elastic/ IOrderRepository_Elastic / OrderRepository_Elastic
    Redis/   IOrderRepository_Redis / OrderRepository_Redis

Core/
  Abstraction/
    dbDriver.cs                     ← abstract class，實作 IdbDriver，_Service 只有 getter
    BaseRandomDataGenerator.cs      ← 隨機工具基底（GetRandomFrom / NextInt）
  DTO/
    DTO.cs                          ← 實作 IDTO，產生 Condition 物件
    EventGiftRandomDataGenerator.cs ← 注入 IUniversalMapper，產生測試資料
  Entity/
    EventGiftModel.cs               ← 繼承 Order
    EventGiftSummaryModel.cs        ← 繼承 OrderSummary
    Query.cs
  Interface/
    IEngine<T>    ← Task<IResult> Insert/Update/Read/Remove
    IRepository<T>← Task<IResult> insertData/updateData/getData/removeData
    IResult       ← bool IsSuccess / string Msg / string DataJson
    IdbDriver     ← string _Service
    IDTO          ← Condition getCondition(string id)
    IUniversalMapper ← TDestination Map<TSource,TDestination>(TSource source)
    IRandamDataGenerator<T,T1> ← Generate / Generate(count) / ToSummary
  Models/
    Result.cs            ← public, immutable, private ctor, static factory
    ConnectionSettings.cs← Mongo / Elastic / Redis 各一個 DbDetail
    Condition.cs         ← immutable，只有 _id getter

Infrastructure/
  Driver/
    ElasticDriver.cs     ← 接收 ConnectionSettings，建立 ElasticsearchClient
    MongoDBDriver.cs     ← 接收 ConnectionSettings，建立 MongoClient
    RedisDriver.cs       ← 接收 ConnectionSettings，建立 ConnectionMultiplexer
  MAP/
    UniversalMapper.cs   ← AutoMapper 封裝，ConcurrentDictionary 快取 IMapper
                           內含 NullLoggerFactory / NullLogger（AutoMapper 16.1.1 需要）
  Persistence/
    Elastic/ ElasticRepository<T> / ElasticMap / ElasticFilter
    Mongo/   MongoRepository<T> / MongoMap
    Redis/   RedisRepository<T> / RedisMap
```

---

## 核心設計決策

### Result 模式
```csharp
// immutable value object，不可繼承修改
public class Result : IResult {
    private Result(bool isSuccess, string msg, string dataJson) { ... }
    public static Result setResult(string msg, bool isSuccess = true) => new(...);
    public static Result setResult(string msg, string DataJson, bool isSuccess = true) => new(...);
    public static Result setErrorResult(string MethodName, string msg) => new(...);
}
```

### IEngine<T> 合約
```csharp
public interface IEngine<T> {
    Task<IResult> Insert(T Data);
    Task<IResult> Update(string ConditionData_Json, T Data);
    Task<IResult> Read(string ConditionData_Json);
    Task<IResult> Remove(string ConditionData_Json);
}
```

### 查詢條件傳遞方式
所有查詢條件統一序列化為 JSON 字串傳入，各 Repository 自行解析：
```csharp
string condition = JsonSerializer.Serialize(dto.getCondition("EVT2569_GFT98142"));
// → {"_id":"EVT2569_GFT98142"}
```

### ConnectionSettings（appsettings.json 結構）
```json
{
  "ConnectionSettings": {
    "Mongo":   { "Uri": "", "User": "", "Password": "" },
    "Elastic": { "EndPoint": "", "ApiKey": "" },
    "Redis":   { "EndPoint": "", "Port": 0, "User": "", "Password": "" }
  }
}
```

---

## 尚未解決的問題

### P0（必須處理）
- appsettings.json 仍有明文憑證，需移至環境變數並加入 .gitignore

### P1（架構債）
- Program.cs 的 BuildServiceProvider 仍呼叫兩次，第二次應改用同一個 provider
- NullLoggerFactory / NullLogger 命名（目前仍叫 LoggerFactory / logger，與 MS 官方命名衝突）
- Application/Sample 層的 OrderRepository_* 繼承 Infrastructure 具體類別（違反 DIP，長期目標）

### P2（整理）
- Core/DTO/ 命名語意不精確（放的是 DataGenerator，不是 DTO）
- Core/Interface/ 部分檔案有殘留 System.* 樣板 using

---

## 接續指令

新 Session 開始時請先閱讀此文件，然後直接從「尚未解決的問題」繼續，不需重新分析整體架構。
