# 🚀 Enterprise Solution Refactoring Project

[![Build Status](https://img.shields.io/badge/Build-Passing-success?style=flat-square&logo=github)]()
[![Framework](https://img.shields.io/badge/.NET-8.0+-512BD4?style=flat-square&logo=dotnet)]()
[![Architecture](https://img.shields.io/badge/Architecture-Clean_Architecture-blue?style=flat-square)]()
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)]()

這是一個基於 **Clean Architecture** 與 **Domain-Driven Design (DDD)** 核心思想重構的現代化開發框架。專案旨在解決舊有系統中的非同步安全隱患、高度耦合與例外處理不一致等痛點。

---

## 🏗️ Architecture Overview

本專案遵循 **Dependency Inversion Principle (DIP)**，確保核心邏輯不依賴於外部框架或資料庫實作。

### 📂 Layering Strategy
* **Core (Domain Layer)**: 包含核心實體 (`Entity`)、`IResult` 定義及領域介面。不依賴任何外部套件。
* **Application (Use Case Layer)**: 包含 `dbSDKEngine` 等業務邏輯實作。負責編排 (Orchestration) 領域物件，透過介面與 Infrastructure 溝通。
* **Infrastructure (Persistence & External Services)**: 負責 `MongoRepository`、`RedisRepository` 等具體實作。處理技術細節如持久化、快取與第三方 SDK。
* **Presentation / Client**: 啟動進入點。目前正進行從 `Manual Instantiation` 遷移至 `Dependency Injection` 的轉型。

---

## 🛠️ Design Patterns & Principles

| 模式 / 原則 | 應用位置 | 設計意圖 |
| :--- | :--- | :--- |
| **Result Pattern** | `Task<IResult>` | 取代 Exception-based 流程控制，提供可預測的 Immutable 執行結果。 |
| **Repository Pattern** | `MongoRepository`, `RedisRepository` | 抽象化資料存取邏輯，實現 Data Store 的抽換彈性。 |
| **Dependency Inversion** | `IEngine`, `IRepository` | 高層模組不應依賴低層模組，雙方皆應依賴於抽象。 |
| **Static Factory** | `Result.Success()`, `Result.Failure()` | 優化物件建立過程，強制執行結果物件的封裝性。 |
| **LSP Compliance** | `RedisRepository` 重構 | 修正子類行為不一致問題，確保回傳值符合介面契約而非拋出非預期例外。 |

---

## 🗺️ Project Roadmap

### 🔴 P0: Critical & Security (Focus: Stability)
- [x] **Result Persistence**: 修正 `IEngine` 介面，全面回傳 `Task<IResult>`。
- [x] **Async Integrity**: 補全非同步方法中的 `await` 關鍵字，排除 Thread-safety 隱患。
- [x] **Result Refactoring**: 實作不可變 (Immutable) 的 `Result` 物件。
- [ ] **Deployment Security**: 導入 `User Secrets` 或環境變數，將 `appsettings.json` 敏感資訊隔離。

### 🟡 P1: Architectural Evolution (Focus: Decoupling)
- [x] **Layer Re-alignment**: 重新分配專案目錄結構，確保 Entity 回歸 Core 層。
- [x] **Interface Segregation**: 移除特定 Repository 對 Exception 的依賴。
- [ ] **Composition over Inheritance**: 重構 Sample 層，移除對 Infrastructure 的直接繼承耦合。
- [ ] **DI Container Integration**: 導入 `Microsoft.Extensions.DependencyInjection` 全面接管生命週期。

### 🟢 P2: Maintenance & Quality (Focus: Clean Code)
- [x] **Encapsulation**: 優化屬性存取權限 (Internal/Public) 控制。
- [ ] **Thread-Safe Singleton**: 移除手寫 Singleton 模式，改用 DI Container 單例注入。
- [ ] **Dead Code Cleanup**: 清理 `Program.cs` 殘留死碼與未使用的命名空間。
- [ ] **Development Sandboxing**: 將 `#region` 內的測試邏輯轉化為正式的單元測試 (Unit Tests)。


## 🚀 快速開始 (Quick Start)

```csharp
// 1. 配置資料庫連接資訊 (建議由配置檔注入)
var dbInfo = new dbInfo("Your_Connection_String");

// 2. 初始化引擎 (建議透過實踐 DI 注入)
// 目前架構正從繼承轉向組合 (Composition)
var engine = new dbSDKEngine<Order>(new MongoRepository<Order>(...));

// 3. 執行業務操作並接收結果
var result = await engine.Read("filter-condition");

if(result.IsSuccess) {
    Console.WriteLine(result.Message);
}
