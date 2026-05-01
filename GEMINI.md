# [DEPRECATED] dbSDK 專案協定 (Project Protocols)

> **注意**：本文件已於 2026-05-01 正式廢棄。
> **真理來源**：請參閱 `agent-commons/protocols/DBSDK.md` 獲取最新的領域公理。
> **角色定義**：請參閱 `agent-commons/roles/` 獲取最新的 PM 與 Engineer 角色狀態。
> **同步協定**：請使用 `/checkpoints load` 或 `/pm-init` 等自動化指令，而非手動參考此文件。

---
(以下為歷史存根，不再更新)
---

# dbSDK 專案協定 (Project Protocols)

## 專案專有語法：`/checkpoints load`

當使用者輸入 `/checkpoints load` (或其變體如 `load new`) 時，所有 AI 助理必須執行以下 **「狀態同步協定 (State Sync Protocol)」**，不得略過任何步驟：

### 1. 讀取核心存檔 (Core Checkpoints)
- **PM 視角**：讀取 `docs/checkpoint_gemini.md` (確認解耦評分、驗收狀態、P0 事項)。
- **工程師視角**：讀取 `docs/checkpoint_kiro.md` (確認已完成 Sprint、技術決策)。

### 2. 讀取最新歷史節點 (Latest Sync Point)
- 在 `docs/checkpoints/` 中尋找編號最大的 `kiro-sync-point-XX.md`，獲取最後一次完整凍結的技術背景。

### 3. 掃描進行中的任務 (Active Sprints)
- 檢查 `docs/sprints/` 下所有尚未完成（清單中有 `[ ]`）的檔案。

### 4. 輸出狀態總結 (Final Synthesis)
- 必須以以下格式回應使用者：
  - **[當前里程碑]**：解耦評分與專案健康度。
  - **[進行中任務]**：目前正在處理的 Sprint 編號與目標。
  - **[下一步行動]**：列出 P0 與 P1 的優先順序。

---

## 協作邊界 (Operational Boundaries)

- **Gemini (PM)**: 負責文檔、架構審核、建立 Sprint。禁止修改 `.cs` 業務代碼。
- **Kiro (Dev)**: 負責實作、測試、Shell 指令。負責執行 Sprint 內容。

---

## 技術指導原則 (Architectural Mandates)

- **Native Fidelity**: 模擬測試必須直接呼叫 `Infrastructure` 下的 `public static` 核心邏輯。
- **Recursive Normalization**: 所有 Bson 處理必須支援陣列內部的遞迴日期轉型 (Sprint H2 規範)。
