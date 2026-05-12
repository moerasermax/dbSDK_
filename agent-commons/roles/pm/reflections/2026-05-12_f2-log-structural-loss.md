# Reflection: [F2-20260512-02] F1 entry 寫入時意外覆寫鄰近 entry header 事故

- **日期**：2026-05-12
- **角色**：pm (gemini-cli)
- **類型**：F2 (Logic/Architecture Regression) / F6 (Structural Loss)
- **失效 ID**：F2-20260512-02

## 1. 事故描述 (Incident Review)
在修補 `failure_mode_log.md` 以加入 F1 紀錄時，由於 `replace` 工具的 `old_string` 定位範圍過大或不精確，意外將緊鄰的 `## [F3-20260508-01]` 標題行覆寫，導致舊紀錄變成孤兒段落。

## 2. 根因分析 (Root Cause Analysis)
1. **工具精度失控 (Tool Precision)**：在編輯 Append-only 文件時，未採用最安全的「結尾追加」策略，而是試圖在中間進行區塊替換，且未仔細核對 `old_string` 的邊界。
2. **紀律遺忘**：違反了日誌「僅能追加、禁止修改舊標題」的核心管理規則。

## 3. 預防與紀律 (Prevention & Discipline)
- **L1 策略修正**：對於 `failure_mode_log.md` 等 Append-only 文件，優先使用能確保「僅在文末新增」的操作模式。
- **L2 寫入後檢驗**：執行 `replace` 後，應立即掃描標題塊數量，確保核心結構無損。

---
*學習迴圈更新：此事故再次印證了 PM 在處理長文檔時的結構性盲點，已強化標題塊檢查意識。*
