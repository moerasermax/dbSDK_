# Sprint S34：Mongo 12 表深度資料整合 (S2/S3)
tracking_label: P2-4

## 任務目標
實作 Search_2/3 的二階段查詢邏輯，利用 ES 取得訂單序號後，從 MongoDB 補強完整之 12 表關聯資料。

## 需求背景
ES 僅儲存扁平資訊。為滿足 Search_2/3 金標要求的 12 個子物件結構，需從 Mongo 撈取完整實體。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## 任務清單
- [x] **第 0 項 (Probe)**：確認 Mongo `OrderModel` schema。
    - **來源實體**：`CPF.Service.SendDataToMongoDB/Model/Order/OrderModel.cs`
    - **結構驗證**：已確認包含 12 個子物件，結構與金標高度對齊。
    - **設計議題**：跨專案引用路徑由 Engineer 在動工時決議。
- [ ] 在 `OrderSearchDal` 實作二階段組合邏輯
- [ ] 第一階段：呼叫 ES 查詢取得分頁後的 `coom_no` 清單
- [ ] 第二階段：透過 `IRepository<OrderModel>` (Mongo) 根據序號批次撈取實體
- [ ] 將 `OrderModel` 實體映射至金標要求的 12 個子物件結構並處理 camelCase

---

## PM 驗收項目 (VCP)

### 1. 完整資料結構驗證 (12 Sub-objects)
- 執行 `dotnet run --project CPF.Sandbox -- dump-s2`
- 驗證子物件清單：`jq '.data[0] | keys | length'` 應為 `>= 12`
- 逐項驗證關鍵 Key：`c_Order_M`, `c_Order_D`, `e_Shipment_M`, `e_Shipment_L`, `e_Shipment_S`, `c_Goods_Item`, `c_Order_C`, `c_Question_M`, `c_Cancel_M`, `e_CCDHL`, `e_CCCS`, `e_RtnDHL_Apply` 均應存在於結果中。

---

## 技術檢核點
- [ ] 需注意分頁後批次查詢的效能，避免 N+1 問題
- [ ] 確保映射邏輯完整覆蓋 12 個子物件
