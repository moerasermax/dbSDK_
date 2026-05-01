# Sprint S23：建立 OrderSearchRequest 統一入口 DTO
tracking_label: P23

## 任務目標
建立一個包含所有 7 個搜尋場景所需參數的萬能 DTO，作為 BLL 的統一入參。

## 需求背景
目前 6 個搜尋方法的參數高度重疊且鬆散，改用 Parameter Object 模式可提升 API 穩定性與擴充性。

## 任務核准狀態 (Co-sign)
- [x] **工程師審核**: Confirmed — 任務清單可行，補充 DateStartPoP/DateEndPoP/DateRangeType 至實作
- [x] **VCP 合理性確認**: Conditional — VCP 描述模糊，由 Engineer 實作時自行對照 7 個 Search 場景欄位清單驗收
- **核准日期**: 2026-05-01

## 任務清單
- [ ] 在 `PIC.CPF.OrderSDK.Biz.Read.Elastic.Models` 下建立 `OrderSearchRequest.cs`。
- [ ] 整合 `CuamCid`, `MemSid`, `DateStart/End`, `PageInfo`, `Sorts` 等核心欄位。
- [ ] 為每個屬性添加 XML 註解，標明對應的 Search 編號。
- [ ] 屬性設為 `{ get; init; }` 確保不可變性。

## 檢核點
- [ ] 程式碼編譯通過。
- [ ] 屬性涵蓋了 `Search_1` 到 `Search_7` 的所有輸入需求。
