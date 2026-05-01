# Sprint S29：Service 層與介面对齊
tracking_label: P29

## 任務目標
更新 Service 層以符合重構後的 BLL 介面。

## 任務核准狀態 (Co-sign)
- [ ] **工程師審核**: (Awaiting Review)
- [ ] **VCP 合理性確認**: (Awaiting Review)
- **核准日期**: <YYYY-MM-DD>

## 任務清單
- [ ] 更新 `IElasticOrderSearchService` 定義。
- [ ] 更新 `ElasticOrderSearchService` 實作，作為 BLL 的薄封裝。

## 檢核點
- [ ] 外部 Call 端（如 Web API）能通過 Service 存取所有 7 個搜尋功能。
