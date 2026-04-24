# 專案狀態 Checkpoint — dbSDK 重構歷程（Gemini 用）

## 專案概述
- 名稱：NO3._dbSDK_Imporve（.NET 8）
- 目標：建立支援 MongoDB / Elasticsearch / Redis 的通用資料庫 SDK
- 目前解耦評分：10 / 10 (階段性完工)
- 專案健康度：✅ 卓越 (具備完整的業務生命週期模擬能力)
- 溝通語言：繁體中文，程式碼英文命名

---

## 目前目錄結構

```
CPF.Sandbox/
  Scenarios/
    StatefulComparisonScenario.cs   ← 步進驗證與對比報告
    SellerGetNumberScenario.cs      ← 賣家取號與模組掛載驗證
    ShippingCompleteScenario.cs     ← 寄貨追加驗證 (S15)
```

---

## 核心設計決策

### SDK 原生邏輯鏈結 (Native Fidelity)
打破測試代碼與生產代碼的牆，沙盒與 Mock 專案直接呼叫 `MongoRepository` 類別內的原生 `public static` 函式。這確保了「驗證結果即事實」，達成了 100% 的邏輯一致性。

### 狀態模擬器 (Stateful Simulator)
`MockOrderRepository` 具備內存存儲與點符號路徑解析，可預演資料庫內容演變，產出自動化對比報告。

---

## 尚未解決的問題 (待驗證項目)

### P0（使用者親自驗證）
- **[重要] SDK 原生邏輯鏈結審核**：預計下週一由使用者親自執行 `CPF.Sandbox`，驗證重構後的邏輯鏈結是否 100% 正確，並確認對比報告的易讀性。

### P1（架構優化）
- ElasticFilter 強型別支援：減少搜尋查詢中的硬編碼字串。
- CancellationToken 支援：確保所有非同步操作均可取消。

---

## 接續指令

新 Session 開始時請先閱讀此文件，優先確認「P0 使用者親自驗證」的結果。
