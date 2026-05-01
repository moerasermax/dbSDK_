# Sprint S28：BLL 入口點大重構
tracking_label: P28

## 任務目標
將 `ElasticOrderSearchBll` 的所有公開方法重構為統一入口模式。

## 任務核准狀態 (Co-sign)
- [ ] **工程師審核**: (Awaiting Review)
- [ ] **VCP 合理性確認**: (Awaiting Review)
- **核准日期**: <YYYY-MM-DD>

## 任務清單
- [ ] 將 `SearchOrderInfoAsync` 分拆為 `SearchOrderInfoBySellerId` 與 `SearchOrderInfoByBuyerId`。
- [ ] 所有 6+1 個方法入參統一改為 `OrderSearchRequest`。
- [ ] 在 BLL 層處理 `OrderSearchRequest` 到內部 DAL Model 的轉換（對齊鐵律 ⑤）。

## 檢核點
- [ ] BLL 介面達成強型別化，不再依賴鬆散參數。
