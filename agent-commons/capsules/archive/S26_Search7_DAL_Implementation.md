# Sprint S26：實作 Search 7 DAL 聚合查詢
tracking_label: P26

## 任務目標
在 `OrderSearchDal` 實作 `GetUserCgdmDataAsync` 方法。

## 需求背景
核心技術點：Nested → Terms → Reverse Nested → Max 跨層聚合。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-01
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 在 `OrderSearchDal.Aggregate.cs` 實作 `GetUserCgdmDataAsync`
- [x] 使用 `dbSDK` 的 `AdvancedSearchAsync` 發起請求
- [x] 實作解析邏輯，針對 `cood_items` 進行 Nested 聚合

---

## PM 驗收項目

本 Sprint 為 DAL 內部查詢實作，無直接 PM 可操作的驗收項目。  
End-to-end 功能驗收統一在 **S30** 執行。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認 DAL 正確性由 S30 Search_7 情境驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] ES DSL 包含 `nested` 路徑正確的聚合結構
- [x] 成功取得去重後的 `cgdmId` 及其對應的 `max(_ord_modify_date)`
