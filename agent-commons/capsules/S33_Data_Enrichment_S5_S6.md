# Sprint S33：趨勢資料補零、排序序號補全與容器轉型
tracking_label: P2-3

## 任務目標
修正 Search_5/6 的趨勢圖輸出（補零），加入 `rankingNo`，並將主結構改為單一物件。

## 需求背景
金標顯示趨勢圖需維持時間軸完整性（補 0），且銷售排行需有 `rankingNo`。Search_5/6 的 `data` 節點應為單一物件。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## 任務清單
- [ ] 修改 `OrderSearchDal.AggregateHelpers` 的趨勢圖解析，對缺失的時段進行補 0 (Zero Padding)
- [ ] 在 `ProductSalesRanking` 模型加入 `RankingNo` 屬性，並在 BLL 解析時依序填入
- [ ] 修改 Search_5/6 BLL 回傳值，將 `ApiResponseWrapper<List<AppSalesMetricsResultModel>>` 改為 `ApiResponseWrapper<AppSalesMetricsResultModel>` (單一物件)

---

## PM 驗收項目 (VCP)

### 1. 趨勢長度驗證 (Today vs Week)
- **S5 (Today)**：執行 `dump-s5`，驗證 `jq '.data.salesTrendData | length'` 應為 `24`
- **S6 (Week)**：執行 `dump-s6`，驗證 `jq '.data.salesTrendData | length'` 應為 `7`

### 2. 序號與容器驗證
- 執行 `dump-s6`
- 驗證 `data` 型態：`jq '.data | type'` 應為 `"object"`
- 驗證排行序號：`jq '.data.productSalesRanking[0].rankingNo'` 應為 `1`

---

## 技術檢核點
- [ ] 補零邏輯需處理 Today (24 buckets) 與 Week (7 buckets) 不同場景
- [ ] 確保 `RankingNo` 正確對應 camelCase
