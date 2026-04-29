# Sprint S21: ElasticSearch Parallel Transfer Alignment (平行轉移精準對齊)

## 1. 描述 (Description)
為了確保從 OpenSearch 到 ElasticSearch 的平行轉移不產生任何資料格式偏差，必須針對 `OrderInfoModel` 進行 100% 的 JSON Schema 對齊。所有屬性名稱、大小寫與型別（如字串 vs 數字），必須與客戶端實際運行的 OpenSearch 資料流 (`Sample_Data`) 絕對一致。

## 2. 核心規範 (Architectural Mandates)
- **Zero Deviation (零偏差)**：欄位名稱必須強制使用 `[JsonPropertyName]`，不得依賴全域的序列化設定（如 CamelCase 轉 SnakeCase 的默認行為）。
- **Legacy Compatibility (向下兼容)**：即便原本的欄位命名有瑕疵（如 `coom_rcv_totalamt` 無底線，但新增的 `esmm_rcv_total_amt` 有底線），也必須照單全收，不可隨意「修正」客戶的資料結構。
- **Native Fidelity (沙盒實證)**：必須透過 `CPF.Sandbox` 生成 Mock 物件，並輸出序列化後的 JSON 字串，以肉眼或程式驗證與 `Sample_Data` 內的 `Elastic_更新後` 格式完全吻合。

## 3. 工程師 (Kiro) 執行清單 (Task List)

### 3.1 徹底重構 `OrderInfoModel`
- [x] 目標檔案：`CPF.Service.SendDataToElasticCloud/Model/OrderInfoModel.cs`
- [x] **屬性重構**：
  - 清理不相干的冗餘屬性。
  - 為所有保留屬性加上 `[JsonPropertyName("...")]`。
  - 將 `CoomCuamCid` 與 `CoocMemSid` 設為 `int?` (對應 JSON 內的無引號數字)。
  - 將 `CoodNames` 的 Json 映射改為 `"cood_name"`。
  - 確保同時存在 `CoomRcvTotalAmt` (`"coom_rcv_totalamt"`) 與 `EsmmRcvTotalAmt` (`"esmm_rcv_total_amt"`)。
- [x] **子結構對齊 `CoodItems`**：
  - 確保 `CoodCgdsId` 映射為 `"cood_cgdsid"`。

### 3.2 資料流賦值邏輯更新
- [x] 目標檔案：`CPF.Service.SendDataToElasticCloud/Worker.cs`
- [x] **取號事件 (`UpdateSellerGetNumberEvent`)**：
  - 更新 `EsmmStatus` 為 `"01"`。
  - 組合 `EsmmShipNo` 與 `EsmmShipNoAuthCode` 並賦值。
- [x] **寄貨事件 (`Delivery_CargoDynamics_02`)**：
  - 更新 `CoomStatus` 為 `"30"`。
  - 更新 `EsmmStatus` 為 `"10"`。
  - 提取事件中的時間，賦值給 `EsmlStatusShippingDatetime` (`"esml_status_shipping_datetime"`)。

### 3.3 沙盒驗證體系 (Sandbox Verification)
- [x] 目標檔案：`CPF.Sandbox/Generators/ProductionDataGenerator.cs` 與 `CPF.Sandbox/Scenarios/ElasticSearchScenario.cs`
- [x] 更新 Generator，使其產出的 `OrderInfoModel` 物件包含所有新舊欄位。
- [x] 在 `ElasticSearchScenario` 中，透過 `JsonSerializer.Serialize` 將該物件轉為字串並列印。
- [x] **驗證條件**：輸出的 JSON 必須與 `新增功能_貨態更新_寄貨_資料流.txt` 中的 `Elastic_更新後` 結構完全一致。

## 4. 驗收標準 (Acceptance Criteria)
- [x] `CPF.Service.SendDataToElasticCloud` 編譯成功。
- [x] `CPF.Sandbox` 執行 `ElasticSearchScenario` 時，輸出的 JSON 格式（大小寫、底線、數字/字串型別）完全命中 `Sample_Data` 規範。

## 5. 參考資料流 (Reference Data Flow)
請工程師實作與驗證時，嚴格對照以下從 OpenSearch 實際運行的目標格式（寄貨更新後）：

```json
{
    "coom_no": "CM2604160395986",
    "coom_name": "test",
    "coom_status": "30",
    "coom_temp_type": "01",
    "coom_create_datetime": "2026-04-16T05:39:04.1827004Z",
    "coom_cuam_cid": 528672,
    "cooc_no": "CC2604160431308",
    "cooc_payment_type": "1",
    "cooc_deliver_method": "1",
    "cooc_ord_channel_kind": "1",
    "cooc_mem_sid": 528672,
    "cood_name": [
        "testtesttete test"
    ],
    "coom_rcv_totalamt": 138,
    "cood_items": [
        {
            "cgdd_cgdmid": "GM2512170027503",
            "cgdd_id": "2601270002368654",
            "cood_cgdsid": "2601270002368655",
            "cood_name": "testtesttete test",
            "cood_qty": 1,
            "cood_image_path": "2601270002368647.jpg"
        }
    ],
    "esmm_rcv_total_amt": 138,
    "esmm_ship_no": "D88032120964",
    "esmm_status": "10",
    "esml_status_shipping_datetime": "2026-04-16T06:20:00Z"
}
```
