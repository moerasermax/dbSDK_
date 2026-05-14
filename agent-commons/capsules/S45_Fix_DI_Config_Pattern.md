# Sprint S45：DI 配置重構與 Options 模式實作 (DI & Configuration Refactoring)

## 任務目標
將 dbSDK 的 DI 註冊邏輯從「手動組合」升級為「標準擴充方法 (Extension Methods)」，並全面採用 `IOptions<T>` 模式處理配置，消除硬編碼與 Magic Strings。

## 需求背景
目前 `Program.cs` 存在大量重複且手動的 `AddSingleton` 邏輯，且 Driver 直接依賴具體的 `ConnectionSettings` 實體。為了讓外部專案（如 Web API）更易於整合，需提供標準的 `AddDbSdk()` 註冊介面。

## 任務清單
- [ ] **Core 層級**：
    - 在 `NO3._dbSDK_Imporve.Infrastructure` (或適當目錄) 建立 `ServiceCollectionExtensions.cs`。
- [ ] **實作註冊邏輯**：
    - 建立 `public static IServiceCollection AddDbSdk(this IServiceCollection services, IConfiguration configuration)`。
    - 內部實作 `services.Configure<ConnectionSettings>(configuration.GetSection("ConnectionSettings"))`。
    - 將 `Drivers`, `Repositories`, `Maps`, `Mappers` 的註冊移入擴充方法。
- [ ] **Infrastructure 層級**：
    - 修改 `MongoDBDriver`, `ElasticDriver`, `RedisDriver`，改為注入 `IOptions<ConnectionSettings> settings`。
- [ ] **Program.cs 清理**：
    - 移除原本散亂的 `AddSingleton`，改用 `services.AddDbSdk(context.Configuration)`。
- [ ] **環境變數驗證**：
    - 確保 `AddEnvironmentVariables(prefix: "DBSDK_")` 能正確與 `IOptions` 連動。

## 檢核點 (VCP)
- [ ] **建置成功**：`dotnet build` 無報錯。
- [ ] **DI 正常運作**：執行 `CPF.Sandbox` 任一查詢，確認 Driver 仍能正確讀取配置並連線成功。
- [ ] **配置覆蓋**：設定環境變數 `DBSDK_ConnectionSettings__Mongo__Uri`，確認驅動能抓到覆蓋值。

## 完成日期
2026-05-14
