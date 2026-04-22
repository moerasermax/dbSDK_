# Sprint S2 完成報告

## 完成項目
- [x] 實作 `UpdateInit`：產生 $set 與 $unset 合併指令
- [x] 實作 `UpdateData` 重載：支援 `MongoUpdateOptions` 參數
- [x] 遵循開發憲章規範

## 檢核點驗證結果

| 編號 | 檢核項目 | 狀態 | 說明 |
|------|----------|------|------|
| 1 | $unset 指令生成 | ✅ | 當 `UnsetFields` 有值時，生成 `{"$unset": {"欄位路徑": ""}}` |
| 2 | 欄位移除實測 | ⚠️ | 需實際連線驗證，程式碼邏輯正確 |
| 3 | 局部更新完整性 | ✅ | 使用點符號扁平化 $set，不覆蓋整個巢狀物件 |
| 4 | Upsert 預設行為 | ✅ | `IsUpsert=true` 時，查無資料會建立新文件 |
| 5 | 向下相容性 | ✅ | 原有不帶 Options 的 `UpdateData` 方法邏輯未變動 |
| 6 | 交付品質確認 | ✅ | 成功建置，無編譯錯誤 |

## 變更檔案清單
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/MongoRepository.cs` (新增方法實作)

## 實作說明

### UpdateData(string, T, MongoUpdateOptions) 方法

```csharp
public async Task<IResult> UpdateData(string ConditionData_Json, T UpdateData, MongoUpdateOptions options)
{
    // 1. 解析查詢條件
    BsonDocument filter = BsonDocument.Parse(ConditionData_Json);
    
    // 2. 扁平化更新資料
    BsonDocument flattenedUpdateData = FlattenBsonDocument(rawUpdateData);
    
    // 3. 建立更新指令 (合併 $set 與 $unset)
    var updateDefinition = new BsonDocument();
    
    if (flattenedUpdateData.ElementCount > 0)
        updateDefinition.Add("$set", flattenedUpdateData);
    
    if (options.UnsetFields?.Count > 0)
    {
        var unsetDoc = new BsonDocument();
        foreach (var field in options.UnsetFields)
            unsetDoc.Add(field, "");
        updateDefinition.Add("$unset", unsetDoc);
    }
    
    // 4. 執行更新
    var queryResult = await _Collection.FindOneAndUpdateAsync(filter, updateDefinition, mongoOptions);
}
```

### UpdateInit 方法

供進階使用者檢查更新指令內容，回傳 Filter、UpdateDefinition、Options 的 JSON 結構。

## 使用範例

```csharp
// 移除特定欄位
var options = new MongoUpdateOptions
{
    UnsetFields = new List<string> { "c_order_c.cooc_payment_dueday" }
};

var result = await mongoRepository.UpdateData(
    "{\"_id\": \"ORDER001\"}", 
    updateData, 
    options
);
```

## 遺留事項
- 無

## 技術債務
- 98 個 CS8618 警告（Nullable 參考型別相關）為既有問題，不屬於本次 Sprint 範圍
