# Kiro 技術評估報告

> 評估日期：2026-04-23
> 評估者：Kiro (首席工程師)
> 適用 Sprint：S3 (架構優化)、S5 (貨態業務實作)

---

## 1. 文件閱讀摘要

### 1.1 資料流分析

#### Status 20 (賣家取號)
- **Redis 輸入**：扁平化 Args 結構，包含 `CoomNo`, `EsmmShipNo`, `EsmmStatus` 等
- **MongoDB 更新**：
  - 新增 `e_shipment_m` (物流主檔)
  - 新增 `e_shipment_l` 陣列 (長度 = 1)
  - 新增 `e_shipment_s` 陣列 (長度 = 1)
  - 更新 `c_order_m.coom_status` = "20"

#### Status 30 (寄貨完成)
- **Redis 輸入**：包含 `esmm_status` = "10", `esml_status_shipping_datetime`
- **MongoDB 更新**：
  - `e_shipment_l` 陣列**追加**一筆 (長度 1 → 2)
  - `e_shipment_s` 陣列**追加**一筆
  - 更新 `c_order_m.coom_status` = "30"
  - 更新 `e_shipment_m.esmm_status` = "10"

### 1.2 核心挑戰識別

| 挑戰 | 說明 | 難度 |
|------|------|------|
| 陣列追加 | `e_shipment_l` 需從 1 變 2，禁止全量覆蓋 | 高 |
| 日期格式 | `yyyyMMddHHmmss.fff` 格式需正確解析 | 中 |
| 扁平映射 | Redis Args → OrderModel 複雜結構 | 中 |
| 架構解耦 | Repository 從繼承改組合 | 中 |

---

## 2. 技術評估回報

### A. 陣列追加與原子化更新 (Atomic List Append)

#### 問題分析

目前 `FlattenBsonDocument` 的行為：
```csharp
// 現有邏輯：遇到陣列會整個保留
if (element.Value.IsBsonArray)
{
    flatDoc.Add(prefix + element.Name, element.Value);
}
```

這會導致 `$set` 時**全量覆蓋**陣列，而非追加。

#### 執行路徑

**方案：擴充 `MongoUpdateOptions`，新增 `PushFields` 屬性**

```csharp
// MongoUpdateOptions.cs 新增
public class MongoUpdateOptions
{
    public bool IsUpsert { get; set; } = true;
    public List<string>? UnsetFields { get; set; }
    
    // 新增：指定哪些欄位需要使用 $push 追加
    public Dictionary<string, BsonValue>? PushFields { get; set; }
}
```

**擴充 `FlattenBsonDocument` 邏輯**：

```csharp
// MongoRepository.cs 修改
private (BsonDocument setDoc, BsonDocument pushDoc) FlattenBsonDocumentWithPush(
    BsonDocument doc, 
    string prefix = "",
    HashSet<string>? pushFieldPaths = null)
{
    var setDoc = new BsonDocument();
    var pushDoc = new BsonDocument();
    
    foreach (var element in doc.Elements)
    {
        if (prefix == "" && element.Name == "_id") continue;
        if (element.Value.IsBsonNull) continue;
        
        var fullPath = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";
        
        // 判斷是否為需要 $push 的陣列欄位
        if (element.Value.IsBsonArray && pushFieldPaths?.Contains(fullPath) == true)
        {
            // 陣列中的每個元素都需要 $push
            var array = element.Value.AsBsonArray;
            if (array.Count > 0)
            {
                // 取第一個元素作為追加項目
                pushDoc.Add(fullPath, new BsonDocument("$each", array));
            }
        }
        else if (element.Value.IsBsonDocument)
        {
            var (nestedSet, nestedPush) = FlattenBsonDocumentWithPush(
                element.Value.AsBsonDocument, 
                fullPath + ".", 
                pushFieldPaths);
            setDoc.Merge(nestedSet);
            pushDoc.Merge(nestedPush);
        }
        else
        {
            setDoc.Add(fullPath, TryConvertToBsonDateTime(element.Value));
        }
    }
    
    return (setDoc, pushDoc);
}
```

**更新 `UpdateData` 方法**：

```csharp
public async Task<IResult> UpdateData(string ConditionData_Json, T UpdateData, MongoUpdateOptions options)
{
    // ... 前置處理 ...
    
    var (setDoc, pushDoc) = FlattenBsonDocumentWithPush(
        rawUpdateData, 
        "", 
        options.PushFields?.Keys.ToHashSet());
    
    var updateDefinition = new BsonDocument();
    
    if (setDoc.ElementCount > 0)
        updateDefinition.Add("$set", setDoc);
    
    if (pushDoc.ElementCount > 0)
        updateDefinition.Add("$push", pushDoc);
    
    // ... 後續處理 ...
}
```

#### 避免全量覆蓋的關鍵

1. **分離 `$set` 與 `$push`**：陣列欄位不走 `$set`，改走 `$push`
2. **使用 `$each`**：支援一次追加多個元素
3. **呼叫端控制**：透過 `MongoUpdateOptions.PushFields` 明確指定哪些欄位要追加

#### 使用範例

```csharp
// Status 30 追加貨態歷程
var options = new MongoUpdateOptions
{
    PushFields = new Dictionary<string, BsonValue>
    {
        ["e_shipment_l"] = new BsonDocument("esml_esmm_status", "10")
            .Add("esml_status_datetime", DateTime.UtcNow),
        ["e_shipment_s"] = new BsonDocument("esms_dlv_status_no", "1A01")
            .Add("esms_status_datetime", DateTime.UtcNow)
    }
};

await repository.UpdateData(condition, updateData, options);
```

---

### B. 既有工具活用 (No Re-inventing the Wheel)

#### B.1 日期解析 - 活用 `MultiCultureDateTimeSerializer`

**現有支援格式**：
```csharp
// MultiCultureDateTimeSerializer 已支援
"yyyy-MM-ddTHH:mm:ss.fffZ"
"yyyy-MM-ddTHH:mm:ss"
"yyyy/MM/dd HH:mm:ss"
```

**需新增格式**：`yyyyMMddHHmmss.fff`

**擴充方案**：

```csharp
// MultiCultureDateTimeSerializer.cs 修改
private static readonly string[] SupportedFormats = new[]
{
    // 現有格式...
    
    // 新增：CSV/Redis 常見格式
    "yyyyMMddHHmmss.fff",
    "yyyyMMddHHmmss",
    "yyyyMMdd"
};
```

**驗證方式**：
```csharp
// 測試案例
var testCases = new[]
{
    "20260416061156.970",  // yyyyMMddHHmmss.fff
    "20260416061156",      // yyyyMMddHHmmss
    "20260416"             // yyyyMMdd
};

foreach (var dateStr in testCases)
{
    var serializer = new MultiCultureDateTimeSerializer();
    // 應能正確解析
}
```

#### B.2 物件對齊 - 活用 `UniversalMapper`

**問題**：Redis Args 是扁平結構，OrderModel 是巢狀結構

**現有 `UniversalMapper` 能力**：
- 使用 AutoMapper
- 同名欄位自動映射
- 支援快取機制

**執行路徑**：

1. **建立中繼 DTO**：將扁平 Args 先轉為結構化 DTO
2. **使用 `UniversalMapper` 映射**：DTO → OrderModel

```csharp
// 建立 ShippingUpdateDto.cs
public class ShippingUpdateDto
{
    public string? CoomNo { get; set; }
    public string? CoomStatus { get; set; }
    
    // 巢狀結構對應
    public C_Order_M_Dto? coom { get; set; }
    public E_Shipment_M_Dto? esmm { get; set; }
    public List<E_Shipment_L_Dto>? esml { get; set; }
    public List<E_Shipment_S_Dto>? esms { get; set; }
}

// 使用 UniversalMapper
var orderModel = _mapper.Map<ShippingUpdateDto, OrderModel>(dto);
```

**關鍵**：DTO 屬性名稱需與 OrderModel 子模型屬性名稱一致，AutoMapper 會自動處理。

---

### C. Mock 測試與型別安全策略 (Testing & Safety)

#### C.1 IResult<T> 泛型化設計

**介面定義**：

```csharp
// IResult.cs 修改
public interface IResult
{
    bool IsSuccess { get; }
    string Msg { get; }
    string DataJson { get; }
}

public interface IResult<out T> : IResult
{
    T? Data { get; }
}
```

**Result<T> 實作**：

```csharp
public class Result<T> : Result, IResult<T>
{
    public T? Data { get; }
    
    private Result(bool isSuccess, string msg, T? data) 
        : base(isSuccess, msg, data?.ToJson() ?? "")
    {
        Data = data;
    }
    
    public static Result<T> SetResult(string msg, T data, bool isSuccess = true) 
        => new(isSuccess, msg, data);
    
    public static Result<T> SetErrorResult(string methodName, string msg) 
        => new(false, $"發生錯誤請檢查，函示名稱{methodName}\r\n錯誤訊息：{msg}", default);
}
```

#### C.2 TestFlow_Mock 設計

**目標**：在不啟動 DB 的情況下，驗證貨態歷程追加邏輯

```csharp
// Program.cs 新增
static async Task TestFlow_Mock()
{
    Console.WriteLine("=== Mock 測試：貨態歷程追加 ===");
    
    // 1. 模擬 Status 20 資料
    var status20Data = new OrderModel
    {
        PK = "CM2604160395986",
        C_Order_M = new C_Order_M_Model { CoomStatus = "20" },
        E_Shipment_L = new List<E_Shipment_L_Model>
        {
            new() { EsmlEsmmStatus = "01", EsmlStatusDatetime = DateTime.Parse("2026-04-16T06:11:56.97Z") }
        }
    };
    
    // 2. 模擬 Status 30 追加
    var status30Append = new E_Shipment_L_Model
    {
        EsmlEsmmStatus = "10",
        EsmlStatusDatetime = DateTime.Parse("2026-04-16T06:20:00Z")
    };
    
    // 3. 驗證 $push 邏輯 (不連線 DB)
    var options = new MongoUpdateOptions
    {
        PushFields = new Dictionary<string, BsonValue>
        {
            ["e_shipment_l"] = status30Append.ToBsonDocument()
        }
    };
    
    // 4. 使用 UpdateInit 檢查指令
    var mockRepo = new MockMongoRepository<OrderModel>();
    var initResult = await mockRepo.UpdateInit(
        @"{""coom_no"": ""CM2604160395986""}", 
        status20Data, 
        options);
    
    // 5. 驗證結果
    var updateDef = BsonDocument.Parse(initResult.DataJson);
    Assert.True(updateDef.Contains("$push"));
    Assert.True(updateDef["$push"].AsBsonDocument.Contains("e_shipment_l"));
    
    Console.WriteLine("✅ Mock 測試通過：$push 指令正確生成");
}
```

#### C.3 Repository 解耦策略 (繼承 → 組合)

**現有問題**：
```csharp
// OrderRepository_Mongo 目前繼承 MongoRepository<Orders>
public class OrderRepository_Mongo : MongoRepository<Orders>  // ❌ 違反 DIP
{
    public OrderRepository_Mongo(MongoDBDriver driver, MongoMap mongoMap, IDTO dto) 
        : base(driver, mongoMap, dto, "Order") { }
}
```

**重構方案**：

```csharp
// 改為組合模式
public class OrderRepository_Mongo : IRepository<Orders>
{
    private readonly IMongoDBRepository<Orders> _innerRepository;
    
    public OrderRepository_Mongo(MongoDBDriver driver, MongoMap mongoMap, IDTO dto)
    {
        // 組合底層 Repository，而非繼承
        _innerRepository = new MongoRepository<Orders>(driver, mongoMap, dto, "Order");
    }
    
    // 委派實作
    public async Task<IResult> GetData(string condition) 
        => await _innerRepository.GetData(condition);
    
    public async Task<IResult> InsertData(Orders data) 
        => await _innerRepository.InsertData(data);
    
    public async Task<IResult> UpdateData(string condition, Orders data) 
        => await _innerRepository.UpdateData(condition, data);
    
    public async Task<IResult> RemoveData(string condition) 
        => await _innerRepository.RemoveData(condition);
    
    // 可擴充業務特定方法
    public async Task<IResult> UpdateShippingStatus(string coomNo, ShippingUpdateDto dto)
    {
        // 業務邏輯封裝在此
        var options = BuildPushOptions(dto);
        return await _innerRepository.UpdateData(
            $"{{\"coom_no\": \"{coomNo}\"}}", 
            MapToOrders(dto), 
            options);
    }
}
```

**DI 註冊不受影響**：

```csharp
// Program.cs DI 註冊保持不變
services.AddSingleton<IRepository<Orders>, OrderRepository_Mongo>();
```

因為 `OrderRepository_Mongo` 仍然實作 `IRepository<Orders>` 介面。

---

## 3. 執行計畫

### 3.1 Sprint 執行順序

```
S3 (架構優化) → S5 (貨態業務)
```

**理由**：
1. S3 的 `IResult<T>` 是 S5 Mock 測試的基礎
2. S3 的 Repository 解耦讓 S5 的業務邏輯封裝更乾淨

### 3.2 工作項目分解

| 順序 | Sprint | 工作項目 | 預估時間 |
|------|--------|----------|----------|
| 1 | S3 | IResult<T> 泛型化 | 30 分鐘 |
| 2 | S3 | Result<T> 實作 | 30 分鐘 |
| 3 | S3 | OrderRepository_Mongo 改組合模式 | 45 分鐘 |
| 4 | S3 | TestFlow_Mock 實作 | 30 分鐘 |
| 5 | S5 | MongoUpdateOptions 新增 PushFields | 20 分鐘 |
| 6 | S5 | FlattenBsonDocumentWithPush 實作 | 45 分鐘 |
| 7 | S5 | MultiCultureDateTimeSerializer 擴充格式 | 15 分鐘 |
| 8 | S5 | ShippingUpdateDto 與映射邏輯 | 30 分鐘 |
| 9 | S5 | 整合測試 | 30 分鐘 |
| **總計** | | | **約 5 小時** |

### 3.3 風險評估

| 風險 | 等級 | 因應對策 |
|------|------|----------|
| IResult<T> 向下相容 | 中 | 保留原有 `IResult` 介面，`IResult<T>` 繼承它 |
| $push 與 $set 衝突 | 低 | 分離處理，確保同一欄位不會同時出現在兩者 |
| AutoMapper 映射失敗 | 中 | 建立完整的 DTO 命名對照表，確保屬性名稱一致 |
| DI 註冊失效 | 低 | 確保 `OrderRepository_Mongo` 仍實作 `IRepository<Orders>` |

---

## 4. 既有函數活用清單

| 函數/工具 | 用途 | Sprint |
|-----------|------|--------|
| `MultiCultureDateTimeSerializer` | 解析 `yyyyMMddHHmmss.fff` 日期格式 | S5 |
| `FlattenBsonDocument` | 基礎扁平化，擴充支援 `$push` | S5 |
| `TryConvertToBsonDateTime` | 日期字串轉 BsonDateTime | S5 |
| `UniversalMapper.Map<TSource, TDestination>` | DTO → Model 映射 | S5 |
| `MongoUpdateOptions` | 擴充 `PushFields` 屬性 | S5 |
| `Result.SetResult/SetErrorResult` | 擴充泛型版本 | S3 |

---

## 5. 結論

本評估報告確認：

1. **陣列追加**：透過擴充 `MongoUpdateOptions.PushFields` 與新增 `FlattenBsonDocumentWithPush`，可實現真正的增量掛載，避免全量覆蓋。

2. **既有工具活用**：
   - `MultiCultureDateTimeSerializer` 只需新增格式字串即可支援 `yyyyMMddHHmmss.fff`
   - `UniversalMapper` 可直接用於 DTO → Model 映射

3. **Mock 測試**：透過 `IResult<T>` 與 `UpdateInit` 方法，可在不連線 DB 的情況下驗證 `$push` 指令正確性。

4. **架構解耦**：`OrderRepository_Mongo` 改為組合模式後，DI 註冊不受影響，且業務邏輯封裝更清晰。

---

**待 PM (Gemini) 審查確認後，即可開始動工。**
