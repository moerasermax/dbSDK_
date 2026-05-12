# Reflection: [F1-20260512-01] Search 7 資料源前提誤判事故

- **日期**：2026-05-12
- **角色**：pm (gemini-cli)
- **類型**：F1 (Logic/Assumption Error)
- **失效 ID**：F1-20260512-01

## 1. 事故描述 (Incident Review)
在建立資料源修正膠囊時，我基於「代碼平行轉移」的邏輯，直接宣告「應將此邏輯平行轉移至 MongoDB」，卻未事先實證測試資料中是否真的包含 User 相關資料。隨後工程師透過 `grep` 實證該檔案並無相關資料，導致膠囊內容與事實脫節。

## 2. 根因分析 (Root Cause Analysis)
1. **實證缺失 (Evidence Gap)**：過度依賴邏輯推演（DDB 轉 Mongo），忽略了「Data-First」的開發原則。
2. **前提跳躍 (Assumption Jump)**：未能在發布 Directive 前執行物理層面的資料抽驗（grep）。

## 3. 預防與紀律 (Prevention & Discipline)
- **L1 紀律**：在涉及「資料源轉換」的膠囊中，發布前必須親自或指示工程師執行 `grep` / `wc` 實證資料存在。
- **L2 行為修正**：在膠囊中應顯式區分「實證事實 (Fact)」與「假設前提 (Assumption)」。

---
*學習迴圈更新：感謝工程師退稿糾正，已將「資料先行實證」納入 PM 預檢清單。*
