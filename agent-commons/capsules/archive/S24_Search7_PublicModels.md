# Sprint S24：建立 Search 7 公開模型 (DTO)
tracking_label: P24

## 任務目標
建立 `GetUserCgdmData` 接口對外暴露的 Result 模型。

## 需求背景
依據 `Search_7_GetUserCgdmData.txt` 的輸出格式要求。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作（已追認 2026-05-03）
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（已追認 2026-05-03）
- **核准日期**: 2026-05-01
- **狀態**: `ACTIVE`

---

## 任務清單
- [x] 建立 `UserCgdmDataResultModel`（含 `CuamCid`, `Cgdm[]`）
- [x] 建立 `CgdmDataModel`（含 `CgdmId`, `CgdmUpdateDatetime`）

---

## PM 驗收項目

本 Sprint 為模型結構定義，無獨立 PM 可操作的驗收項目。  
資料結構正確性由 **S30** Search_7 情境端對端驗收。

### 驗收項目簽核
- [x] **PM 簽核**：確認無需獨立驗收（已追認 2026-05-03）
- [x] **Engineer 簽核**：確認模型正確性由 S30 驗證（已追認 2026-05-03）

---

## 技術檢核點
- [x] 程式碼編譯通過（0 errors）
- [x] 模型結構包含 `data { cuamCid, cgdm: [] }`
- [x] `CgdmDataModel` 屬性精確對應 `CgdmId`（string）與 `CgdmUpdateDatetime`（string）
