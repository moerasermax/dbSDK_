# Sprint S40：公用模型重構 (Public Model Refactoring)
tracking_label: P2B-REFACTOR-1

## 任務目標
將 `SearchBySellerAsync` (Search 2) 與 `SearchByBuyerAsync` (Search 3) 的回傳結構，由目前的平坦 JSON 格式重構為與客戶 Golden Out 一致的巢狀模型 (`c_Order_M`, `c_Order_C`, `c_Order_D` 等)。

## 需求背景
目前 SDK 直接回傳 Elastic 的 `OrderDocument` (JsonElement)，不符合客戶提供的數據圖樣。為了確保前端串接無礙且與 Golden Recipe 100% 對齊，必須進行結構化封裝。

## 實作內容
1. **建立 Model**：
   - 建立 `PIC.CPF.OrderSDK.Biz.Read.Elastic/Models/SearchOrderInfoDataModel.cs`。
   - 內部包含類別：`OrderMasterModel (c_Order_M)`, `OrderCartModel (c_Order_C)`, `OrderItemModel (c_Order_D)` 等。
2. **更新 Extension**：
   - 修改 `ConverToExtension.cs` 中的 `ConvertToSearchOrderInfoResultModel`。
   - 將 `OrderDocument` 的欄位正確映射至對應的巢狀 Model 中。
3. **Casing 檢查**：
   - 確保所有屬性的 `[JsonPropertyName]` 與客戶 Golden Out 的 CamelCase 命名完全一致。

## 驗收標準
- [ ] `dump-s2` 的輸出 JSON 包含 `data: [{ "c_Order_M": { ... }, "c_Order_C": { ... } }]` 結構。
- [ ] 專案編譯成功，不影響現有的 BLL 呼叫介面。
