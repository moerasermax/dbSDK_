# Sprint S35：整合測試腳本對齊與驗證 (S23-S30)
tracking_label: P2-V1.1

## 任務目標
更新 `CPF.Sandbox` 內的 E2E 整合測試腳本，使其符合 S32/S33 的結構變更（容器 Object 化），並完成全量邏輯驗證。

> **[注意]** 客戶將會再提供新的測試資料，執行前需確認數據版本。

## 需求背景
由於 S32 將 S1/S4 的回傳容器由 Array 改為 Object，現有的 `E2E_S1_HomeOverview.cs` 與 `E2E_S4_AppDashboard.cs` 會因為嘗試存取不存在的陣列方法或屬性而編譯失敗或執行報錯。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，涵蓋所有受影響之測試場景 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## 任務清單
- [ ] **E2E_S1 修正**：修改 `E2E_S1_HomeOverview.cs`，移除對 `BuyerPerformance` 等容器的陣列處理，直接讀取物件屬性。
- [ ] **E2E_S4 修正**：修改 `E2E_S4_AppDashboard.cs`，移除對 `AppSellerOverView` 等容器的陣列處理。
- [ ] **ExpectedValueCalculator 同步**：檢查 `ExpectedValueCalculator.cs` 是否需同步調整以配合補零邏輯 (S33)。
- [ ] **執行全量驗證**：跑 `dotnet run --project CPF.Sandbox -- inttest validate`。

---

## PM 驗收項目 (VCP)

### 1. 整合測試全數通過
- 執行 `inttest validate`
- 驗證輸出：S1, S2, S3, S4, S5, S6, S7 必須全部顯示 `✅ PASS`。

---

## 技術檢核點
- [ ] 測試程式碼編譯成功。
- [ ] 測試數據對比邏輯不受 camelCase 影響（因 C# 屬性仍為 PascalCase）。
