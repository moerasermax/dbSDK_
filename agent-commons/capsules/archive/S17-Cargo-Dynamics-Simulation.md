# Sprint S17：端到端貨態更新場景模擬 (End-to-End Cargo Dynamics)

## 任務目標
透過 Redis 事件觸發，完整模擬「取號 -> 寄貨 -> 付款更新」的資料流轉，驗證 SDK 對於巢狀物件解析、多資料庫分發及 MongoDB 動態結構初始化的處理能力。

## 需求背景
本任務需應對三種複雜情境：
1. **取號 (Delivery_01)**：處理巢狀 Redis Event，初始化 MongoDB 的 `e_shipment_*` 結構。
2. **寄貨 (Delivery_02)**：模擬外部 DB Model 接入，驗證 MongoDB 的增量 `$push` 指令。
3. **付款更新**：驗證 Redis 事件驅動下的付款狀態變更。

## 任務清單

### 1. 基礎環境修復 (Dependency)
- [x] **修復模型權限**：將 `OrderInfoModel` 改為 `public` 並補齊 `CoomNo` 欄位，解決編譯報錯。
- [ ] **對齊 Internal 模型**：修正 `AppSellerOverView` 系列模型，區分 Query 與 Result。

### 2. 業務模型實作 (Core / Infrastructure)
- [x] **定義 Redis 事件 Model**：依據 `取號_資料流` 實作 `UpdateSellerGetNumberEvent` 及其巢狀類別 (`Coom`, `Esmm`, `Esml`, `Esms`)。
- [x] **定義外部輸入 Model**：實作 `DBModel` 用於模擬寄貨流程。

### 3. 沙盒場景開發 (Sandbox)
- [x] **導入狀態步進驗證 (Stateful Verification)**：
    - 每個場景必須包含：`Insert(初始訂單) -> Read(V1) -> Update(S17事件) -> Read(V2) -> 對比報告`。
- [x] **Method: `Delivery_CargoDynamics_01` (取號)**：
    - 驗證 MongoDB 生成初始化指令。
    - **證明**：原訂單的 `cood_items` 筆數與 `coom_name` 在 V2 中保持不變。
- [x] **Method: `Delivery_CargoDynamics_02` (寄貨)**：
    - 驗證 MongoDB 產生 `$push` 增量指令。
    - **證明**：V2 的 `e_shipment_l` 陣列長度增加，且原有的狀態 Log 依然存在。
- [x] **Method: `Update_Payment_Info` (付款)**：
    - 模擬 Redis 付款事件，驗證資料流完整性。

### 4. 核心邏輯驗證
- [x] **驗證命名轉換**：確保 `EsmmShipNo` -> `esmm_ship_no` 的映射準確無誤。
- [x] **驗證原子更新**：MongoDB 指令必須在單次操作中完成 `$set` 與 `$push`。

## 檢核點
- [x] 沙盒執行後，輸出的 MongoDB Bson 與 Elastic DSL 必須與資料流檔案中的「更新後」JSON 結構完全一致。
- [x] `ProductionDataGenerator` 產出的測試資料需符合 2026 年的日期格式。

## 完成日期
2026-04-28
