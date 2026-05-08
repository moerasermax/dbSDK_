# dbSDK Pending Business Logic & Data Gaps

> 此文件用於追蹤與 Golden Recipe 對齊過程中發現的業務邏輯疑點 (Spec Discrepancies) 與 測試資料缺源 (Data Gaps)。

## 1. 測試資料缺源 (Data Gaps) - P1 Blocker

### Search 7: cgdmUpdateDatetime 遺失
- **現象**：`GetUserCgdmData` 輸出的 `cgdmUpdateDatetime` 永遠為空字串 `""`。
- **根因**：
    - SDK 預期從 ES 欄位 `_ord_modify_date` 抓取最大值。
    - 現行 `測試資料_Elastic.txt` (cuamCid=528672) 中完全不含此欄位。
- **需廠商確認**：
    1. Production 環境的 ES 是否確定有 `_ord_modify_date`？
    2. 若有，測試資料是否應補齊？若無，則 SDK 應改用哪個欄位作為更新時間基準？

---

## 2. 業務邏輯疑點 (Spec Discrepancies) - P2 Alignment

### Search 1: 待出貨 (toship) 定義
- **現象**：現行 SDK Filter (coom_status [10,20] + 已付款) 在測試資料中命中 **6 筆**，但 Golden Recipe 期望為 **1 筆**。
- **需廠商確認**：
    - 「待出貨」的完整過濾條件為何？是否需排除特定物流狀態 (esmm_status) 或特定訂單旗標？

### Search 4: 預設查詢區間
- **現象**：Golden Recipe 樣張未提供輸入區間，但結果呈現 5 筆資料。SDK 若不傳區間預設 fallback 至 90 天，會導致命中 15 筆。
- **需廠商確認**：
    - 當呼叫端未傳入時間範圍時，該 API 的預設行為 (Default Date Range) 應為何？

---

## 3. 已知限制與揭示

- **退貨申請過濾**：目前測試資料中 `crsa_applied` 均為 0，因此 Search 1 的 `buyerReturnReq` / `sellerReturnReq` 雖然通過驗收，但尚未在「有退貨資料」的情境下驗證過 Filter 邏輯。
- **物流進階狀態**：`esms_dlv_status_seller_pickup` 等欄位在測試資料中為空，影響部分物流視角的過濾功能。
