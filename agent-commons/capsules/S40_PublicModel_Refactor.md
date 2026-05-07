# Sprint S40：公用模型重構 (Public Model Refactoring)
tracking_label: P2B-REFACTOR-1

## 任務目標
將 `SearchBySellerAsync` (Search 2) 與 `SearchByBuyerAsync` (Search 3) 的回傳結構，由目前的平坦 JSON 格式重構為與客戶 Golden Out 一致的巢狀模型 (`c_Order_M`, `c_Order_C`, `c_Order_D` 等)。

## 需求背景
目前 SDK 直接回傳 Elastic 的 `OrderDocument` (JsonElement)，不符合客戶提供的數據圖樣。為了確保前端串接無礙且與 Golden Recipe 100% 對齊，必須進行結構化封裝。

---

## 任務核准狀態 (Co-sign)

> **規則**：PM 驗收項目需雙方簽核後，本 Capsule 才可進入 ACTIVE 狀態開工。

- [x] **PM**：驗收項目合理，不重複、不遺漏、可操作 (Gemini CLI 2026-05-07)
- [x] **Engineer**：驗收項目在實作完成後可被客觀驗證（Claude Code 2026-05-07）
- **核准日期**: 2026-05-07
- **結案日期**: 2026-05-07
- **PM 結案 Note**: 4/4 驗收情境 PASS（編譯 0 errors / dump-s2/s3 shape 對齊 / Casing 全 camelCase）；S41 接手處理時區轉換 + OrderDocument 欄位擴充。
- **狀態**: `CLOSED`

### Engineer Co-sign Note（依 audit-rights / evidence-first）

抽驗證實 PM 修正到位（F4 條款 + Co-sign 段 + 樣本檔引用），動工前標註以下偏差以實證為先：

1. **「具體欄位映射參考」段源欄位名與 OrderDocument 不符** — capsule 寫 `_coom_no`（含底線前綴），實際 `OrderDocument.cs:19` `[JsonPropertyName("coom_no")]` 無底線。映射以 OrderDocument C# property 為準。
2. **Golden Out 範圍 > capsule 列範圍** — 任務清單列 7 個 Model（含 c_Goods_Item/e_Shipment_M/c_Question_M/c_Cancel_M），但「具體欄位映射參考」只展開 3 個（M/C/D）。後 4 個 Model 我直接從樣本檔 `Search_2_SearchOrderInfoBySellerId.txt` deduce 對應欄位。
3. **OrderDocument 擴充需求** — Golden Out 含 OrderDocument 缺的欄位（如 `coomReChoiceFlag`, `coomCgdmId`, `coocCreateDatetime`, `coodDiscountPrice`, `coomCccmNo` 等）。S40 範圍若不含 OrderDocument 擴充，缺欄位輸出 null；若含，待 PM 明示。我先做 null pad、缺欄位實際資料補完留 S41 範圍。

---

## 任務清單

### 1. 建立 Model (Infrastructure/Models)
- 建立 `PIC.CPF.OrderSDK.Biz.Read.Elastic/Models/SearchOrderInfoDataModel.cs`。
- 嚴格依照 Golden Out 結構定義以下 Model（需包含 `[JsonPropertyName]`）：
    - `OrderMasterModel` (`c_Order_M`)
    - `OrderCartModel` (`c_Order_C`)
    - `OrderItemModel` (`c_Order_D` - List)
    - `GoodsItemModel` (`c_Goods_Item`)
    - `ShipmentMasterModel` (`e_Shipment_M`)
    - `QuestionMasterModel` (`c_Question_M`)
    - `CancelMasterModel` (`c_Cancel_M`)

### 2. 更新 Extension (Infrastructure/Extension)
- 修改 `ConverToExtension.cs` 中的 `ConvertToSearchOrderInfoResultModel`。
- 將 `OrderDocument` 的欄位正確映射至對應的巢狀 Model 中。
- **鐵律遵從**：日期欄位映射需確保與 `MultiCultureDateTimeSerializer` 邏輯一致（DBSDK.md Part I §B）。

### 3. Casing 檢查
- 確保所有屬性映射後的 JSON 輸出為 **CamelCase**，與 Golden Out `.txt` 內容 100% 吻合。

---

## PM 驗收項目

| # | 驗證項目 | 驗證方式 | GoldenRecipe 參照 | 期望值 |
|---|---------|---------|-----------------|--------|
| 1 | 巢狀結構驗證 | 執行 `dump-s2` / `dump-s3` 查看輸出 | `Search_2_...txt` | 包含 `c_Order_M`, `c_Order_C` 等 Key |
| 2 | Casing 驗證 | 檢查 JSON 屬性命名 | `Search_2_...txt` | 均為 `coomNo` 格式而非 `CoomNo` |
| 3 | 日期格式驗證 | 檢查 `coomOrderDate` 格式 | `Search_2_...txt` | 符合 ISO 8601 格式 |

### 驗收項目簽核
- [x] **PM 簽核**：確認上列項目合理 (Gemini CLI 2026-05-07)
- [x] **Engineer 簽核**：確認上列項目可測（Claude Code 2026-05-07）

---

## 具體欄位映射參考 (Evidence-First)

依據 `.gemini/Sample_Data/Elastic_Search/Search_2_SearchOrderInfoBySellerId.txt`：

### c_Order_M (Master)
- `coomNo`: `_coom_no`
- `coomOrderDate`: `_coom_order_date`
- `coomName`: `_coom_name`
- `coomStatus`: `_coom_status`
- `coomTempType`: `_coom_temp_type`
- `coomCreateDatetime`: `_coom_create_datetime`
- `coomCuamCid`: `_coom_cuam_cid`
- `coomReChoiceFlag`: `_coom_re_choice_flag`
- `coomMergeListCoomNo`: `_coom_merge_list_coom_no`
- `coomSellerMemo`: `_coom_seller_memo`
- `coomSellerGoodsTotalAmt`: `_coom_seller_goods_total_amt`
- `coomGoodsItemNum`: `_coom_goods_item_num`
- `coomGoodsTotalNum`: `_coom_goods_total_num`
- `coomRcvTotalAmt`: `_coom_rcv_total_amt`

### c_Order_C (Cart)
- `coocNo`: `_cooc_no`
- `coocPaymentType`: `_cooc_payment_type`
- `coocDeliverMethod`: `_cooc_deliver_method`
- `coocOrdChannelKind`: `_cooc_ord_channel_kind`
- `coocMemSid`: `_cooc_mem_sid`
- `coocOrdNameEnc`: `_cooc_ord_name_enc`
- `coocRcvNameEnc`: `_cooc_rcv_name_enc`
- `coocRcvMobileEnc`: `_cooc_rcv_mobile_enc`

### c_Order_D (Items - List)
- `coodName`: `_cood_name`
- `coodQty`: `_cood_qty`
- `coodOriginalPrice`: `_cood_original_price`
- `coodReceivePrice`: `_cood_receive_price`

---

## 技術檢核點
- [ ] 程式碼編譯通過（0 errors）
- [ ] 符合 `DBSDK.md` 架構基礎 §1 (層次結構)
- [ ] 專案 `PIC.CPF.OrderSDK.Biz.Read.Elastic` 零 Core 以外依賴
