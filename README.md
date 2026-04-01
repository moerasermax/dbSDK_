# 🚀 Data Engine SDK

[![Architecture: Clean](https://img.shields.io/badge/Architecture-Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![Pattern: DDD](https://img.shields.io/badge/Pattern-DDD-green)](#)
[![Security: Audited](https://img.shields.io/badge/Security-Audited-red)](#)

這是一個以 **Clean Architecture** 與 **Domain-Driven Design (DDD)** 為核心設計思想的資料處理 SDK。旨在提供高可擴展、低耦合的跨資料庫（MongoDB, Elasticsearch, Redis）操作介面。

---

## 🏛️ 系統架構 (System Architecture)

本專案遵循依賴反轉原則 (DIP)，確保核心邏輯與基礎設施實作徹底解耦。

### 層級說明：
- **Core Layer**: 定義系統合約 (Interfaces) 與業務模型 (Models)。嚴禁依賴任何外部 SDK。
- **Infrastructure Layer**: 負責技術實作。包含資料庫驅動 (Drivers) 與倉儲實作 (Repositories)。
- **Application (SDK Engine)**: 負責協調業務流程，作為 SDK 的主要入口點。
- **Composition Root (Sample)**: 負責依賴注入 (DI) 的組裝與啟動。

### 模組依賴圖
```mermaid
graph TD
    Sample(Sample / Entry Point) --> Core.Services[Core.Services / Engine]
    Sample --> Infrastructure
    Core.Services --> Core.Interfaces[Core.Interfaces / Models]
    Infrastructure -- 實作 --> Core.Interfaces
