# Sprint S19: 修復取號事件資料欄位缺失 (Fix GetNumber Field Loss)

## 1. 描述 (Description)
修正 `UpdateSellerGetNumberEvent` 在資料傳輸過程中發生的欄位截斷問題，確保 MongoDB `e_shipment_m` 包含完整物流資訊。

## 2. 工程師 (Kiro) 執行清單
- [ ] **Redis Model 修正**：
    - 檔案：`CPF.Services.Redis.Post/Model/QueryModel/MongoDB/UpdateSellerGetNumberEvent.cs`
    - 動作：擴充 `SellerGetNumberArgs`，引入 `EsmmData` 巢狀物件。
- [ ] **資料模擬器修正**：
    - 檔案：`CPF.Services.Redis.Post/Model/AddOrderEventRandomDataGenerator.cs`
    - 動作：在 `GetUpdateSellerGetNumberEventObject` 中補齊 `EsmmNo`, `AuthCode`, `ShipNoA` 等測試資料。
- [ ] **Worker 映射修正**：
    - 檔案：`CPF.Service.SendDataToMongoDB/Worker.cs`
    - 動作：改為對應整個 `Args.esmm` 物件屬性，而非硬編碼兩位數值。

## 3. 驗收標準 (Acceptance Criteria)
- 執行取號後，MongoDB `e_shipment_m` 必須具備：
    - `esmm_no`
    - `esmm_ship_no`
    - `esmm_status`
    - `esmm_ship_no_auth_code`
    - `esmm_ship_no_a`
    - `esmm_ship_method`
    - `esmm_ibon_app_flag`
