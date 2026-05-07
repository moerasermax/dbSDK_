# Sprint S41：Golden Recipe 全量驗收與邏輯修正
tracking_label: P2B-VALIDATE-1

## 任務目標
在 S40 完成「物理結構對齊」後，本任務專注於「資料內容與業務邏輯對齊」，確保 Search 1-7 的數值、欄位完整性與時間格式與 Golden Recipe 100% 吻合。

---

## 任務核准狀態 (Co-sign)

> **規則**：PM 驗收項目需雙方簽核後，本 Capsule 才可進入 ACTIVE 狀態開工。

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (Gemini CLI 2026-05-07)
- [ ] **Engineer**：驗收項目在實作完成後可被客觀驗證
- **核准日期**: 2026-05-07
- **狀態**: `PENDING_COSIGN` — 待 Engineer 簽核後升 ACTIVE

---

## 任務清單

### 1. 基礎模型擴充 (Infrastructure/Models)
- 擴充 `OrderDocument.cs`：補足 ES 中存在但模型缺失的欄位（如 `_coom_seller_memo`, `_cooc_create_datetime` 等）。
- 擴充 `CoodItems.cs`：補足商品單價與折扣價欄位（`coodOriginalPrice`, `coodDiscountPrice`, `coodReceivePrice`）。

### 2. 映射邏輯修復 (Infrastructure/Extension)
- 更新 `ConverToExtension.cs`：
    - 連結所有目前為 `null` 的巢狀物件（如 `c_Goods_Item`, `c_Cancel_M`, `e_CCDHL` 等），確保 ES 資料正確流入。
    - **時間格式修正**：調整 `DateTime` 轉換邏輯，確保輸出不帶 `Z` 後綴且對齊 Local Time（符合客戶 Golden Out 樣張）。

### 3. 業務邏輯校準 (DAL/BLL)
- **Search 1 (Overview)**：
    - 修正 `OrderCount`：統計邏輯需包含所有狀態（含取消單）。
    - 修正 `SalesAmt`：僅統計已完成訂單。
- **Search 2-7 (Query Logic)**：
    - 移除 BLL 中硬編碼的 `DateTime.UtcNow`，改由 Sandbox 傳入參數。
    - 確保排序規則（RankingNo/Sequence）與 Golden Recipe 一致。

### 4. 全量測試驗證 (Sandbox)
- 更新 `P2_SearchScenarioSuite.cs` 的期望值（Expected Values）以符合 Golden Recipe 100% 對齊。
- 執行 `dump-s1` 到 `dump-s7`。

---

## PM 驗收項目

| # | 驗證項目 | 驗證方式 | GoldenRecipe 參照 | 期望值 |
|---|---------|---------|-----------------|--------|
| 1 | 時間格式校驗 | 檢查 `coomCreateDatetime` | `Search_2_...txt` | 應為 `YYYY-MM-DDTHH:mm:ss.fff` 無 Z |
| 2 | 欄位補全校驗 | 檢查 `coomSellerMemo` | `Search_2_...txt` | 不應為 `null` (若 ES 有值) |
| 3 | 邏輯數值校驗 | 執行 `dump-s1` | `Search_1_...txt` | 數值 100% 吻合樣張 |

### 驗收項目簽核
- [x] **PM 簽核**：確認上列項目合理 (Gemini CLI 2026-05-07)
- [ ] **Engineer 簽核**：確認上列項目可測

---

## 技術檢核點
- [ ] 符合 `DBSDK.md Part I §B` (日期讀寫雙軌制)。
- [ ] 執行 `dotnet build` 無新錯誤。
- [ ] Sandbox E2E 測試全數 ✅ PASS。
