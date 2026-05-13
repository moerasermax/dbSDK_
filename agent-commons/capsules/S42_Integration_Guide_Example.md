# Sprint S42：SDK 串接教學與 Mongo 更新範例實作
tracking_label: P2B-DOCUMENT-1 / S42

## 任務目標
在 Sandbox 專案中建立一份「串接小教學」，包含 SDK 查詢服務的 DI 注入說明，以及 MongoRepository 貨態更新（$set/$unset）的程式碼範例。

---

## 需求背景
為了讓使用者能快速將本 SDK 整合至客戶端環境，需要一份一目了然的範例程式。
內容需涵蓋：
1. `PIC.CPF.OrderSDK.Biz.Read.Elastic` 的服務註冊與呼叫。
2. `MongoRepository` 的貨態更新教學（強調 Flatten 局部更新與 UnsetFields 使用）。

---

## 任務核准狀態 (Co-sign)

- [ ] **PM**：任務範例需求定義完成 (Gemini CLI 2026-05-13)
- [ ] **Engineer**：確認範例實作路徑與技術細節 (Claude Code)

---

## 任務清單

### 1. [ ] 建立範例場景檔案
- 檔案：`CPF.Sandbox/Scenarios/IntegrationGuideScenario.cs`
- 內容：
    - 展示如何在 `Program.cs` 或 `Startup.cs` 進行 DI 註冊。
    - 撰寫 `RunAsync` 方法。
    - 範例 1：呼叫 `IElasticOrderSearchService` 進行訂單查詢。
    - 範例 2：呼叫 `IRepository<Order>.UpdateData` 執行 $set (局部更新) 與 $unset (欄位移除)。

### 2. [ ] 註冊至 SandboxRunner (選配)
- 若使用者需要直接執行，請在 `SandboxRunner.cs` 中保留進入點。

---

## PM 驗收項目 (VCP)

| # | 驗證項目 | 驗證方式 | 期望值 |
|---|---------|---------|--------|
| 1 | **教學易讀性** | Code Review | 程式碼註解清晰，明確區分查詢與更新範例 |
| 2 | **技術正確性** | Code Review | 範例中的 DI 註冊清單完整，UpdateData 呼叫方式正確（含 options） |

---

## 技術檢核點
- [ ] 範例代碼需符合 `DBSDK.md` 中的 Clean Architecture 規範。
- [ ] 強調 `FlattenBsonDocument` 在局部更新中的重要性（防止覆寫）。
