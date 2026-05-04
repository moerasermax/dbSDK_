# [P2] SDK 資料與結構對齊 (Phase 2.A Master Tracker - 修正版 v2)

> **需求起源**：對齊 GoldenRecipe 之「內層資料結構」與「資料深度」。
> **變更紀錄**：2026-05-04 依 User 指示，**移除外層 ApiResponseWrapper 實作任務**。SDK 僅負責產出正確的內層資料，外層包裹由客戶端自行處理（`result.Data = ...`）。
> **總體狀態**：⏳ 重新啟動 (0/5 完成)

---

## 📋 任務進度清單 (Checklist)

| 系列標籤 | Sprint 編號 | 任務名稱 | 負責人 | 狀態 | VCP 快速摘要 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **P2-1** | **S31** | 撤銷 Wrapper 與偵錯工具恢復 | Claude | [ ] Todo | 移除 BLL Wrapper 回傳，恢復 `dump-sN` |
| **P2-2** | **S32** | 內層容器結構與 Casing 修正 | Claude | [ ] Todo | S1/S4 改單一物件 + 全面對齊 camelCase |
| **P2-3** | **S33** | 趨勢補零與 RankingNo | Claude | [ ] Todo | S5/S6 補足 24h/7d bucket 與序號 |
| **P2-4** | **S34** | Mongo 深度整合 (S2/S3) | Claude | [ ] Todo | 二階段查詢：ES 篩選 -> Mongo 補強 12 表深度資料 |
| **P2-5** | **V2** | PM 最終驗收 (內層資料對齊版) | Gemini | [ ] Todo | 驗證內層 JSON Schema 與 數值對齊 |

---

## 🛡 跨任務共通鐵律 (Shared Invariants)
- [ ] **Inner Shape Only**：僅對齊 `data` 節點內部的結構（camelCase、單物件型態）。
- [ ] **No SDK Wrapping**：BLL 方法回傳業務 Model 本身，不可包含外層信封。
- [ ] **Data Depth**：Search_2/3 必須從 Mongo 補齊 12 表完整結構。

---
*校正紀錄：2026-05-04 依 User 最新指令，取消 ApiResponseWrapper 任務，由客戶自行包裹。*
