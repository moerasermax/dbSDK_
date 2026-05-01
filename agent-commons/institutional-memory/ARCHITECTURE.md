# dbSDK 系統架構文件

> 專案：NO3._dbSDK_Imporve（.NET 8）  
> 更新日期：2026-04-15  
> 解耦評分：7.5 / 10

---

## 目錄結構

```
NO3._dbSDK_Imporve/
├── Program.cs                          ← 入口點，DI 組裝 + TestFlow
├── appsettings.json                    ← 連線設定（placeholder，實際值由環境變數覆蓋）
│
├── Application/                        ← Use Case 層
│   ├── DbSDKEngine.cs                  ← 通用 CRUD 引擎，只依賴 Core 介面
│   ├── EventGiftRandomDataGenerator.cs ← 測試資料產生器
│   └── Sample/                         ← 範例 Repository 實作
│       ├── Mongo/  OrderRepository_Mongo.cs
│       ├── Elastic/OrderRepository_Elastic.cs
│       └── Redis/  OrderRepository_Redis.cs
│
├── Core/                               ← 核心層（不依賴任何外部）
│   ├── Interface/                      ← 所有公開介面
│   │   ├── IEngine<T>
│   │   ├── IRepository<T>
│   │   ├── IResult
│   │   ├── IdbDriver
│   │   ├── IDTO
│   │   ├── IUniversalMapper
│   │   └── IRandamDataGenerator<T,T1>
│   ├── Models/
│   │   ├── Result.cs                   ← immutable value object
│   │   ├── ConnectionSettings.cs       ← 連線設定 POCO
│   │   └── Condition.cs                ← 查詢條件（immutable）
│   ├── Entity/
│   │   ├── EventGiftModel.cs           ← 繼承 Order
│   │   ├── EventGiftSummaryModel.cs    ← 繼承 OrderSummary
│   │   └── Query.cs
│   ├── Abstraction/
│   │   └── DbDriver.cs                 ← abstract，實作 IdbDriver
│   └── External/
│       └── ObjectExtension.cs          ← 通用工具（deep copy）
│
└── Infrastructure/                     ← 技術實作層
    ├── Driver/
    │   ├── MongoDBDriver.cs
    │   ├── ElasticDriver.cs
    │   └── RedisDriver.cs
    ├── Persistence/
    │   ├── Mongo/   MongoRepository<T> + MongoMap
    │   ├── Elastic/ ElasticRepository<T> + ElasticMap + ElasticFilter
    │   └── Redis/   RedisRepository<T> + RedisMap
    ├── MAP/
    │   └── UniversalMapper.cs          ← AutoMapper 封裝，ConcurrentDictionary 快取
    ├── External/
    │   └── BaseRandomDataGenerator.cs  ← 隨機工具基底
    └── DTO/
        └── DTO.cs                      ← 實作 IDTO
```

---

## Class Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                           Program.cs                                │
│  init() → BuildServiceProvider() → TestFlow_Mongo()                │
└─────────────────────────────────────────────────────────────────────┘
                              │ uses
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                        APPLICATION LAYER                            │
│                                                                     │
│  ┌─────────────────────┐    ┌──────────────────────────────────┐   │
│  │   DbSDKEngine<T>    │    │   EventGiftRandomDataGenerator   │   │
│  │ implements IEngine  │    │ extends BaseRandomDataGenerator  │   │
│  │                     │    │ implements IRandamDataGenerator  │   │
│  │ + Insert(T)         │    │                                  │   │
│  │ + Update(cond, T)   │    │ + Generate()                     │   │
│  │ + Read(cond)        │    │ + Generate(count)                │   │
│  │ + Remove(cond)      │    │ + ToSummary(full)                │   │
│  └──────────┬──────────┘    └──────────────────────────────────┘   │
│             │ depends on IRepository<T>                             │
│  ┌──────────┴──────────────────────────────────────────────────┐   │
│  │                    Sample/                                   │   │
│  │  OrderRepository_Mongo   : MongoRepository<Order>           │   │
│  │  OrderRepository_Elastic : ElasticRepository<OrderSummary>  │   │
│  │  OrderRepository_Redis   : RedisRepository<Query>           │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
                              │ depends on (Core interfaces only)
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                           CORE LAYER                                │
│                                                                     │
│  Interface/                                                         │
│  ┌──────────────┐  ┌───────────────┐  ┌──────────────────────┐    │
│  │  IEngine<T>  │  │ IRepository<T>│  │       IResult        │    │
│  │ + Insert     │  │ + InsertData  │  │ + IsSuccess : bool   │    │
│  │ + Update     │  │ + UpdateData  │  │ + Msg : string       │    │
│  │ + Read       │  │ + GetData     │  │ + DataJson : string  │    │
│  │ + Remove     │  │ + RemoveData  │  └──────────────────────┘    │
│  └──────────────┘  └───────────────┘                               │
│                                                                     │
│  ┌──────────────┐  ┌────────────────┐  ┌─────────────────────┐    │
│  │    IDTO      │  │IUniversalMapper│  │IRandamDataGenerator │    │
│  │+GetCondition │  │ + Map<S,D>     │  │ + Generate()        │    │
│  └──────────────┘  └────────────────┘  │ + Generate(count)   │    │
│                                        │ + ToSummary()       │    │
│  ┌──────────────┐                      └─────────────────────┘    │
│  │  IdbDriver   │                                                   │
│  │ + _Service   │                                                   │
│  └──────────────┘                                                   │
│                                                                     │
│  Models/                         Entity/                            │
│  ┌───────────────────┐           ┌──────────────────────────────┐  │
│  │  Result : IResult │           │ EventGiftModel : Order       │  │
│  │  + SetResult()    │           │ EventGiftSummaryModel        │  │
│  │  + SetErrorResult │           │   : OrderSummary             │  │
│  ├───────────────────┤           │ Query                        │  │
│  │ ConnectionSettings│           └──────────────────────────────┘  │
│  │ + Mongo:DbDetail  │                                              │
│  │ + Elastic:DbDetail│           ┌──────────────────────────────┐  │
│  │ + Redis:DbDetail  │           │ Condition                    │  │
│  ├───────────────────┤           │ + _id : string (immutable)   │  │
│  │ Abstraction/      │           └──────────────────────────────┘  │
│  │ DbDriver:IdbDriver│                                              │
│  ├───────────────────┤           ┌──────────────────────────────┐  │
│  │ External/         │           │ ObjectExtension              │  │
│  │ ObjectExtension   │           │ + Copy<T>(source) : T        │  │
│  └───────────────────┘           └──────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                              │ implements
                              ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE LAYER                           │
│                                                                     │
│  Driver/                                                            │
│  ┌──────────────────┐  ┌──────────────────┐  ┌─────────────────┐  │
│  │  MongoDBDriver   │  │  ElasticDriver   │  │   RedisDriver   │  │
│  │  : DbDriver      │  │  : DbDriver      │  │  : DbDriver     │  │
│  │  + GetDatabase() │  │  + GetClient()   │  │  + GetDataBase()│  │
│  └──────────────────┘  └──────────────────┘  └─────────────────┘  │
│                                                                     │
│  Persistence/                                                       │
│  ┌──────────────────────┐  ┌───────────────────────┐              │
│  │  MongoRepository<T>  │  │  ElasticRepository<T> │              │
│  │  : IRepository<T>    │  │  : IRepository<T>     │              │
│  │  - _map: MongoMap    │  │  - _map: ElasticMap   │              │
│  │  + InsertData()      │  │  + InsertData()       │              │
│  │  + GetData()         │  │  + GetData()          │              │
│  │  + UpdateData()      │  │  + UpdateData()       │              │
│  │  + RemoveData()      │  │  + RemoveData()       │              │
│  └──────────────────────┘  └───────────────────────┘              │
│                                                                     │
│  ┌──────────────────────┐  ┌─────────────┐  ┌─────────────────┐  │
│  │  RedisRepository<T>  │  │  MongoMap   │  │   ElasticMap    │  │
│  │  : IRepository<T>    │  │+ToBsonDoc() │  │ + ToFilter()    │  │
│  │  + InsertData()      │  └─────────────┘  └─────────────────┘  │
│  │  + GetData()         │                                          │
│  │  + UpdateData()      │  ┌─────────────────────────────────┐   │
│  │  + RemoveData()      │  │ ElasticFilter                   │   │
│  │  + PollingData()     │  │ + MustConditions                │   │
│  └──────────────────────┘  │ + ShouldConditions              │   │
│                             │ + Eq() / Gte() / Contains()    │   │
│  MAP/                       └─────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ UniversalMapper : IUniversalMapper                           │  │
│  │ + Map<TSource, TDestination>(source)                         │  │
│  │ - _mapperCache: ConcurrentDictionary (AutoMapper 快取)       │  │
│  │ [內含 NullLoggerFactory / NullLogger]                        │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  External/                       DTO/                               │
│  ┌──────────────────────────┐    ┌──────────────────────────────┐  │
│  │ BaseRandomDataGenerator  │    │ DTO : IDTO                   │  │
│  │ + GetRandomFrom()        │    │ + GetCondition(id):Condition │  │
│  │ # NextInt()              │    └──────────────────────────────┘  │
│  └──────────────────────────┘                                      │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 依賴方向（Dependency Flow）

```
Program.cs
    │
    ├──► Application/DbSDKEngine<T>
    │         └──► Core/IRepository<T>  ◄── Infrastructure/MongoRepository<T>
    │                                   ◄── Infrastructure/ElasticRepository<T>
    │                                   ◄── Infrastructure/RedisRepository<T>
    │
    ├──► Application/EventGiftRandomDataGenerator
    │         └──► Core/IUniversalMapper ◄── Infrastructure/UniversalMapper
    │
    ├──► Core/ObjectExtension
    │
    └──► Core/IDTO ◄── Infrastructure/DTO

Core        ← 不依賴任何層（純介面 + 模型）
Infrastructure → 依賴 Core（實作 Core 介面）
Application    → 依賴 Core（透過介面操作）
```

---

## 各層職責說明

| 層級 | 職責 | 允許依賴 |
|---|---|---|
| Core | 介面定義、Entity、Value Object | 無 |
| Application | Use Case 協調、測試資料產生 | Core only |
| Infrastructure | 技術實作（DB、Mapper、Driver） | Core |
| Program.cs | DI 組裝、入口點 | 全部 |

---

## 已知架構問題

### DIP 違反（Application/Sample）

目前狀況（繼承，強耦合）：
```
OrderRepository_Mongo : MongoRepository<Order>   ← 直接繼承 Infrastructure
OrderRepository_Elastic : ElasticRepository<OrderSummary>
OrderRepository_Redis : RedisRepository<Query>
```

建議改為組合（composition）：
```
OrderRepository_Mongo
    └── 持有 IRepository<Order>
            ↑ 實作
        MongoRepository<Order>
```

---

## 解耦評分

| 面向 | 分數 |
|---|---|
| Core 介面純淨度 | 9/10 |
| 層級隔離 | 7/10 |
| DI 組裝 | 7/10 |
| 功能正確性 | 7/10 |
| 安全性 | 9/10 |
| 命名一致性 | 9/10 |
| **整體** | **7.5 / 10** |
