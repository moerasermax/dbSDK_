---
status: USER-RATIFIED
mutability_default: APPEND-ONLY
created_by: user-ratified-from-ai-draft
created_at: 2026-05-01
---

# dbSDK 開發鐵律 (Domain Axioms) - V1.1 (完整版)

> 版本：v1.1 · USER-RATIFIED · 整合架構基礎、歷史憲章與工程師技術對齊
> 位階：本文件為本專案最高且唯一不可妥協之領域底線。
> 結構：架構基礎 / Part I 鐵律（不可妥協）/ Part II 紀律（應遵守）/ 待辦追蹤

---

## 🏗 架構基礎（Architecture Foundation）

本專案採用 **Clean Architecture + DDD + Design Patterns** 作為統一開發模式。

### 1. 層次結構與依賴方向
```
Core              ──→  零外部依賴（不得引用 MongoDB.Driver、Elastic、Redis）
  ↑
Infrastructure    ──→  實作 Core 介面（Driver、Repository、Serializer、Mapper）
  ↑
Application (BLL) ──→  業務流程、DTO、Result 邊界（組合 Core + Infrastructure）
  ↑
Delivery (Worker) ──→  BackgroundService，依賴介面不依賴具體類別
```
**依賴方向只能向上，禁止反向或跨層。**

### 2. DDD 戰術設計與業務封裝
- **Entity**: 具有唯一識別符，屬性可變但應封裝業務邏輯。**嚴禁將純業務邏輯寫在 Service 中**。
- **Value Object**: 不可變資料結構（如 `Condition`, `Result`），保證執行緒安全。
- **Aggregate Root**: 定義一致性邊界，確保內部實體狀態正確。
- **Result 邊界**: **BLL 為統一的 Result 邊界**。Infrastructure/DAL 拋出的 Exception 應在 BLL 捕捉並轉化為 `Result.SetErrorResult()`。

### 3. 技術債標記（Architecture Debt）
- **[債] IRepository JSON 洩漏**: `IRepository<T>` 目前接收 `string ConditionData_Json` 屬技術債，應避免在介面暴露技術細節。
- **[規範]**: 新功能開發（如 S23+）應優先使用 `AdvancedSearchAsync` 強型別介面。

---

## 🛡 Part I — 鐵律（不可妥協底線）

違反任一條 → 生產環境崩潰、資料損壞或架構崩解。

### A. 兩階段靜態初始化
系統啟動必須確保正確順序：
1. `MongoSerializationConfig.Register()`
2. `MongoMap.EnsureClassMapsRegistered()`
> **驗證**：檢查 `Program.cs` 的 DI 設定，確認 `MongoMap` 註冊為 singleton（自動觸發註冊），或在 Sandbox 頂部確認兩行呼叫順序。

### B. 日期讀寫雙軌制
日期轉換必須同時覆蓋讀取與寫入路徑：
- **讀取**：由 `MultiCultureDateTimeSerializer` 處理，必須支援「上午/下午」及多語系格式。
- **寫入**：由 `MongoCommandBuilder.Flatten` 自動觸發 `TryConvertToBsonDateTime` 轉 UTC。
> **驗證**：確認 `Flatten()` 方法內含日期轉換檢查，且 Serializer 支援 19 種以上格式。

### C. 扁平化 $set 更新
MongoDB 更新強制使用 `MongoCommandBuilder.Flatten()`，將巢狀物件轉為點符號路徑，且 **null 欄位必須被忽略**（防止覆蓋舊值）。
> **驗證**：`MongoRepository.cs` 的 `UpdateData` 實作必須呼叫 `FlattenBsonDocument()`。

### D. 複合操作原子性
涉及 $set + $unset + $push 的操作必須使用 `MongoUpdateOptions` 的 `UpdateData` 重載一次執行，嚴禁拆分為多次獨立呼叫。
> **驗證**：檢查 `Worker.cs` 支付變更邏輯，確認使用 `UnsetFields`。

### E. 查詢條件不可變性
所有查詢主鍵（如 `_coom_no`）在建構後嚴禁被修改。屬性應僅具備 `get` 存取子。
> **驗證**：檢查 `CRUD_Condition_COOM` 等類別，主鍵欄位應無 `set`。

### F. 層次邊界不可越
`Core` 層嚴禁引用任何外部 NuGet 套件（MongoDB, Elastic 等）。
> **驗證**：檢查 `Core/` 目錄下的 `.csproj` 引用。

---

## 📜 Part II — 開發紀律（工程規範）

### P1. Git & Commit 規範
- **分支**: `AI_Dev` 為主開發，功能開發使用 `feature/*`。
- **格式**: `<type>(<scope>): <subject>` (例如: `feat(dal): add search 7 implementation`)。
- **Valid Scopes**: `core`, `infra`, `bll`, `dal`, `model`, `elastic`, `mongo`, `charter`。

### P2. Sprint 文件標準 (Task Capsule)
所有任務膠囊檔案必須包含以下結構：
1. **任務目標**: 最終交付價值。
2. **需求背景**: 說明為何執行。
3. **任務核准狀態 (Co-sign)**: 記錄工程師對膠囊與 VCP 的審核結果（必填）。
4. **任務清單**: 使用 `[ ]` 分層描述實作步驟。
5. **檢核點 (VCP)**: 具體驗證標準。

### P4. 多級任務管理體系 (P-S Architecture)
當單一需求衍生多個子任務時，必須執行雙軌追蹤：
- **S (Sprint)**: 全專案遞增編號，記錄時間序。
- **P (Project/Phase)**: 需求群組化編號，透過 `PX_MASTER_TRACKER.md` 進行統整檢核。
- **標籤**: 每個 Sprint 應標註其在系列中的位置（如 P1-1）。

### P3. 偵錯優先指令預覽
執行複雜更新前，應先呼叫 `UpdateInit` 預覽 JSON 指令，確認 Filter 與 Update 符合預期。

---

## 📈 待辦事項優先級 (Milestone Tracking)

| 優先級 | 項目 | 狀態 |
|--------|------|------|
| P0 | DI 改用 IHost Builder | ✅ 已完成 |
| P0 | Repository 改組合模式 | ✅ 已完成 |
| P1 | ElasticFilter 強型別 | ✅ 已完成 (v1.1 更新) |
| P1 | BLL 統一入口點重構 | 🛠 S23-S30 執行中 |
| P2 | 憑證環境變數化 | ⏳ 待處理 |

---
*校正紀錄：2026-05-01 整合工程師 Claude 對 Result 邊界與 Commit Scope 的回饋，並恢復 V0.5 架構基礎資產。*
