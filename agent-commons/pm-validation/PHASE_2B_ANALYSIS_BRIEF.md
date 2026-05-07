# Phase 2.B 技術分析工作清單 (致 Kiro)

> **發起者**：PM (Gemini CLI)
> **執行者**：Kiro (首席工程師)
> **目標**：完成 Golden Data 導入並建立雙引擎整合之技術基座。

---

## 1. [S37] 數據導入執行與驗證 (Execution & Verification)
- **執行指令**：`dotnet run --project CPF.Sandbox seed-golden`。
- **一致性檢查**：
    - 統計 ElasticSearch `orders-605` 總量。
    - 統計 MongoDB `CpfOrderDb.Orders` 總量。
    - 隨機抽樣 3 筆 `coom_no`，確認兩邊資料完全對齊。

## 2. 雙引擎鏈路分析 (Dual-Engine Linkage Analysis)
- **索引映射**：分析 `orders-605` 中的 `_id` 是否與 Mongo 的主鍵一致（或確認以 `coom_no` 作為唯一關聯鍵）。
- **效能預判**：分析 `In` 查詢在現有數據規模下的回應時間，評估是否需增加 Mongo 索引。

## 3. Golden Recipe 差異分析 (Gap Analysis)
- **環境對齊**：比對客戶提供的 `Search_1_...txt` 等結果，確認其統計區間（Date Range）與 Sample Data 的時區偏差（UTC+0 vs UTC+8）。
- **邏輯比對**：
    - 針對 `S32/S33` 已完成的 Casing 與補零邏輯，分析在真實數據下是否仍有欄位缺失。
    - 檢查客戶 Golden Out 中的 `ranking_no` 排序規則是否與目前的實作 100% 吻合。

---

## 預期產出
1. **S37 執行日誌**：包含成功/失敗筆數。
2. **雙引擎對齊報告**：確認 Linkage Key 的唯一性與索引建議。
3. **S39 預覽報告**：指出目前實作與客戶 Golden Recipe 的潛在差異點。
