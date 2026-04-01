# 🚀 Data Engine SDK 

[![Architecture: Clean](https://img.shields.io/badge/Architecture-Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Pattern: DDD](https://img.shields.io/badge/Pattern-DDD-green)](#)
[![Security: Audited](https://img.shields.io/badge/Security-Audited-red)](#)
[![Status: Refactoring](https://img.shields.io/badge/Status-Refactoring-orange)](#)

這是一個以 **Clean Architecture** 與 **Domain-Driven Design (DDD)** 為核心設計思想的資料處理 SDK。旨在提供高可擴展、低耦合的跨資料庫（MongoDB, Elasticsearch, Redis）統一操作介面。

---

## 🏛️ 系統架構 (System Architecture)

本專案遵循 **DIP (依賴反轉原則)**，確保核心業務邏輯與基礎設施實作徹底解耦。

### 層級劃分：
* **Core Layer (核心層)**: 定義系統合約 (Interfaces) 與業務模型 (Models/Entities)。嚴禁依賴任何外部 SDK 或第三方工具。
* **Application Layer (SDK Engine)**: 負責協調業務流程。原 `dbSDKEngine` 應位於此層級，作為 SDK 的主要入口點。
* **Infrastructure Layer (基礎設施層)**: 負責技術實作。包含資料庫驅動 (Drivers)、倉儲實作 (Repositories) 以及第三方 SDK 的封裝。
* **Composition Root (Sample/Entry)**: 負責依賴注入 (DI) 的組裝、憑證配置與系統啟動。

---

## 🛠️ 設計模式與 SOLID 實踐

在架構設計中，我們嚴格執行以下原則以確保系統的長久可維護性：

| 模式/原則 | 應用位置 | 設計意圖 (Design Intent) |
| :--- | :--- | :--- |
| **DIP 依賴反轉** | `IRepository<T>` | 核心層定義規格，Infrastructure 負責實作，更換資料庫無需修改業務邏輯。 |
| **Strategy 策略模式** | `dbDriver` | 透過抽象類別統一不同驅動 (Elastic/Mongo/Redis) 的行為。 |
| **Facade 外觀模式** | `dbSDKEngine` | 提供開發者單一且簡單的調用入口，隱藏底層驅動複雜性。 |
| **OCP 開閉原則** | `IEngine<T>` | 新增資料庫支援時，只需增加新的實作類別，不需更動核心代碼。 |
| **LSP 里氏替換** | `IRead/Write` | (重構中) 確保子類別實作不破壞父類別介面期待。 |

---

## 📋 重構優化路線圖 (Refactoring Roadmap)

基於架構審計報告，目前系統正處於重構階段，優先處理以下項目：

### 1. 核心功能與安全性修正 (P0)
- [ ] **Result Persistence**: 修正 `IEngine` 介面，將 `Task` 改為 `Task<IResult>`，解決回傳值丟失問題。
- [ ] **Async Integrity**: 修正 `MongoRepository` 等實作中缺失的 `await` 調用，確保執行緒非同步安全。
- [ ] **Credential Security**: 移除代碼中的硬編碼憑證 (Passwords/Keys)，改由配置檔或環境變數注入。

### 2. 架構層級重整 (P1)
- [ ] **Layer Re-alignment**: 將 `dbSDKEngine` 從 Infrastructure 移回 Core/Application 層級。
- [ ] **Domain Model Extraction**: 將分散在各處的 Entity (`Order`, `Query` 等) 統一收納至 `Core.Models`。
- [ ] **Interface Segregation**: 拆分 Read 與 Write 介面，解決 Redis 違反 LSP 的問題。

### 3. 代碼質量維護 (P2)
- [ ] **Thread-Safe Singleton**: 移除靜態欄位的手工 Singleton，改用 DI 容器管理生命週期。
- [ ] **Encapsulation**: 修正 `Result` 的訪問權限，確保作為 SDK 發佈時的型別可見性。

---

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
