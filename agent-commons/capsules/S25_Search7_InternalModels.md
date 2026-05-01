# Sprint S25：建立 Search 7 內部聚合模型
tracking_label: P25

## 任務目標
建立 DAL 層解析 ElasticSearch 聚合結果所需的內部模型。

## 需求背景
為了支援強型別解析 Nested Aggregation 的結果。

## 任務核准狀態 (Co-sign)
- [ ] **工程師審核**: (Awaiting Review)
- [ ] **VCP 合理性確認**: (Awaiting Review)
- **核准日期**: <YYYY-MM-DD>

## 任務清單
- [ ] 在 `Models.Internal` 下建立 `UserCgdmDataAggregateModel`。
- [ ] 定義對應 ES 聚合名稱的屬性結構。

## 檢核點
- [ ] 模型能正確承載 ES 返回的 `Max` 聚合數據。
