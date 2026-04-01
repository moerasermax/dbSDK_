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

### 模組依賴關係 (Dependency Graph)
```mermaid
graph TD
    Sample(Sample / Entry Point) -->|DI Injection| Core.Services[Core.Services / Application]
    Sample -->|Registration| Infrastructure
    Core.Services -->|Define| Core.Interfaces[Core.Interfaces / Models]
    Infrastructure -- Implement / DIP --> Core.Interfaces
