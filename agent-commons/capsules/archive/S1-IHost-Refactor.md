# Sprint S1：DI 改用 IHost Builder 重構

## 任務目標
將 `Program.cs` 的 DI 組裝方式從手動 `ServiceCollection` 重構為標準的 `.NET Generic Host (IHostBuilder)` 模式。

## 需求背景
目前專案手動建立 `ServiceCollection` 並呼叫 `BuildServiceProvider`，這導致：
1. 無法輕易整合 `.NET` 標準的 `appsettings.json` 配置系統。
2. 缺乏標準的日誌 (Logging) 與生命週期管理。
3. 隨著專案擴大，手動組裝會變得難以維護。
使用 `IHost` 可以讓專案更貼近生產環境標準，並簡化後續功能（如環境變數化）的實作。

## 任務清單
- [ ] 引用 `Microsoft.Extensions.Hosting` 套件。
- [ ] 在 `Program.cs` 建立 `CreateHostBuilder` 方法。
- [ ] 將原本在 `Program.cs` 的 DI 註冊邏輯移至 `ConfigureServices` 中。
- [ ] 重構入口點，透過 `host.Services.GetRequiredService` 啟動測試流程。
- [ ] 確保原本的 `TestFlow` 邏輯在新的 Host 架構下仍能正確執行。

## 檢核點
| 編號 | 檢核項目 | 驗收標準 |
|------|----------|----------|
| 1 | Host 初始化 | `Program.cs` 成功使用 `Host.CreateDefaultBuilder` 啟動 |
| 2 | DI 註冊完整性 | 所有原有的 Driver、Mapper、Repository 均能在 Host 中正確解析 |
| 3 | 功能回歸測試 | 執行測試流程（如 Mongo/Elastic 讀寫）結果與重構前一致 |
| 4 | 建置狀態 | **Kiro 確認程式碼可成功建置且無 Bug** |

## 預估影響範圍
- `Program.cs`
- `NO3._dbSDK_Imporve.csproj`

## 技術限制
- 需維持對現有 `appsettings.json` 的支援。
- 測試流程 `TestFlow` 暫時維持在 `Program.cs` 呼叫，不強制立即改為 `IHostedService`。

## 風險評估
| 風險 | 影響程度 | 因應對策 |
|------|----------|----------|
| DI 生命週期改變導致物件解析失敗 | 中 | 嚴格測試 `Scoped` vs `Singleton` 的使用，目前建議維持現狀。 |
| 配置讀取路徑錯誤 | 低 | 確保 `CreateDefaultBuilder` 能正確載入 `appsettings.json`。 |
