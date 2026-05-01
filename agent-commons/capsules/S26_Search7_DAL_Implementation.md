# Sprint S26：實作 Search 7 DAL 聚合查詢
tracking_label: P26

## 任務目標
在 `OrderSearchDal` 實作 `GetUserCgdmDataAsync` 方法。

## 需求背景
核心技術點：Nested -> Terms -> Reverse Nested -> Max 跨層聚合。

## 任務核准狀態 (Co-sign)
- [ ] **工程師審核**: (Awaiting Review)
- [ ] **VCP 合理性確認**: (Awaiting Review)
- **核准日期**: <YYYY-MM-DD>

## 任務清單
- [ ] 在 `OrderSearchDal.Aggregate.cs` 實作查詢邏輯。
- [ ] 使用 `dbSDK` 的 `AdvancedSearchAsync` 發起請求。
- [ ] 實作解析邏輯，將 ES Response 轉換為內部聚合模型。

## 檢核點
- [ ] 聚合指令產出的 JSON 符合 `Search_7` 預期。
- [ ] 成功取得去重後的商品 ID 及其最新時間。
