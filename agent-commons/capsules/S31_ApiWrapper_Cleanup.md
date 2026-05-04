# Sprint S31：撤銷 Wrapper 與偵錯工具恢復 (v2)
tracking_label: P2-1

## 任務目標
將 BLL 回傳值由 `ApiResponseWrapper<T>` 改回業務 Model 類型，並恢復 Sandbox 的 `dump-sN` 偵錯命令。

## 需求背景
User 指示 SDK 不需要自行實作外層 API 回傳格式（Wrapper），客戶會自行處理包裹邏輯。此外，為了後續驗證，需恢復之前被移除的 `dump-sN` 命令。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## v1 → v2 變更紀錄
- **v1 (2026-05-03 CO-SIGNED)**: 含 `ApiResponseWrapper`，已於 `b86b958..dfa5037` commit 範圍實作完畢。
- **v2 (2026-05-04)**: 依 User 裁決撤回 Wrapper，Engineer 已於 `a50a2d5` + `a768ebc` commit 範圍完成 Revert。
- **現狀**: v2 殘餘任務（移除 `Took` / `dump-sN`）由 `6475192` + `dfa5037` 提供，視為 v2 預先完成。

---

## 任務清單
- [x] 修改 `ElasticOrderSearchBll.cs` 的所有方法，移除 `ApiResponseWrapper` 封裝，直接回傳對應的 ResultModel
- [x] 移除 `ApiResponseWrapper.cs` 檔案 (已於 `a768ebc` 執行)
- [x] 移除模型中不必要的 `Took` 屬性 (已於 `6475192` 執行)
- [x] 在 `CPF.Sandbox/Program.cs` 恢復 `dump-s1` 到 `dump-s7` 的入口命令 (已於 `dfa5037` 執行)

---

## PM 驗收項目 (VCP)

### 1. 輸出結構驗證
- 執行 `dotnet run --project CPF.Sandbox -- dump-s7`
- 驗證外層**無** Wrapper：`jq 'has("data")'` 應為 `false` (直接輸出 `UserCgdmDataResultModel` 內容)

### 2. 偵錯工具可用性驗證
- 確保 `dotnet run --project CPF.Sandbox -- dump-s1` 到 `dump-s7` 均能正常執行

---

## 技術檢核點
- [x] 程式碼編譯通過 (暫時除外：Sandbox 腳本待修正)
- [x] 確認 BLL 回傳類型與介面定義同步更新
