# Sprint S46：整合背景服務 (SendDataToXXX) 教學與範例
tracking_label: P2B-DOCUMENT-4 / S46

## 任務目標
在教學文件與 Sandbox 中整合背景同步服務的串接範例。包含從 Redis 接收事件、處理邏輯，並最終寫入 MongoDB 與 ElasticSearch 的完整資料流。

---

## 需求背景
使用者需要將系統全量整合進客戶端，除了查詢 SDK 外，還包含負責資料同步的 Background Services：
1. `CPF.Service.SendDataToMongoDB` (Mongo 寫入)
2. `CPF.Service.SendDataToElasticCloud` (ES 寫入)
3. `CPF.Services.Redis.Post` (Redis 事件源)

重點：強調「新增流程」是從 Redis 觸發，接收 Event 後再進行後續持久化。

---

## 任務清單

### 1. [ ] 更新 docs/SDK_QuickStart.md
- **新增「模組 C：背景同步服務 (Sync Services)」**。
- 說明如何在 `BackgroundService` 中注入 `IRepository<T>`。
- 展示 Redis 事件接收的程式碼片段。
- 描繪全流程圖（文字版）：`Redis Event` → `Worker` → `Mongo/ES Repository`。

### 2. [ ] 在 Sandbox 建立新場景或擴充範例
- 檔案：`CPF.Sandbox/Scenarios/BackgroundServiceScenario.cs` (新增)
- 內容：
    - 模擬一個 `Worker` 類別。
    - 展示如何從 Redis 模擬取得一筆「新增訂單事件」。
    - 呼叫 `MongoRepository` 進行資料落地。
    - 呼叫 `ElasticRepository` 或對應 DAL 進行 ES 同步。

### 3. [ ] 整合至 IntegrationGuideScenario.cs
- 在教學選單中加入「模組 C」的展示入口。

---

## PM 驗收項目 (VCP)

| # | 驗證項目 | 驗證方式 | 期望值 |
|---|---------|---------|--------|
| 1 | **流程完整性** | Code Review | 範例必須包含 Redis -> Worker -> DB 的完整鏈路說明 |
| 2 | **職責分離** | Code Review | 明確區分「查詢 (Search)」與「同步 (Sync)」的代碼邊界 |
| 3 | **配置對齊** | Code Review | 延續 S45 精神，使用 appsettings.json 配置 Redis 連線 |

---

## 技術檢核點
- [ ] 提醒使用者背景服務需繼承 `BackgroundService` 並實作 `ExecuteAsync`。
- [ ] 確保範例中處理了新增流程的 Id 產生邏輯（對齊 `DBSDK.md §7`）。
