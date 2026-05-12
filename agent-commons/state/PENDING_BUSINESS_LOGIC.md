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

### Search 1: 待出貨 (toship) 定義 — 客戶原邏輯 vs Golden 不一致
- **現象**：
    - 客戶原 SDK `OrderStateToshipForBuyerQuery` (user 2026-05-12 提供原碼確認):
      `coom_status IN (10, 20) AND (cooc_payment_pay_datetime exists OR cooc_payment_type="1")`
    - 用此邏輯跑現行測資 → **6 筆**
    - Golden Recipe `Search_1_GetHomeToDoOverview.txt` 樣張期望 → **1 筆**
- **歷程**：
    - 2026-05-12 S41-J 曾嘗試補 `esmm_status="01"` filter 對齊 Golden 1 筆、但屬 Engineer 從測資反推、未經客戶確認業務語義
    - 同日 user 提供客戶原 SDK 原碼確認**無 esmm_status filter**、revert S41-J
    - SDK 現狀 100% 對齊客戶原碼、Suite assertion 期望值改 6 (對齊客戶邏輯、不對齊 Golden)
- **需廠商確認** (三選一):
    1. Golden Recipe 樣張**有誤**、應改為 6 筆 (符合客戶 SDK 行為);或
    2. 業務語義需要**加 esmm_status="01"** (物流待寄件) 或其他 filter、Engineer 補回 query 條件;或
    3. 測資與 Golden 生成時 dataset 不同步、需補測資讓客戶邏輯跑出 1 筆
- **2026-05-12 User 指示**: 先等等，不確定哪邊才是對的，暫時保留 100% 對齊客戶原碼之現狀。

### Search 4: 預設查詢區間
- **現象**：Golden Recipe 樣張未提供輸入區間，但結果呈現 5 筆資料。SDK 若不傳區間預設 fallback 至 90 天，會導致命中 15 筆。
- **2026-05-12 user 補完客戶原 SDK 邏輯**:
    - `AppSellerOverview` = `today.AddDays(-90)` ~ `today` (90 天)
    - `AppSellerPerformance` = `mondayDate` ~ `today+1` (本週一 ~ 今天+1)
    - 兩 bucket 用**不同預設區間**
- **SDK 現狀**:
    - 已對齊客戶原邏輯 (S41-K)、BLL 拆兩段 fallback
    - Suite 注入 FixedClock(2026-05-05) 對齊 Golden Out (S41-L)
- **狀態**:✅ 客戶邏輯確認、PENDING 已解

---

## 3. 已知限制與揭示

- **退貨申請過濾**：目前測試資料中 `crsa_applied` 均為 0，因此 Search 1 的 `buyerReturnReq` / `sellerReturnReq` 雖然通過驗收，但尚未在「有退貨資料」的情境下驗證過 Filter 邏輯。
- **物流進階狀態**：`esms_dlv_status_seller_pickup` 等欄位在測試資料中為空，影響部分物流視角的過濾功能。
