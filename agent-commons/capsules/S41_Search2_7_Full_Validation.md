# Sprint S41：Golden Recipe 全量驗收與邏輯修正
tracking_label: P2B-VALIDATE-1

## 任務目標
在 S40 格式對齊後，執行 Search 1-7 的完整 Golden Recipe 測試，並修正所有數值不吻合的業務邏輯（Bug-fixing & Alignment）。

## 實作內容
1. **Search 1 修正 (已定位)**：
   - 修正 `SellerPerformance.OrderCount`：應包含取消單 (Total 24)。
   - 修正 `SellerPerformance.SalesAmt`：僅統計已完成單 (138)。
2. **Search 2-7 測試執行**：
   - 更新 `P2_SearchScenarioSuite.cs` 的期望值與日期參數。
   - 執行 `dump-s2` 至 `dump-s7`。
3. **邏輯排除與校準**：
   - 針對 FAIL 點進行 Debug。常見可能：
     - 時區偏差 (UTC vs Local)。
     - 狀態代碼定義 (什麼是 DealWith？)。
     - 分頁與排序規則。

## 驗收標準
- [ ] `dump-s1` 到 `dump-s7` 全數通過 (✅ PASS)。
- [ ] 最終輸出的數值與客戶 `.txt` 檔案完全一致。
