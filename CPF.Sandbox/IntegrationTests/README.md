# 整合測試 (Integration Tests)

## Phase 1.A 已完成（2026-05-03）

100 筆 deterministic 假資料，跨 orders-602/603/604 三個 monthly index，驗證 Search Read SDK 的 7 個 Search 端對端一致性。

### 怎麼跑
```
dotnet run --project CPF.Sandbox -- inttest seed       # 重建 ES + 植入 100 筆
dotnet run --project CPF.Sandbox -- inttest validate   # 跑 7 個 E2E scenario
```

### 架構
```
IntegrationTests/
├── DataFactory/
│   ├── TestDataset.cs                  # 100 筆 + 元資料 容器
│   ├── ScenarioPresets.cs              # 18 筆預埋（涵蓋 S5/S6/S7 必要場景）
│   ├── OrderTestDataFactory.cs         # 80 background random + presets = 98+2 = 100 筆
│   └── ExpectedValueCalculator.cs      # 從 dataset LINQ 反算 Search 1~7 expected
├── PipelineSeeders/
│   └── ElasticSeeder.cs                # 跨月 index 路由 + explicit mapping
└── Scenarios/
    └── E2E_S1~S7_*.cs                  # 7 個 E2E scenario
```

### 設計原則
- **Deterministic seed**：`OrderTestDataFactory.Build(seed: 42)` 永遠產生同樣 100 筆
- **Source of Truth = Dataset**：ExpectedCalculator 跟 ES 各自從 dataset 算/查，比對應該完全一致
- **資料格式對齊客戶 CUN9101 範例**：欄位選擇邏輯與客戶 30 筆樣本對齊（pay_datetime/esmm_rcv 只在特定 status 出現）
- **預埋必要 deviation**：`_ord_modify_date` 客戶範例缺，預埋 4 筆對齊 S7 GoldenRecipe

### 已修的 BUG (production 也受影響)
1. `ElasticRepository.AdvancedSearchAsync` 沒設 IgnoreUnavailable（跨月 index 崩潰）
2. `OrderSearchDal.AggregateHelpers.AppSalesMetricsTrend` DateHistogram 沒設 TimeZone(+08:00)
3. `OrderSearchDal.Sort` `_id` 不能 sort，改 coom_no
4. SeedScenario 用 dynamic mapping，改 explicit (keyword + nested)

---

## Phase 1.B 待辦：Mongo + Redis Seeder（如果未來需要 Read 跨儲存層）

當前只驗 Elastic 端。如果 Search SDK 未來需要 fallback 到 Mongo（例如某些 query 走 mongo），需加：
- `PipelineSeeders/MongoSeeder.cs` — 把 100 筆寫到 mongo collection
- `PipelineSeeders/RedisSeeder.cs` — 把 hot data 寫到 Redis

---

## Phase 2.A 待辦：SDK 對外輸出格式對齊 GoldenRecipe（**最高優先**）

2026-05-03 PM 驗收路上發現 SDK 回傳 model 格式跟客戶 GoldenRecipe Out **不一致**，
經客戶決議「以客戶 GoldenRecipe 為準」，整個 SDK 輸出層需要重新設計：

### 已確認 4 個層級差異（以 Search_1 為 sample）
1. **缺外層 wrapper**：GoldenRecipe `{data, code, message, errorMsg, total}`，SDK 直接返回 data
2. **Container 形態**：GoldenRecipe Search_1/4/7 用 single object，SDK 用 array
3. **Property Casing**：GoldenRecipe camelCase（`buyerOverView`），SDK PascalCase（`BuyerOverview`）
4. **多餘欄位**：SDK 多 `Took`，GoldenRecipe 沒有

### 工作清單
- [ ] 設計外層 wrapper class `ApiResponseWrapper<T>` (含 code/message/errorMsg/total/data)
- [ ] 7 個 BLL method 回傳值改包 wrapper（影響 `ElasticOrderSearchBll.cs`）
- [ ] 全部 Result Model 加 `[JsonPropertyName("camelCase")]` 對齊客戶欄位名
- [ ] Search_1/4/7 的 Array → Object 轉換（其他 Search 是 array 不變）
- [ ] 移除 `Took`（或加 `[JsonIgnore]`）
- [ ] 7 個 Search 各跑一次 dump 對比 GoldenRecipe 逐欄位確認
- [ ] Sandbox scenario S23~S29、E2E_S1~S7 全部對齊新 shape
- [ ] ExpectedValueCalculator 對齊新 shape
- [ ] 重新給 PM 驗收（含 30 筆 + 100 筆兩階段）

工程量估計：3-5 個 commit、800~1500 行修改。

### 注意
- 不影響 BLL 的 query / aggregate 邏輯（4 個已修 BUG 仍有效）
- 客戶可能還沒補資料（_ord_modify_date / 04/28-29 doc / Search_2/3 mongo-elastic 對齊問題），
  但這個格式對齊不需要等客戶補資料就能做
- PM 驗收 V1 (2026-05-03) 雖然 32+41 PASS，但**因格式不對齊已標記作廢**，下個 session 改完後重來

---

## Phase 2.B 待辦：真 pipeline ETL 整合測試

當前是「假 pipeline」— 直接寫 Elastic。Phase 2 要驗證 **Redis → Mongo → Elastic 的 ETL 邏輯本身**：

1. 改 SeedScenario 為「寫入 Redis」入口
2. 啟動 `CPF.Service.SendDataToMongoDB` Worker，驗證 Redis → Mongo 同步
3. 啟動 `CPF.Service.SendDataToElasticCloud` Worker，驗證 Mongo → Elastic 同步
4. 整合測試 = ETL 跑完後再跑 Search SDK，跟 ExpectedCalculator 比對

注意：
- ETL Worker 是長駐 service，整合測試要能 start/stop
- Redis schema 需確認（hash? list? stream?）
- ETL 可能有資料轉換邏輯（例如 mongo → elastic 欄位 mapping），ExpectedCalculator 要看 mongo 還是 elastic 的 view 算

---

## 與 CUN9101 30 筆驗證的關係（並行）

- `Scenarios/S23~S29_*.cs` (30 筆) — 對齊客戶範例的「兼容性測試」
- `IntegrationTests/Scenarios/E2E_S*.cs` (100 筆) — 完整邏輯覆蓋的「整合測試」

兩者並存。30 筆驗證仍跑 `dotnet run -- validate`，100 筆跑 `inttest validate`。
