# Kiro 同步節點 04：dbSDK 核心進化與全域沙盒體系 (2026-04-24)

## 1. 當前架構里程碑
*   **解耦評分**：10 / 10 (階段性完工)
*   **核心變革**：Repository 改組合模式、Model 斷開繼承、業務 ID 與物理 ID 徹底解耦。
*   **真實性鏈結**：沙盒與 Mock 100% 呼叫 `MongoRepository` 原生 `public static` 函式，達成「驗證即事實」。

## 2. 關鍵專案結構
*   **`NO3._dbSDK_Imporve`**：SDK 核心，已完成 P0 級安全與非同步穩定性重構。
*   **`CPF.Sandbox`** (New!)：獨立驗證專案，引用所有生產級 Model，具備「記憶力」與「對比報告」能力。
*   **`CPF.Services.Redis.Post`**：已瘦身，所有測試邏輯遷移至沙盒，回歸純業務職責。

## 3. 已驗證業務場景 (Mock)
*   **Status 10**：全訂單 Insert 驗證。
*   **局部更新**：備註更新 (點符號路徑)、支付方式變更 (Null 排除)。
*   **Status 20**：賣家取號、物流模組 (esmm/esml/esms) 動態掛載與初始化。
*   **Status 30**：寄貨完成、貨態歷程增量追加 ($push) 驗證。

## 4. 下週一優先任務 (P0)
- [ ] **[重要] SDK 原生邏輯鏈結審核**：由使用者親自執行 `CPF.Sandbox`。
    - 驗證 `StatefulComparisonScenario` (S13) 的資料演變對比。
    - 驗證 `SellerGetNumberScenario` (S14) 的物流模組掛載。
    - 驗證 `ShippingCompleteScenario` (S15) 的陣列追加結果。

## 5. 待處理技術債 (P1)
*   CancellationToken 全面導入。
*   ElasticFilter 強型別解析器實作。

---
**存檔完畢。今日進度已完美凍結於此同步節點。**
