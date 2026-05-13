# Sprint S45：修正 DI 註冊範例（改採 appsettings.json 配置）
tracking_label: P2B-DOCUMENT-3 / S45

## 任務目標
修正 `docs/SDK_QuickStart.md` 與 `CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs` 中的 DI 註冊範例，將硬編碼 (Hardcoded) 的連線字串改為從 `appsettings.json` 讀取並綁定 (Bind) 的標準方式。

---

## 需求背景
目前教學範例直接在程式碼中 new `ConnectionSettings` 並填入字串，這不符合 .NET 最佳實踐，也與本專案 `appsettings.json` 的現有結構脫節。
客戶端工程師需要看到如何正確使用 `IConfiguration` 來載入配置。

---

## 任務清單

### 1. [ ] 更新 docs/SDK_QuickStart.md
- **模組 A 與 B** 的註冊範例，加入 `IConfiguration` 綁定邏輯：
    ```csharp
    var settings = new ConnectionSettings();
    configuration.GetSection("ConnectionSettings").Bind(settings);
    ```
- 說明如何從 `appsettings.json` 對應到 `ConnectionSettings` 類別。

### 2. [ ] 更新 CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs
- 在 `BuildSearchService` 與 `RunUpdateExample` 中，加入 `ConfigurationBuilder` 讀取 `appsettings.json` 的邏輯。
- 展示如何將讀取到的 `settings` 物件傳遞給 Driver。

---

## PM 驗收項目 (VCP)

| # | 驗證項目 | 驗證方式 | 期望值 |
|---|---------|---------|--------|
| 1 | **配置解耦** | Code Review | 範例程式碼中不再出現 hardcoded 的 IP 或連線字串 |
| 2 | **綁定正確性** | Code Review | 註冊範例包含 `.GetSection("ConnectionSettings").Bind(settings)` 關鍵動作 |
| 3 | **執行驗證** | teaching 模式 | 執行 `teaching` 模式時，能正常從 Sandbox 根目錄的 `appsettings.json` 讀取值並顯示預覽 |

---

## 技術檢核點
- [ ] 確保 `using Microsoft.Extensions.Configuration;` 已加入。
- [ ] 提醒使用者 `appsettings.json` 需設為「固定複製到輸出目錄」。
