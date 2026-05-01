# Sprint S5：貨態更新原子化流程實作

## 任務目標
實作「賣家取號（Status 20）」到「寄貨完成（Status 30）」的連續資料流，並支援 MongoDB 陣列追加邏輯。

## 需求背景
1. **業務連續性**：取號（Status 20）時初始化物流主檔，寄貨（Status 30）時更新物流狀態。
2. **歷程完整性**：必須完整紀錄每一筆貨態變更（`esml` 陣列），禁止覆蓋舊有歷程。

## 任務清單
- [ ] **Model 對齊**
    - [ ] `OrderModel` 與 12 個子模型屬性需完全對應 `Dynamodb欄位_20251106.csv`。
- [x] **SDK 陣列追加 ($push)**
    - [x] 擴充 `MongoUpdateOptions`，新增 `PushFields` 屬性。
    - [x] 擴充 `MongoRepository.UpdateData` 與 `UpdateInit`，支援 `$push` 操作。
    - [x] 擴充 `MultiCultureDateTimeSerializer`，支援 `yyyyMMddHHmmss.fff` 格式。
- [x] **Application 層：業務適配與解析 (The Adapters)**
    - [x] 在 `Application/DTO/` 建立 `ShippingUpdateDto.cs`，承接 `Redis_1` 的管線字串結構。
    - [x] 實作解析邏輯：將 `Esml_EsmmNo_List` (如 `01|T1,10|T2`) 分解為結構化物件清單。
    - [x] 實作解析邏輯：確保日期字串正確轉錄，並具備重複狀態碼的過濾機制。
- [x] **Application 層：雙端同步協調 (The Coordinator)**
    - [x] 建立 `ShippingSyncService`，協調 MongoDB 與 Elasticsearch 雙端同步。
    - [x] **MongoDB 同步**：透過 `MongoUpdateOptions.PushFields` 指定 `e_shipment_l` 與 `e_shipment_s` 為追加項。
    - [x] **Elastic 同步**：實作全量展平更新，且當狀態為 `10` (寄貨) 時，自動寫入 `esml_status_shipping_datetime` 欄位。
- [x] **驗證與測試 (Verification)**
    - [x] 在 `Program.cs` 建立管線字串解析測試。
    - [x] 擴充 `Orders` 與 `OrderSummary` Model。

## 檢核點
1. ✅ **分層守則**：字串解析邏輯位於 `Application` 層，`Infrastructure` 層僅負責執行 Bson 指令。
2. ✅ **原子性**：MongoDB 的 `$set` 與 `$push` 必須在同一次調用中完成。
3. ✅ **資料一致性**：Status 10 (寄貨) 的時間戳記必須同時出現在 Mongo 歷程與 Elastic 業務欄位。
4. ✅ 所有的日期解析必須符合 `yyyyMMddHHmmss.fff`。

## 完成日期
- 2026-04-23: 完成 `$push` 功能實作、日期格式擴充、DTO 與同步服務

## 受影響檔案
- `NO3._dbSDK_Imporve/Application/DTO/ShippingUpdateDto.cs` (新增)
- `NO3._dbSDK_Imporve/Application/Services/ShippingSyncService.cs` (新增)
- `NO3._dbSDK_Imporve/Core/Entity/OrderModels.cs` (新增)
- `NO3._dbSDK_Imporve/Core/Entity/EventGiftModel.cs` (修改 - 擴充 Orders)
- `NO3._dbSDK_Imporve/Core/Entity/EventGiftSummaryModel.cs` (修改 - 擴充 OrderSummary)
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/Models/MongoUpdateOptions.cs` (修改)
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/MongoRepository.cs` (修改)
- `NO3._dbSDK_Imporve/Infrastructure/Persistence/Mongo/Serializers/MultiCultureDateTimeSerializer.cs` (修改)
- `NO3._dbSDK_Imporve/Program.cs` (修改 - 加入測試流程)
