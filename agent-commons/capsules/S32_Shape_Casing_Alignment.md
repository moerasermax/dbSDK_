# Sprint S32：容器結構修正與 camelCase 全局對齊
tracking_label: P2-2

## 任務目標
將 Search_1/4 的輸出由陣列改為單一物件，並將 S1, S4, S5, S6, S7 的所有屬性名稱改為小寫開頭 (`camelCase`)。

## 需求背景
客戶 GoldenRecipe 的 Search_1/4 輸出為單一物件 `{}` 非陣列 `[]`。此外，所有欄位名在金標中均為 `camelCase`。

---

## 任務核准狀態 (Co-sign)

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (已簽署 2026-05-04)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證 (已簽署 2026-05-04)
- **核准日期**: 2026-05-04
- **狀態**: `CO-SIGNED`

---

## 任務清單
- [ ] 修改 `AggregateOrderInfoResultModel` (S1) 與 `AppDashboardAggregateResultModel` (S4) 的 BLL 映射邏輯，將其內部的 sub-section (如 `BuyerOverview`, `AppSellerOverView`) 改為回傳單一物件而非陣列
- [ ] 在 S1, S4, S5, S6, S7 相關的所有 ResultModel 屬性加上 `[JsonPropertyName("camelCase")]`
- [ ] 修正 BLL 內部對這些模型的物件轉換邏輯，確保對應到正確的 camelCase 名稱

---

## PM 驗收項目 (VCP)

### 1. 容器型態驗證 (S1)
- 執行 `dotnet run --project CPF.Sandbox -- dump-s1`
- 驗證 `data.buyerOverView` 是 Object 而非 Array：`jq '.data.buyerOverView | type'` 應為 `"object"`

### 2. Casing 驗證 (S7)
- 執行 `dotnet run --project CPF.Sandbox -- dump-s7`
- 驗證欄位名：`jq '.data | has("cuamCid")'` 應為 `true`

---

## 技術檢核點
- [ ] 所有 S1, S4, S5, S6, S7 涉及的 Model 均已完成屬性標註
- [ ] 程式碼編譯通過
