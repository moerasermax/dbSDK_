# Sprint S9：安全性強化與入口點重構 (Security & Cleanup)

## 任務目標
1.  **憑證安全 (P0)**：建立環境變數讀取機制，確保敏感資訊不進入版本控制。
2.  **入口點瘦身 (P1)**：將測試邏輯與 Mock 類別從 `Program.cs` 移出，恢復單一職責原則。

## 需求背景
目前 `Program.cs` 內容過於臃腫，包含大量測試流程與臨時 Mock 類別，不利於 SDK 的長期維護。此外，憑證管理需標準化，以符合企業級開發規範。

## 任務清單

### 1. 安全性與配置優化 (Core / Presentation)
- [x] **環境變數映射**：
    - 修改 `Program.cs`，加入 `config.AddEnvironmentVariables(prefix: "DBSDK_")`。
    - 前綴為 `DBSDK_`，例如 `DBSDK_ConnectionSettings__Mongo__Uri`。
- [x] **Git 策略**：
    - 更新 `.gitignore`，加入 `appsettings.*.json` 排除規則，並以 `!appsettings.Template.json` 保留範本。
    - 建立 `appsettings.Template.json` 作為配置範本。

### 2. 測試邏輯抽離 (Application / Presentation)
- [x] **建立測試容器**：
    - 新增 `NO3._dbSDK_Imporve/Application/Sample/TestFlows.cs`。
    - 將 `TestFlow_Mongo`, `TestFlow_ShippingSync`, `TestFlow_Mock` 移至此靜態類別。
- [x] **建立 Mock 類別檔案**：
    - 新增 `NO3._dbSDK_Imporve/Application/Sample/Mongo/MockOrderRepository.cs`。
    - 將 `MockOrderData`, `MockUpdateOptions`, `MockOrderRepository` 移入此檔案。
- [x] **重構 Program.cs**：
    - 僅保留 DI 註冊、`CreateHostBuilder` 方法與最簡潔的啟動呼叫。
    - `host.Services` 透過參數傳遞至 `TestFlows.RunMongoFlow()`。

### 3. 日期序列化器調整 (Infrastructure)
- [x] 新增 `Infrastructure/Persistence/Mongo/Serialization/MongoSerializationConfig.cs`。
- [x] 將 `BsonSerializer.RegisterSerializer` 邏輯集中至 `MongoSerializationConfig.Register()`。
- [x] `Program.cs` 頂部改為單行呼叫 `MongoSerializationConfig.Register()`。

## 檢核點
- [x] 全案編譯成功（0 錯誤）。
- [x] `Program.cs` 程式碼行數從 ~250 行縮減至 ~55 行，結構清晰。
- [x] 環境變數前綴 `DBSDK_` 已設定，可覆蓋 appsettings.json 中的任何配置值。

## 完成日期
2026-04-24
