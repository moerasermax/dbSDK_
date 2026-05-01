# Sprint S12：建立 CPF 全域模擬沙盒專案 (Global Sandbox)

## 任務目標
建立獨立的 `CPF.Sandbox` 專案，作為 SDK 與 CPF 各服務之間的「整合驗證中心」，支援使用生產級 Model 進行全流程 Mock 模擬。

## 需求背景
為了避免生產專案 (Redis.Post, SendDataToMongoDB) 包含過多測試代碼，且為了解決跨專案 Model 引用的複雜性，決定建立獨立的沙盒專案，提供「資料產生 -> 映射轉換 -> 指令產出」的完整驗證鏈。

## 任務清單

### 1. 建立專案結構 (Infrastructure)
- [x] **新增 Console 專案**：`CPF.Sandbox/CPF.Sandbox.csproj`（net8.0 Console）。
- [x] **配置專案引用**：`NO3._dbSDK_Imporve`、`CPF.Service.SendDataToMongoDB`、`CPF.Service.SendDataToElasticCloud`、`CPF.Services.Redis.Post`。
- [x] **更新解決方案**：已加入 `NO3._dbSDK_Imporve.slnx`。

### 2. 資料產生器遷移與升級 (Infrastructure / Application)
- [x] **建立 ProductionGenerator**：`CPF.Sandbox/Generators/ProductionDataGenerator.cs`，整合 `AddOrderEventRandomDataGenerator`，直接產出 `SendDataToMongoDB.Model.Order.OrderModel` 與 `SendDataToElasticCloud.Model.OrderInfoModel`。
- [x] **支援模組化**：包含 `E_Shipment_M`、`E_Shipment_L`、`E_Shipment_S` 物流子模型，以及 `UpdateSellerMemo` 與 `ChangePayType` 局部更新 patch 產生器。

### 3. 沙盒場景實作 (Presentation)
- [x] **建立 SandboxRunner**：`CPF.Sandbox/Scenarios/SandboxRunner.cs`，實作四個場景並輸出驗證結果。
- [x] **主程式**：`CPF.Sandbox/Program.cs`，初始化序列化器後呼叫 `SandboxRunner.RunAll()`。

## 檢核點
- [x] `CPF.Sandbox` 建置成功（0 錯誤）。
- [x] 成功產出正式 `OrderModel` 的 MongoDB BsonDocument（含點符號路徑與 null 排除）。
- [x] 成功產出 `OrderInfoModel` 的 ElasticSearch 映射 JSON。

## 完成日期
2026-04-24
