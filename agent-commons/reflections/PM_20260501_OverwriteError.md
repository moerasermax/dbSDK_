# Reflection: Avoid Destructive Overwrites in Protocol Updates
DATE: 2026-05-01
FAIL_ID: F2-20260501-01

### 1. 發生了什麼？ (What happened?)
在試圖將工程師 Claude 的技術建議整合進 `DBSDK.md` 時，我「以為」只需要保留最新協議，而忽略了該文件同時也擔任「架構教育」的功能。我使用了 `write_file` 覆蓋了原本長達 200 多行的文件，僅留下 50 幾行的摘要。

### 2. 我學到了什麼？ (What did I learn?)
- **知識的密度不等於價值**：對於 PM 角色來說，架構基礎與驗證步驟雖然冗長，卻是維持工程穩定性的關鍵。
- **Summarization 是危險的**：在更新「合規性文件」時，Summarization 等於「刪減法律條文」。

### 3. 未來如何改進？ (Remediation)
- **執行「結構化合併」**：在更新 `.md` 文件前，先識別出文件中的 `##` 標題塊。確保更新後的版本，原本存在的關鍵標題塊（如 Architecture Foundation）依然存在。
- **顯式檢查**：在 `write_file` 後，主動向使用者確認：「我已保留了原有的架構基礎段落，僅針對 P1-P6 進行修訂。」

---
*學習迴圈更新：已將「增量合併」納入 PM 核心工作流。*
