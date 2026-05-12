# Sprint S36：Sandbox 測試腳本整合 (Consolidation)
tracking_label: P2-REF-1

## 任務目標
將 `CPF.Sandbox/Scenarios/` 目錄下零散的 S23-S29 腳本整合進單一類別 `SearchScenarioSuite.cs`，提升程式碼整潔度並減少檔案數量。

## 需求背景
目前的 Sandbox 測試腳本採用「一功能一檔案」模式，導致檔案結構過於臃腫，且每個檔案都重複包含相似的 `Check` 方法與輸出格式。透過整合成單一 Suite 類別，可以共用輔助工具並讓專案結構更簡潔。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：方案符合整潔代碼原則，不影響現有驗證邏輯 (已簽署 2026-05-04)
- [ ] **Engineer**：待工程師審核實作可行性
- **核准日期**: 2026-05-04
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] **建立 Suite 類別**：在 `CPF.Sandbox/Scenarios/` 建立 **`P2_SearchScenarioSuite.cs`**。
    - **命名規範**：因 S23-S30 屬於同一批 Search 功能優化，故在類別與檔案前標註中階層標籤 `P2_`。
- [x] **遷移與整合 S23-S30 邏輯**：
    - 將 `S23_GetHomeToDoOverViewScenario.cs` 等檔案整合。
    - **溯源標註**：在每個方法上方使用 XML 註解標註其原始 Sprint（例如：`/// <summary> [S23] Search 1 ... </summary>`）。
    - 方法命名對齊 Search 編號：`RunSearch1Async()` 到 `RunSearch7Async()`。
- [x] **簡化 Program.cs 入口點**：
    - 更新呼叫對象為 `P2_SearchScenarioSuite`。
    - 統一使用 `P2_` 前綴以利於未來「自我升級」時的角色與任務識別。

---

## PM 驗收項目 (VCP)

### 1. 功能等價驗證
- 依序執行 `dotnet run --project CPF.Sandbox dump-s1` 到 `dump-s7`。
- 驗證輸出內容（包含 PASS/FAIL 檢核點）與重構前完全一致。

### 2. 架構整潔度
- `CPF.Sandbox/Scenarios/` 檔案數量應顯著減少。
- 無重複的 `Check` 方法實作。

---

## 技術檢核點
- [ ] 專案編譯成功。
- [ ] `SearchScenarioSuite` 內的方法名稱應具備描述性（例如 `RunS23_HomeOverviewAsync`）。
