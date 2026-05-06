# PM Operational Reflections & Protocols (2026-05-04)

## 1. 核心手令協定 (Directive Header Protocol)
- **知識點**: 在多代理系統中，明確的「傳遞目標」與「結構化手令」是無縫協作的關鍵。
- **格式準則**:
---
致 [角色名稱/AI Vendor]
{任務內容}
---

## 2. 命名慣例與知識結構 (Systemic Standardization)
- **知識點**: `reflections/` 目錄並非暫存區，而是「元知識 (Meta-knowledge)」的儲存庫。
- **準則**: 必須遵循 `YYYY-MM-DD_topic.md` 格式，以利於跨 Session 的時間軸索引與機器學習。

## 3. 知識邊界區分 (Knowledge Boundary Differentiation)
- **核心學習**: 必須嚴格區分「專案狀態 (Project State)」與「元知識 (Meta-knowledge)」。
- **分類準則**:
    - **專案狀態**: 如 S36 優先級、具體 Bug 修復、測試數據狀態等。這些應保留在 `Capsule` 或 `nextwork.md` 中，隨專案結束而歸檔。
    - **元知識**: 如協作手令、命名慣例、權限硬約束、系統性溝通模式。這些應記錄在 `Reflections` 中，用於 AI 的長期自我進化與跨環境能力遷移。
- **自省**: 誤將專案決策寫入 Reflections 會導致知識庫污染，使其失去通用的學習價值。

## 4. 執行模式與邊界 (Operational Boundaries)
- **知識點**: PM 的權限邊界（禁止修改業務代碼）是為了維護架構完整性所設定的「硬約束」。
- **準則**: 所有的驗證必須建立在可量化的測試執行（Sandbox）之上，而非邏輯推演。



