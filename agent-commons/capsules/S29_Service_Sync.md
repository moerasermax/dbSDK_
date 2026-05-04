# Sprint S29：Service 層與介面對齊
tracking_label: P29

## 任務目標
更新 Service 層以符合重構後的 BLL 介面。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-03
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 更新 `IElasticOrderSearchService` 定義（7 個方法，入參 `OrderSearchRequest`）
- [x] 更新 `ElasticOrderSearchService` 實作，作為 BLL 的薄封裝

---

## PM 驗收項目

本 Sprint 為 Service 層薄封裝，無獨立 PM 可操作的驗收項目。  
End-to-end 功能驗收統一在 **S30** 執行。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認 Service 正確性由 S30 全部情境驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] 外部呼叫端（如 Web API）可透過 `IElasticOrderSearchService` 存取全部 7 個搜尋功能
