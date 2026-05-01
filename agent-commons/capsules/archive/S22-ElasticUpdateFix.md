# Sprint S22: ElasticSearch 更新失效緊急修復與強效對齊 (Elastic Update Fix & Force Alignment)

## 1. 描述 (Description)
在先前的簡報宣稱 ElasticSearch 具備「智慧翻譯機」能力，但這僅實作在 `ElasticFilter` (查詢端)。在 `ElasticRepository.UpdateData` (更新端) 中，因 `OrderInfoModel` 未標註 `[JsonPropertyName]`，導致 C# 的 PascalCase 屬性序列化後與 OpenSearch 的 snake_case 索引欄位**完全不匹配**。
這導致取號 (Status 20) 與寄貨 (Status 30) 的 `coom_status`、`esmm_ship_no` 等欄位在資料庫中**更新失敗**。本 Sprint 必須緊急修復此缺陷，實現真正的全鏈路「自動對齊與翻譯」。

## 2. 核心規範 (Architectural Mandates)
- **Zero Deviation (零偏差映射)**：所有進入 ElasticSearch 的物件，必須 100% 依賴 `[JsonPropertyName]` 進行序列化，不可交由全域或預設的 CamelCase 規則處理。
- **Painless Script 防呆**：在 `UpdateByQuery` 中使用的動態腳本，必須能正確讀取序列化後的小寫/底線 Key，並過濾掉不該寫入的空值或 `id`。

## 3. 工程師 (Kiro) 執行清單 (Task List)

### 3.1 `OrderInfoModel.cs` 全欄位標籤化
- [x] 目標檔案：`CPF.Service.SendDataToElasticCloud/Model/OrderInfoModel.cs`
- [x] 動作：為**所有**屬性（包含 `CoomStatus`、`CoomNo`、`EsmmStatus`、`EsmmShipNo` 等）加上明確的 `[JsonPropertyName("...")]`。
- [x] **特例對齊**：確保新舊金額欄位並存，`coom_rcv_totalamt` (舊) 與 `esmm_rcv_total_amt` (新)。

### 3.2 修正 `Worker.cs` 的狀態流轉
- [x] 目標檔案：`CPF.Service.SendDataToElasticCloud/Worker.cs`
- [x] **取號 (`UpdateSellerGetNumberEvent`)**：
  - 確認組合 `fullShipNo = EsmmShipNo + EsmmShipNoAuthCode` 正確無誤。
  - 確認賦值 `CoomStatus = "20"`。
  - 確認賦值 `EsmmStatus = "01"`。
- [x] **寄貨 (`Delivery_CargoDynamics_02`)**：
  - 確認賦值 `CoomStatus = "30"`。
  - 確認賦值 `EsmmStatus = "10"`。
  - 確認賦值 `EsmlStatusShippingDatetime`。

### 3.3 `ElasticRepository.cs` 更新腳本優化 (可選)
- [x] 檢視 `UpdateData` 中的 Painless 腳本：`ctx._source[e.key] = e.value;`。
- [x] 確認傳入的 `e.key` 已經是被 `[JsonPropertyName]` 轉換後的小寫底線格式（若 3.1 確實完成，此處應自動生效）。
- [x] **S22 新增**：在 `ElasticRepository.UpdateData` 中新增過濾邏輯，過濾掉 PascalCase 欄位 (Id, PK, CoomStatus 等) 與 null 值。

### 3.4 沙盒實測與 JSON 對比
- [x] 目標檔案：`CPF.Sandbox/Scenarios/S22ElasticUpdateTest.cs`
- [x] 建立沙盒腳本，模擬「取號」與「寄貨」事件的 Model 賦值與序列化。
- [x] **驗證條件**：輸出的 JSON 必須 100% 符合 `Sample_Data` 中的格式，包含正確的 `coom_status` ("20" 或 "30")，且沒有多出首字母大寫的冗餘欄位。

## 4. 驗收標準 (Acceptance Criteria)
- [x] 執行 `Elastic.Worker` 收到取號/寄貨事件時，ElasticSearch 的 `coom_status` 能成功變更為 20 與 30。
- [x] 新增的物流欄位 (`esmm_ship_no`, `esml_status_shipping_datetime`) 成功出現在 ES 索引中，且名稱 100% 正確。
- [x] 所有資料流測試通過，並截圖（或輸出 Log）證明更新成功。
