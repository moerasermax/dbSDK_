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

## Phase 2 待辦：真 pipeline ETL 整合測試

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
