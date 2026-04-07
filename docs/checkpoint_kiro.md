# Checkpoint — dbSDK 重構歷程（Kiro 用）

## 專案基本資訊
- 專案名稱：NO3._dbSDK_Imporve
- 框架：.NET 8
- 目標：建立支援 MongoDB / Elasticsearch / Redis 的通用 SDK

## 目前解耦評分：7 / 10

## 最終目錄結構
```
Application/
  dbSDKEngine.cs              ← Use Case，只依賴 Core 介面
  Sample/
    Mongo/  Elastic/  Redis/  ← IOrderRepository_* / OrderRepository_*

Core/
  Abstraction/
    dbDriver.cs               ← abstract，實作 IdbDriver
    BaseRandomDataGenerator.cs← 隨機工具基底
  DTO/
    DTO.cs                    ← 實作 IDTO，產生 Condition
    EventGiftRandomDataGenerator.cs ← 注入 IUniversalMapper，產生測試資料
  Entity/
    EventGiftModel.cs / EventGiftSummaryModel.cs / Query.cs
  Interface/
    IEngine<T> / IRepository<T> / IResult / IdbDriver
    IDTO / IUniversalMapper / IRandamDataGenerator<T,T1>
  Models/
    Result.cs        ← public, immutable, static factory
    ConnectionSettings.cs / DbDetail
    Condition.cs     ← immutable，_id only getter

Infrastructure/
  Driver/   ElasticDriver / MongoDBDriver / RedisDriver
  MAP/      UniversalMapper  ← AutoMapper 封裝，ConcurrentDictionary 快取
  Persistence/
    Elastic/ Mongo/ Redis/   ← *Repository<T> + Map + Filter
```

## 已解決問題清單

### P0（功能性）
- [x] IEngine<T> 四個方法改為 Task<IResult> 回傳
- [x] dbSDKEngine 補上 return
- [x] MongoRepository.updateData 補 await
- [x] RedisRepository.updateData 改回傳 IResult（不再拋例外）
- [x] Result 改為 public + immutable + static factory
- [x] IResult 移回 Core/Interface/IResult.cs
- [x] 憑證從 .cs 移至 appsettings.json（結構正確，值仍明文）

### P1（架構）
- [x] dbSDKEngine 移至 Application 層
- [x] Entity 移至 Core/Entity/
- [x] ConnectionSettings namespace 與路徑對齊（Core.Models）
- [x] dbDriver._Service 改為只有 getter
- [x] IRandamDataGenerator 改為 public
- [x] RandomDataGenerator 重構為可注入類別（EventGiftRandomDataGenerator）
- [x] UniversalMapper 移至 Infrastructure/MAP/
- [x] LoggerFactory.Dispose() / AddProvider() 改空實作
- [x] logger.BeginScope() 改回傳 null
- [x] Test_Condition 移至 Core/Models/Condition
- [x] Program.cs 死碼清除（mongoConnStr / redisConnStr / 開發區段）

### P2（整理）
- [x] 跨 Driver 污染 using 清除（MongoDB.Bson in Redis、Elastic in Redis）
- [x] dbDriver 子類別移除重複賦值 _Service
- [x] EventGiftRandomDataGenerator 移除重複的 GetRandomFrom

## 尚未解決

### P0
- [ ] appsettings.json 明文憑證 → 環境變數 + .gitignore

### P1
- [ ] Program.cs BuildServiceProvider 仍呼叫兩次（第二次應改用同一個 provider）
- [ ] LoggerFactory / logger 命名與 MS 官方衝突，建議改名 NullLoggerFactory / NullLogger
- [ ] Application/Sample 層繼承 Infrastructure 具體類別（DIP 違反，長期目標）

### P2
- [ ] Core/DTO/ 命名語意不精確（放的是 DataGenerator，不是 DTO）
- [ ] Core/Interface/ 殘留 using（IdbDriver、IResult 的 System.* 樣板）
