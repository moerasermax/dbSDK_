# [P1] ElasticSearch 功能開發 (Master Tracker)

> **需求起源**：對接 CUN9101 Sample Data，實作 BLL 統一入口與 Search 7 聚合查詢。
> **總體狀態**：⏳ 執行中 (0/8 完成)

---

## 📋 任務進度清單 (Checklist)

| 系列標籤 | Sprint 編號 | 任務名稱 | 負責人 | 狀態 | VCP 快速摘要 |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **P1-1** | **S23** | OrderSearchRequest 建立 | Claude | [ ] Todo | 包含 Search 1-7 全參數，init only 屬性 |
| **P1-2** | **S24** | Search 7 公開模型 (DTO) | Claude | [ ] Todo | 對齊 Sample JSON 輸出結構 |
| **P1-3** | **S25** | Search 7 內部聚合模型 | Claude | [ ] Todo | 強型別承載 ES Max Agg 結果 |
| **P1-4** | **S26** | Search 7 DAL 實作 | Claude | [ ] Todo | 完成 Nested -> Reverse Nested 查詢 |
| **P1-5** | **S27** | Search 7 轉換擴充 | Claude | [ ] Todo | ConvertToUserCgdmDataResultModel |
| **P1-6** | **S28** | BLL 入口點大重構 | Claude | [ ] Todo | 統一 Request DTO 入參，分拆 2/3 |
| **P1-7** | **S29** | Service 層同步更新 | Claude | [ ] Todo | 更新 Interface 與 Service 實作 |
| **P1-8** | **S30** | 綜合範例與測試 | Claude | [ ] Todo | 7 個 Search 場景 100% 驗證通過 |

---

## 🛡 跨任務共通鐵律 (Shared Invariants)
- [ ] **Result 邊界**：所有 BLL 方法必須回傳 `IResult` 並捕捉底層 Exception。
- [ ] **Commit 規範**：使用 `feat(scope): [P1-x] subject` 格式。

---
*此總表為 P1 階段的最高進度索引。*
