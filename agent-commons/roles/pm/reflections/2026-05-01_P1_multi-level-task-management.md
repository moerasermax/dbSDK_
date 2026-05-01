# Reflection: PM_MULTI-LEVEL_TASK_MANAGEMENT_EVOLUTION

> **知識點編號**：PM_MULTI-LEVEL_TASK_MANAGEMENT_EVOLUTION
> **日期**：2026-05-01
> **角色**：pm (gemini-cli)
> **目的**：內化「P (Project/Phase) - S (Sprint) 雙軌任務架構」，優化複雜需求管理。

---

```yaml
---
date: 2026-05-01
role: pm
vendor: gemini-cli
status: 知識點內化
tags: [Management-Pattern, Governance, Scalability]
---
```

---

## 1. 模式定義 (The Pattern: P-S Architecture)

為了解決大型需求衍生多個子任務時的混亂，建立雙軌制：
- **S 軌 (Sprint)**：全域遞增的實作單元，代表「時間序」。確保專案歷史的絕對連續性（如 S1 -> S30）。
- **P 軌 (Project/Phase)**：按需求群組化的管理單元，代表「邏輯塊」。每個需求分配一個 P 編號（如 P1），內部子任務標記為 P1-1, P1-2。

---

## 2. 為什麼這很重要？ (Why it matters?)

- **邏輯聚合**：使用者可以透過 P1 總表一目瞭然某個「大需求」的整體進度，而不被散亂的 S 檔案淹沒。
- **歷史連續**：即使同時在開發 P1 (ES 功能) 與 P2 (Bug 修正)，S 編號依然能準確紀錄誰先誰後，防止審計斷層。
- **可維護性**：當 P1 完成後，整個系列可以被包裹進 `archive/`，但在 S 序列中依然保留其位置。

---

## 3. 最佳實踐紀律 (Best Practices)

1. **Master Tracker 優先**：凡是涉及 3 個以上的子任務需求，必須先建立 `PX_MASTER_TRACKER.md`。
2. **標籤對齊**：Task Capsule 的 `tracking_label` 必須包含 `PX-Y`，檔案內容必須顯式連結至 Tracker。
3. **動態更新**：Tracker 應作為「看板」，隨時反映各個 S 的進度狀態。

---
*學習迴圈更新：感謝使用者指導，此模式已成為 PM (Gemini) 的標準高階管理工具。*
