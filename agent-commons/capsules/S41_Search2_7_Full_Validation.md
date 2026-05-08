# Sprint S41：Golden Recipe 全量驗收與邏輯修正 (S41-B Dual Engine 版)
tracking_label: P2B-VALIDATE-1 / S41-B

## 任務目標
在 S40 完成「物理結構對齊」後，本任務專注於「資料內容與業務邏輯對齊」。基於測試資料與客戶 SDK 實證，改採 **Dual Engine Hydration** 模式：由 ES 取得 ID 清單，再由 Mongo 補完完整明細（含 12 個巢狀結構），確保 Search 1-7 的數值、欄位完整性與時間格式與 Golden Recipe 100% 吻合。

---

## 任務核准狀態 (Co-sign)

> **規則**：PM 驗證項目需雙方簽核後，本 Capsule 才可進入 ACTIVE 狀態開工。

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (Gemini CLI 2026-05-08 - 升級 S41-B 策略)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (Claude Code 2026-05-08)
- **核准日期**: 2026-05-07 (PM 初版) / 2026-05-08 (PM/Eng 雙簽 S41-B)
- **狀態**: `ACTIVE` — 雙簽完成，S41-B 執行中

---

## 任務清單 (S41-B 拆解)

### 1. [✅ 完成] S41-B-1: 建置 MongoOrder Internal Model
- 新增 `Models/Internal/MongoOrder.cs`：
    - 1:1 Mirror 客戶 `OrderData` 結構（含 12 個 nested class）。
    - 使用 `[BsonElement("snake_case")]` 對齊 Mongo 欄位名。
    - 加入 `[BsonIgnoreExtraElements]` 以防 Production 環境欄位變更。

### 2. [ ] S41-B-2: 建置 Mongo DAL
- 新增 `DAL/MongoSearchDal.cs`：
    - 實作 `SearchByDDBAsync(List<string?> keyList)`，支援透過 `coom_no` 大量抓取資料。

### 3. [ ] S41-B-3: 建置 Convert Helper
- 於 `ConverToExtension.cs` 新增 `ConvertToOrderData` 方法：
    - 負責將 `MongoOrder` (DDBData) 轉換為 Public Model。
    - **時間格式修正 (IRON §B)**：移除 `Z` 後綴，格式統一為 `yyyy-MM-ddTHH:mm:ss.fff`。

### 4. [ ] S41-B-4: BLL Hydration 串接
- 修改 `SearchBy{Seller|Buyer}Async`：
    - ① ES query → `OPSResult` / `KeyList`。
    - ② Mongo find → `DDBData`。
    - ③ `ConvertToOrderData(DDBData)` → Public Model。

---

## 命名對齊原則 (平行轉移約束)

| 客戶原 SDK 字面 | dbSDK 對應 (PascalCase 化) |
|---|---|
| `_searchDal.SearchByOPS(model)` | `_searchDal.SearchByOPSAsync(req)` |
| `_searchDal.SearchByDDB(KeyList)` | `_mongoSearchDal.SearchByDDBAsync(keyList)` |
| `local var OPSResult / KeyList / DDBData` | **1:1 完全對齊** |
| `ConvertToOrderData(DDBData)` | `ConvertToExtension.ConvertToOrderData(DDBData)` |

---

## PM 驗收項目

| # | 驗證項目 | 驗證方式 | GoldenRecipe 參照 | 期望值 |
|---|---------|---------|-----------------|--------|
| 1 | **結構驗收** | 執行 `dump-s2` | 內部追蹤 | 輸出 JSON 應含 12 nested 結構 (即便為 null) |
| 2 | **命名對齊驗收** | Code Review BLL | 客戶 SDK 截圖 | 變數命名需完全對齊客戶字面 (OPSResult, etc.) |
| 3 | **資料補完驗收** | 執行 `dump-s2` | `Search_2_...txt` | `coomSellerMemo` 等 Mongo 欄位應正確輸出非 null 值 |
| 4 | **時間格式校驗** | 檢查 `coomCreateDatetime` | `Search_2_...txt` | 應為 `YYYY-MM-DDTHH:mm:ss.fff` 無 Z |

---

## 已知限制與揭示
- **Schema 推測**：6 個 nested 結構（CGoodsItem/EShipmentM/CCancelM/EShipmentL/EShipmentS/ECCCS）暫依 Public Model 反推，待後續校正。
- **一致性**：ES↔Mongo 同步機制不在本 Sprint 範圍，測試環境依賴 `GoldenSeeder`。

---

## 技術檢核點
- [ ] 符合 `DBSDK.md Part I §A` (MongoMap 是否需註冊 MongoOrder)。
- [ ] 符合 `DBSDK.md Part I §F` (核心層引用限制檢查)。
- [ ] 符合 `DBSDK.md Part I §B` (日期讀寫雙軌制)。
- [ ] 執行 `dotnet build` 無新錯誤。
- [ ] Sandbox E2E 測試全數 ✅ PASS。
