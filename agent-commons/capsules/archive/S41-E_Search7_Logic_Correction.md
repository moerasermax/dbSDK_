# Sprint S41-E：Search 7 邏輯修正 (棄用 ES 聚合，改採 Mongo 單筆查詢)
tracking_label: P2B-VALIDATE-1 / S41-E

## 任務目標
修正 Search 7 (`GetUserCgdmData`) 的資料來源邏輯。根據客戶最新源碼截圖，原程式碼是從 DynamoDB 直接讀取單筆 `UserModel`，而非從 ElasticSearch 聚合 `_ord_modify_date`。本任務需將其改為從 MongoDB 直接讀取對應的資料，解決 P1 Blocker。

---

## ⚠️  ASSUMPTION (前提假設)
- **資料源假設**：本方案假設 MongoDB 中存在 `User` / `Cgdm` 相關的 Collection，且包含與 Golden Recipe 對齊的欄位（如更新時間）。
- **實證現況**：(資料源已到位、2026-05-12 補測完成)。補測檔案 `測試資料_Mongo_User.txt` 內含 `cuam_cid=528672` 及更新時間。

---

## 整合策略 (Integration Strategy)
為了維持客戶端既有代碼的審計追蹤 (Audit Trail) 與低侵入性，採用 **Wrapper Pattern**：
1. **客戶端變動**：客戶端 `SearchController` 或 `SearchBll` 新增對應方法，命名規範如下：
    - 僅呼叫 MongoDB：`<原 method 名稱>Mongo`
    - 僅呼叫 ElasticSearch：`<原 method 名稱>Elastic`
    - 雙引擎同時使用 (Dual Engine)：`<原 method 名稱>Both`
2. **審計保留**：保留原有的 DDB/ES 呼叫代碼並以註解形式存在。
3. **SDK 介面**：我們 SDK 的對外介面 (`IElasticOrderSearchService`) 保持不變，僅在 BLL 內部切換實作路徑。

---

## 需求背景
原先以為 Search 7 的 `cgdmUpdateDatetime` 需從 ES 的 `_ord_modify_date` 欄位聚合最大值，但客戶提供的 `測試資料_Elastic.txt` 無此欄位，導致驗收卡關 (P1 Blocker)。
經核實客戶源碼，原實作為 `_searchDal.GetUserByCuamCidFromDDB(CuamCid)`，直接呼叫 DynamoDB `LoadAsync`。在目前的雙引擎架構下，應將此邏輯平行轉移至 MongoDB。

---

## 任務核准狀態 (Co-sign)

> **規則**：PM 驗證項目需雙方簽核後，本 Capsule 才可進入 ACTIVE 狀態開工。

- [x] **PM**：方案符合客戶源碼邏輯，資料源已實證到位 (Gemini CLI 2026-05-12 - 重新簽署)
- [x] **Engineer**：膠囊資料源 / 整合策略 / 時區規則 / 檔名類別名 4 項補正抽驗全綠（PM 連續綠燈累積 +1）、User 已下「你先登入」明示授權動工 (Claude Code 2026-05-12)
- **核准日期**: 2026-05-12
- **狀態**: `ACTIVE` — 使用者授權，開始執行更新與邏輯修正。

---

## 任務清單

### 1. [x] 檢視 MongoDB 測試資料 (歷史紀錄)
- **結果**：資料源已補全於 `測試資料_Mongo_Order.txt` / `測試資料_Mongo_User.txt`。

### 2. [x] 修正 BLL 邏輯
- 檔案：`PIC.CPF.OrderSDK.Biz.Read.Elastic/BLL/ElasticOrderSearchBll.cs` (line 260-280)
- `GetUserCgdmDataAsync` 從 `_dal.GetUserCgdmDataAsync(cid)` 改為 `_mongoSearchDal.GetUserByCuamCidFromMongoAsync(cid)`、原 ES 聚合 helper 保留為 audit trail

### 3. [x] 實作 Mongo DAL 查詢
- 檔案：`PIC.CPF.OrderSDK.Biz.Read.Elastic/DAL/MongoSearchDal.cs` + `Models/Internal/MongoUser.cs` (新增)
- `MongoSearchDal` ctor 加注入 `IMongoCollection<MongoUser>`、新增 `GetUserByCuamCidFromMongoAsync(int cuamCid)` 單筆 Find
- `MongoUser` schema 依 `測試資料_Mongo_User.txt`：`cuam_cid` (string)、`c_goods_m[]`（含 `cgdm_id` / `cgdm_update_datetime`）
- DI 註冊改在 `CPF.Sandbox/Scenarios/SearchSdkSetup.cs`、加 `mongoUserCollection = "Users"` 參數
- Seed 流程在 `CPF.Sandbox/IntegrationTests/PipelineSeeders/GoldenSeeder.cs` 加 `SeedMongoUserAsync` + 拆共用 `SeedMongoCollectionAsync` helper、檔名同步更新（`測試資料_Mongo.txt` → `_Order.txt` / `_User.txt`）

### 4. [x] 修正 Convert Helper
- 檔案：`PIC.CPF.OrderSDK.Biz.Read.Elastic/Extension/ConverToExtension.cs`
- 新增 Mongo overload：`ConvertToUserCgdmDataResultModel(this MongoUser?, int cuamCid)`
- 時區處理：`SpecifyKind(Utc)` → `ConvertTimeFromUtc(TaipeiTz)` → `ToString("yyyy-MM-ddTHH:mm:ss.fff")`、無 Z 後綴
- `TaipeiTz` resolver 採 3 段 fallback：`Asia/Taipei` → `Taipei Standard Time` → CustomTimeZone(+8h)（不動既有 `DateTimeNoZConverter`、防 Search 1-6 連帶影響）

---

## PM 驗收項目 (VCP)

| # | 驗證項目 | 驗證方式 | 期望值 | Engineer 自驗 |
|---|---------|---------|--------|----------|
| 1 | **架構對齊** | Code Review | Search 7 完全移除對 ES `_ord_modify_date` 的依賴，改採 Mongo 單筆直讀 | ✅ BLL 263-280 改呼叫 `_mongoSearchDal.GetUserByCuamCidFromMongoAsync`、不再走 `_dal.GetUserCgdmDataAsync` |
| 2 | **資料正確性** | 執行 `dump-s7` | 輸出之 `cgdmUpdateDatetime` 符合 MongoDB 補測資料、對齊 Golden Recipe 時區 | ✅ 4 項 Check 全 PASS：cuamCid=528672 / count=2 / GM2508260014245 @ 14:35:51.775 / GM2512180014259 @ 14:35:36.628 |

---

## 技術檢核點
- [x] 遵守 `DBSDK.md Part I §A` (MongoMap 註冊檢查)：`MongoSearchDal` ctor 注入 `MongoMap` 觸發兩階段靜態初始化
- [x] 遵守 `DBSDK.md Part I §F` (核心層不引用外部套件)：新增碼僅在 `PIC.CPF.OrderSDK.Biz.Read.Elastic` 與 `CPF.Sandbox`、Core 層零變動
- [x] 嚴禁修改非相關的 Search 1-6 邏輯：未動 `DateTimeNoZConverter` / `OrderSearchDal.*` / `ConvertToUserCgdmDataResultModel(ES)` 等 Search 1-6 路徑檔
- [x] Build 0 errors（98 warnings 均為既有 baseline、新增碼零警告貢獻）
