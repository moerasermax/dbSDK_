# Sprint S23：建立 OrderSearchRequest 統一入口 DTO
tracking_label: P23

## 任務目標
建立一個包含所有 7 個搜尋場景所需參數的萬能 DTO，作為 BLL 的統一入參。

## 需求背景
目前 6 個搜尋方法的參數高度重疊且鬆散，改用 Parameter Object 模式可提升 API 穩定性與擴充性。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-01
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 在 `PIC.CPF.OrderSDK.Biz.Read.Elastic.Models` 下建立 `OrderSearchRequest.cs`
- [x] 整合 `CuamCid`, `MemSid`, `DateStart/End`, `DateStartPoP`, `DateEndPoP`, `DateRangeType`, `PageInfo`, `Sorts` 等核心欄位
- [x] 為每個屬性添加 XML 註解，標明對應的 Search 編號
- [x] 屬性設為 `{ get; init; }` 確保不可變性

---

## PM 驗收項目

本 Sprint 為純 DTO 結構定義，無獨立 PM 可操作的驗收項目。  
End-to-end 功能驗收統一在 **S30** 執行。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認 DTO 覆蓋度由 S30 Sandbox 情境驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] 屬性涵蓋了 Search_1 到 Search_7 的所有輸入需求
