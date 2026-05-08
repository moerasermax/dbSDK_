致 下一任開發者 (Engineer/PM)

## 1. 任務上下文 (Context)
- **當前任務**：Sprint S41 (Golden Recipe 全量驗收) 已進入 **PENDING CONFIRMATION** 狀態。
- **核心進度**：
    - 已完成 Search 1-7 的技術對齊，包含 Dual Engine Hydration (ES+Mongo) 與時間格式 (無 Z)。
    - 工程師已將 `SearchByDDB` 重構為 `SearchByMongo`。
    - Sandbox 已加強選單互動功能，支援 dump-s1~s7 的 verbose 模式。

## 2. 關鍵阻塞 (P1 Blockers)
- **Search 7 資料缺源**：`cgdmUpdateDatetime` 輸出為空，根因為 ES 測試資料缺少 `_ord_modify_date` 欄位。已詳列於 `agent-commons/state/PENDING_BUSINESS_LOGIC.md`。

## 3. 待確認規格 (P2 Alignment)
- **Search 1**: 待出貨 (toship) 的精確過濾條件（目前 SDK 命中 6 筆 vs Golden 期望 1 筆）。
- **Search 4**: Caller 不傳時間時的預設區間定義。

## 4. 下一步行動 (Next Actions)
1. 提報 `PENDING_BUSINESS_LOGIC.md` 給客戶或廠商進行規格確認。
2. 待規格確認後，由 Engineer 修正 BLL 過濾邏輯（若需）。
3. 執行 S41 結案並歸檔膠囊。
4. 啟動 S38 (Dual Engine Integration - 自動化同步機制)。

## 5. 技術揭示
- 6 個 Mongo nested schema 目前為反推值，若客戶 Production 端 schema 不同，`[BsonIgnoreExtraElements]` 會確保不崩潰，但需逐一校正欄位。
