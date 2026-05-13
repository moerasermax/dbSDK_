# Sprint S44：重構串接教學場景（邏輯分段）
tracking_label: P2B-DOCUMENT-2 / S44

## 任務目標
重構 `CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs`，將查詢 (Search) 與 寫入 (Write) 的範例完全解耦並區分段落，對齊 `docs/SDK_QuickStart.md` 的結構。

---

## 需求背景
目前教學範例採用單一線性流程，不利於僅需使用特定模組的開發者閱讀。
需要將教學分為：
1. **全局初始化** (Static Init)
2. **模組 A：查詢 SDK 範例** (Search Workflow)
3. **模組 B：貨態更新範例** (Write/Update Workflow)

---

## 任務清單

### 1. [ ] 重構 IntegrationGuideScenario.cs
- 使用 `Console.WriteLine` 劃分清楚的模組邊界。
- 建立獨立的 Private Helper 方法（例如 `RunSearchExample`, `RunUpdateExample`）或明確的 Code Blocks。
- **註解強化**：在程式碼中顯式標註「模組 A 註冊」與「模組 B 註冊」的差異。

### 2. [ ] 驗證輸出格式
- 執行 `dotnet run --project CPF.Sandbox -- teaching`。
- 確保輸出內容先顯示全局初始化說明，再分別顯示兩個模組的範例。

---

## PM 驗收項目 (VCP)

| # | 驗證項目 | 驗證方式 | 期望值 |
|---|---------|---------|--------|
| 1 | **結構清晰度** | Code Review | 查詢與寫入邏輯不交叉，各有獨立的註冊與呼叫範例 |
| 2 | **對齊一致性** | 文件比對 | Sandbox 輸出結構與 `docs/SDK_QuickStart.md` 標題對齊 |
