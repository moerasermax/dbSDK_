# NO3.dbSDK_Improve 🚀

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![Architecture](https://img.shields.io/badge/Architecture-Clean--Architecture-blue?style=flat-square)
![Status](https://img.shields.io/badge/Status-7.5%2F10-orange?style=flat-square)

基於 **Clean Architecture** 與 **DDD (Domain-Driven Design)** 原則重構的資料庫 SDK 模型。旨在解決高耦合繼承、異步誠信與安全憑證管理問題。

---

## 🏗️ Architecture Layers

本專案嚴格遵循 **Dependency Inversion Principle (DIP)**，確保核心業務不依賴技術實作：

* **Core (Domain Layer)**
    * `Core.Models`: 定義領域實體 (Entities) 與不可變的 `ConnectionSettings`。
    * `Core.Interfaces`: 定義 `IEngine` 與 `IRepository` 抽象契約，為系統的穩定核心。
* **Infrastructure (Data Layer)**
    * 實作具體的資料庫驅動 (MongoDB, Elastic, Redis)。
    * 封裝技術細節，如 `dbDriver` 建構邏輯。
* **Application / Sample (Application Layer)**
    * 協調業務流程，目前正處於「繼承改組合 (Composition over Inheritance)」的轉型期。
* **Presentation (Client Layer)**
    * `Program.cs`: 負責 DI 容器組裝與 Entry Point 執行。

---

## 🧩 Design Patterns & Principles

| 模式/原則 | 應用位置 | 設計意圖 |
| :--- | :--- | :--- |
| **DIP (依賴反轉)** | `IEngine` 與實作類 | 確保 Application 層不直接依賴特定的資料庫實作。 |
| **Immutable Object** | `dbDriver._Service` | 使用唯讀屬性 (getter only) 防止執行時期狀態被竄改。 |
| **Repository Pattern** | `IOrderRepository` | 抽象化資料存取邏輯，支援多種存儲媒體 (Polyglot Persistence)。 |
| **Dependency Injection** | `Microsoft.Extensions.DependencyInjection` | 管理物件生命週期，取代手寫的非執行緒安全 Singleton。 |

---

## 🗺️ Project Roadmap

### 🔴 P0: Critical & Security (Focus: Stability)
- [ ] **Secret Isolation**: 移除 `appsettings.json` 中的明文憑證 (Password/ApiKey)，改由環境變數或 User Secrets 注入。
- [ ] **Result Immutability**: 完成 `IResult` 的不可變物件重構。

### 🟡 P1: Architectural Evolution (Focus: Decoupling)
- [ ] **DI Integration**: 將 `dbSDKEngine` 註冊至 DI 容器，消除 `Program.cs` 中的 `new` 關鍵字手動實例化。
- [ ] **Dead Code Cleanup**: 移除 `Program.cs` 中未使用的連線字串組裝變數 (`mongoConnStr`, `redisConnStr`)。
- [ ] **Decoupling Inheritance**: 將 `OrderRepository_Mongo` 等類別從繼承具體類別改為組合方式，徹底分離 Infrastructure。

### 🟢 P2: Maintenance & Quality (Focus: Clean Code)
- [ ] **Namespace Cleanup**: 移除 `IEngine.cs` 中無效的 `using NO3._dbSDK_Imporve.Core.Models`。
- [ ] **Singleton Refactor**: 移除 `ElasticMap` 與 `RandomDataGenerator` 的手動實作，改用 DI 容器的 `.AddSingleton()`。
- [ ] **Sandbox Removal**: 清理 `Program.cs` 中的 `#region 開發區`。

---

## 🚀 Quick Start (.NET 8.0)

### 1. 註冊服務 (Dependency Injection)
```csharp
// 在 Program.cs 中配置
var services = new ServiceCollection();

// 註冊 Repository 與 Engine (P1 待優化目標)
services.AddScoped<IOrderRepository_Mongo, OrderRepository_Mongo>();
services.AddTransient<IEngine<Order>>(sp => 
    new dbSDKEngine<Order>(sp.GetRequiredService<IOrderRepository_Mongo>()));

var provider = services.BuildServiceProvider();
