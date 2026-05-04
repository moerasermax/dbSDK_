# [P2] SDK 格式與資料全對齊 (Phase 2.A Master Tracker)

> **需求起源**：依據 HANDOFF_4 及客戶 GoldenRecipe 實測對比，修正 SDK 輸出之「物理形狀」與「資料深度」差異。
> **總體狀態**：⏳ 重新啟動 (0/6 完成)
> **目標**：確保 SDK 輸出與金標 100% 對齊（含 Wrapper、Casing、資料補強），通過 V2 PM 驗收。

---

## 📋 任務進度清單 (Checklist)

| 系列標籤 | Sprint 編號 | 任務名稱 | 負責人 | 狀態 | VCP 快速摘要 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **P2-1** | **S31** | 外層信封與全局清理 | Claude | [ ] Todo | 建立 ApiResponseWrapper + 移除 Took 欄位 |
| **P2-2** | **S32** | 容器結構與 Casing 修正 | Claude | [ ] Todo | S1/S4 改單一物件 + S1/4/5/6/7 全面 camelCase |
| **P2-3** | **S33** | 趨勢補零與 RankingNo | Claude | [ ] Todo | S5/S6 補足 24h/7d bucket (含 0) 與序號 |
| **P2-4** | **S34** | Mongo 深度整合 (S2/S3) | Claude | [ ] Todo | 二階段查詢：ES 篩選 -> Mongo 補強 12 表深度資料 |
| **P2-5** | **S37-40** | 測試體系對齊 V2 | Claude | [ ] Todo | 更新 30 筆與 100 筆測試腳本及預期值 (符合 V2 Shape) |
| **P2-6** | **V2** | PM 最終驗收 | Gemini | [ ] Todo | 物理形狀 (JSON Schema) 與 數值 (Values) 雙對齊 |

---

## 🛡 跨任務共通鐵律 (Shared Invariants)
- [ ] **Contract Integrity**：以 `.gemini/Sample_Data/CUN9101/` 下之金標 Out 為最高準則。
- [ ] **CamelCase**：所有對外 Model 屬性必須透過 `[JsonPropertyName]` 映射為小寫開頭。
- [ ] **Data Depth**：Search_2/3 必須具備 12 表完整 Join 結構，不允許僅回傳 ES 扁平欄位。
- [ ] **Zero Padding**：趨勢圖必須確保時間軸連續，無資料之 bucket 必須填 0。

---
*校正紀錄：2026-05-03 撤回 No-Wrapper/No-Casing 限制，恢復全對齊路徑。*
