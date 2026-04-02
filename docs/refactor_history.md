# dbSDK 重構歷程與總結

日期：2026-04-02
專案：NO3._dbSDK_Imporve（.NET 8）

---

## 起點狀態

初始解耦評分：4.5 / 10

主要問題：
- 所有憑證硬編碼在 Program.cs
- IEngine<T> 四個方法回傳 Task（無回傳值），結果永遠丟棄
- MongoRepository.updateData 缺少 await，序列化的是 Task 物件而非資料
- RedisRepository.updateData 拋 NotImplementedException（LSP 違反）
- Result 是 internal class，且有可變狀態（public setter）
- IResult 介面定義在 Core/Models/Result.cs，命名空間語意錯誤
- dbSDKEngine 放在 Infrastructure 層（Use Case 錯置）
- Entity（Order / OrderSummary / Query）散落在 Sample 層的 Interface 檔案內
- 跨 Driver 污染 using（Redis 引用 MongoDB.Bson、Elastic SDK）
- dbDriver._Service 有 public setter，子類別重複賦值
- RandomDataGenerator 是靜態手工 Singleton，非 thread-safe，不可注入
- Sample 層透過繼承耦合 Infrastructure 具體類別

---

## 重構歷程

### 階段一：P0 功能性修正

**IEngine<T> 回傳值**
- 問題：四個方法簽章為 Task，Repository 回傳的 IResult 全部丟棄
- 修正：改為 Task<IResult>，dbSDKEngine 補上 return

**MongoRepository.updateData**
- 問題：FindOneAndUpdateAsync 前缺少 await，queryResult 是 Task 物件
- 修正：補上 await，正確取得資料後再序列化

**RedisRepository.updateData**
- 問題：throw new NotImplementedException()，呼叫端無法安全使用介面
- 修正：改為 return Result.setErrorResult(...)，不再拋例外

**Result 重構**
- 問題：mutable public setter，internal 可見性
- 修正：改為 private 建構子 + static factory method，所有屬性只有 getter，改為 public

**IResult 歸位**
- 問題：IResult 定義在 Core/Models/Result.cs，命名空間是 Core.Models
- 修正：移回 Core/Interface/IResult.cs，MongoRepository 的 alias workaround 消除

---

### 階段二：憑證安全性

**appsettings.json 結構化**
- 問題：MongoDB 密碼、Elastic API Key、Redis 密碼全部明文寫在 Program.cs
- 修正：建立 ConnectionSettings / DbDetail POCO，透過 IConfiguration.Bind 載入
- 三個 Driver 建構子統一接收 ConnectionSettings，不再各自接收獨立參數
- 注意：appsettings.json 內值仍為明文，.gitignore 尚未設定（待辦）

---

### 階段三：層級結構重整

**Application 層建立**
- 問題：dbSDKEngine 放在 Infrastructure，Use Case 與技術實作混在同一層
- 修正：建立 Application/ 目錄，dbSDKEngine 移入

**Entity 歸位**
- 問題：Order / OrderSummary / Query / EventGiftModel 散落在 Sample 層的 Interface 檔案
- 修正：建立 Core/Entity/，所有 Entity 獨立歸位

**Sample 層移至 Application**
- 問題：Sample 層定位模糊，與 Infrastructure 混在一起
- 修正：移至 Application/Sample/，命名空間對齊

**ConnectionSettings namespace 對齊**
- 問題：檔案在 Core/Models/，namespace 是 Core.Configurations
- 修正：namespace 改為 Core.Models，與路徑一致

---

### 階段四：細節清理

**dbDriver._Service**
- 問題：public setter，子類別建構子重複賦值
- 修正：改為只有 getter，建構子賦值一次即可

**跨 Driver 污染 using 清除**
- ElasticMap.cs 的 using MongoDB.Bson 移除
- RedisRepository.cs 的 using MongoDB.Bson 移除
- IOrderRepository_Redis.cs / OrderRepository_Redis.cs 的 using Elastic.Clients.Elasticsearch.Snapshot 移除

**Condition 類別**
- 問題：Test_Condition 定義在 Program.cs 底部
- 修正：移至 Core/Models/Condition.cs，immutable（只有 getter）

**Program.cs 清理**
- 移除死碼：mongoConnStr / redisConnStr（組裝後從未使用）
- 移除開發區段 #region

---

### 階段五：TestFlow 重構

**TestFlow 移出 dbSDKEngine**
- 問題：TestFlow 放在 dbSDKEngine 內，泛型內部強制 cast 成 EventGiftModel，破壞泛型語意
- 問題：Read 回傳 IResult，被當成 EventGiftModel cast，永遠是 null
- 修正：TestFlow 移至 Program.cs，改用 .DataJson 正確取值

---

### 階段六：RandomDataGenerator 重構

**靜態 Singleton 改為可注入類別**
- 問題：靜態手工 Singleton，非 thread-safe，無法透過 DI 注入，不可測試
- 修正：
  - 建立 BaseRandomDataGenerator（abstract）：共用 GetRandomFrom / NextInt
  - 建立 IRandamDataGenerator<T,T1>（public interface）
  - EventGiftRandomDataGenerator 繼承 BaseRandomDataGenerator，實作介面
  - 透過建構子注入 IUniversalMapper

**ToSummary 改用 AutoMapper**
- 問題：手工逐欄位 mapping，維護成本高
- 修正：ToSummary 改呼叫 _map.Map<EventGiftModel, EventGiftSummaryModel>(full)

---

### 階段七：UniversalMapper 建立

**AutoMapper 封裝**
- 建立 IUniversalMapper 介面（Core/Interface/）
- 建立 UniversalMapper 實作（Infrastructure/MAP/）
- 使用 ConcurrentDictionary 快取 IMapper，避免每次 Map 重新建立 Configuration
- AutoMapper 16.1.1 需要傳入 ILoggerFactory，建立 NullLoggerFactory / NullLogger
- NullLoggerFactory：Dispose() / AddProvider() 空實作，CreateLogger 回傳 NullLogger
- NullLogger：BeginScope() 回傳 null，IsEnabled 全部 true，Log 輸出至 Console

**IDTO 建立**
- 建立 IDTO 介面與 DTO 實作，統一產生 Condition 物件
- Program.cs 改透過 dto.getCondition() 取得查詢條件

---

## 最終狀態

解耦評分：7 / 10

```
起點 4.5 → 最終 7.0
```

### 各面向分數變化

| 面向 | 起點 | 最終 |
|---|---|---|
| Core 介面純淨度 | 6/10 | 9/10 |
| 層級隔離 | 3/10 | 8/10 |
| DI 組裝 | 4/10 | 6/10 |
| 功能正確性 | 3/10 | 8/10 |
| 安全性 | 1/10 | 6/10 |
| 可擴展性 | 5/10 | 7/10 |

---

## 尚未完成（下次繼續）

### P0
- appsettings.json 明文憑證 → 環境變數覆蓋 + .gitignore

### P1
- Program.cs BuildServiceProvider 仍呼叫兩次，第二次應改用同一個 provider
- NullLoggerFactory / NullLogger 命名（目前仍叫 LoggerFactory / logger）
- Application/Sample 層繼承 Infrastructure 具體類別（DIP 違反，長期目標）

### P2
- Core/DTO/ 命名語意不精確（放的是 DataGenerator，不是 DTO）
- Core/Interface/ 部分檔案殘留 System.* 樣板 using
