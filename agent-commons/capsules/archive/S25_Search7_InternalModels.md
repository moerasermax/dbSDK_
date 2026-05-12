# Sprint S25：建立 Search 7 內部聚合模型
tracking_label: P25

## 任務目標
建立 DAL 層解析 ElasticSearch 聚合結果所需的內部模型。

## 需求背景
為了支援強型別解析 Nested Aggregation 的結果。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-01
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 在 `Models.Internal` 下建立 `UserCgdmDataAggregateModel`
- [x] 定義對應 ES 聚合名稱 `max_modify_date` 的屬性

---

## PM 驗收項目

本 Sprint 為 ES 內部聚合模型，無直接 PM 可操作的驗收項目。  
End-to-end 功能驗收統一在 **S30** 執行。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認內部模型正確性由 S30 驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] 模型能正確承載 ES 返回的 `Max` 聚合數據（double/long 轉 DateTime）
