# dbSDK Failure Mode Log

> **目的**：記錄開發過程中的重大失誤、架構違規與技術事故，作為學習迴圈的輸入源。
> **紀律**：禁止刪除紀錄，僅能追加（Append-only）。

---

## [F2-20260501-01] 領域公理文件內容遺失事故

- **日期**：2026-05-01
- **角色**：PM (Gemini CLI)
- **失效模式**：F2 (Logic/Architecture Regression) & F6 (Structural Loss)
- **事故描述**：
    在執行 `DBSDK.md` 從 V1.0 升級至 V1.1 的過程中，由於過度追求內容精簡（Summarization Bias），導致 `write_file` 動作意外刪除了 V0.5 中原有的「架構基礎」、「DDD 模式對應表」與「詳細驗證步驟」。
- **根本原因**：
    採用「全盤重寫」策略而非「增量合併」策略。未在寫入前對新舊內容進行結構化對比（Diff Check），導致高價值的長期資產被視為冗餘資訊而移除。
- **後果**：
    專案喪失了重要的架構指引與驗證標準，若非使用者即時提醒，將導致後續 AI 助理（Claude）因資訊缺失而產生開發偏離。
- **補救措施**：
    1. 立即從歷史對話與 `read_file` 緩衝中恢復完整內容並重新合併（已完成）。
    2. **新增開發紀律**：更新核心文件時，必須先讀取全文字數，寫入後確認字數波動是否符合預期（或執行邏輯塊檢查）。
- **狀態**：✅ 已修復並加入學習迴圈。

---

## [F3-20260502-01] PM 角色權限邊界越權事故

- **日期**：2026-05-02
- **角色**：PM (Gemini CLI)
- **失效模式**：F3 (Role Boundary Violation)
- **事故描述**：
    在處理 Docker 環境連動時，誤將「修改配置」理解為「修改程式碼邏輯」，擅自修改了 `IDTO.cs` 與 `MongoDBDriver.cs` 等 C# 業務原始碼，違反了 PM (Gemini) 僅限於文檔與配置、禁止修改 .cs 程式碼的權限邊界。
- **根本原因**：
    對於「改動」一詞的上下文理解出現偏差，未能嚴格遵守 `management/GEMINI.md` 中定義的角色邊界限制。
- **後果**：
    在未經工程師確認的情況下變動了核心業務邏輯，破壞了專案的「分工權威性」。
- **補救措施**：
    1. 立即還原所有受影響的 `.cs` 檔案（已完成）。
    2. 更新 `docs/ENVIRONMENT_SETUP_S23_S30.md`，將環境連動邏輯引導至 `appsettings.json` 的純配置層面。
    3. **強化紀律**：在 PM 模式下，若改動對象包含 `.cs` 檔案，必須觸發強制警告並停止操作。
- **狀態**：✅ 已修復並記錄。

---

## [F2-20260503-02] JSON 輸出格式合約不匹配事故

- **日期**：2026-05-03
- **角色**：PM (Gemini CLI)
- **失效模式**：F2 (Logic/Architecture Regression)
- **事故描述**：
    在 S23-S30 的驗收過程中，PM 僅根據數值對比 (PASS/FAIL) 核准了功能，卻忽略了 JSON 的「物理形狀 (Shape)」與客戶 GoldenRecipe 合約不符。
    - 缺外層 Wrapper (ApiResponseWrapper)。
    - Object/Array 類型不對齊 (S1/4/7)。
    - 命名風格錯誤 (PascalCase vs camelCase)。
- **根本原因**：
    驗證腳本僅對比了「反序列化後」的屬性值，未執行「原始 JSON 字串」的逐字 Diff。PM 未能嚴格執行 API Contract 的高保真校驗。
- **補救措施**：
    1. 標記 V1 驗收無效。
    2. 下個 Session 優先啟動 **方案 B (格式對齊)**。
    3. **新增開發紀律**：所有 API 驗收必須包含 `ApiResponseWrapper<T>` 的實體序列化測試與 Schema 對比。
- **狀態**：⚠️ 待重構 (Phase 2.A)。

---

## [F2-20260504-01] Phase 2.A SDK 對外格式方向反覆事件

- **日期**：2026-05-04
- **角色**：PM (Gemini CLI)
- **失效模式**：F2 (Logic/Architecture Regression) — 同類偏差第 3 次
- **歷程**：
    - v1 (2026-05-03 初版 P2 tracker): No Wrapper / No Casing
        → 與 HANDOFF_4 §6.2 客戶決議反向、Engineer 退稿
    - v2 (2026-05-03 修正版): 含 Wrapper + camelCase
        → PM CO-SIGNED, Engineer 動工 4 commit (S31 v1 實作)
    - v2' (2026-05-04 再次修正): 不需 SDK Wrapper, 客戶端自包
        → User 親自裁決, Engineer revert 2 commit
- **根本原因**：
    客戶端 / SDK 對外 contract 設計議題未在 Phase 2.A 啟動前收斂；三輪迭代浪費 ~5h Engineer 工程量。
- **補救**：
    1. 後續 contract 議題在 capsule 寫入前先取得 User 明示。
    2. 重大方向變更 commit message 必引述 User 裁決原文行號。
- **狀態**：✅ 已結案。
