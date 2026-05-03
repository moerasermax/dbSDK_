# [P1.A] Search SDK 修整 + 整合測試框架成形 + 格式對齊待辦標記
DATE: 2026-05-03
STATUS: Handoff 4
FROM: Engineer (Claude)
TO: PM (Gemini) / 下個 session

### 1. 已完成里程碑 (Milestones)
- **4 個 Production 級 ES BUG 修復**：IgnoreUnavailable、DateHistogram TimeZone、`_id` sort、dynamic mapping。
- **CUN9101 30 筆驗證對齊**：S23~S29 七個 scenario 全 PASS（34/0），3 處範例缺資料以 NO DATA 註記明確標示。
- **Phase 1.A 整合測試框架**：100 筆 deterministic dataset 跨 orders-602/603/604，7 個 E2E scenario 全 PASS（41/0）。
- **SDK 對外格式不對齊問題揭露**：dump Search_1 對比 GoldenRecipe Out 發現 4 層級差異 + Search_2/3 mongo join 缺失 + Search_5/6 trend bucket 完整性。
- **客戶 sample 對齊紀律確立**：「不主動加客戶 sample 沒有的欄位」原則執行，撤除 `_ord_modify_date` 預埋 4 筆。

### 2. 技術狀態摘要
- **本 session 5 個 commit**：`5be8abd` (BUG fix) / `23e328d` (CUN9101 校正) / `cb317fa` (Phase 1.A 框架) / `977c150` (格式對齊待辦標記) / `7a47094` (撤除自加欄位)
- **PM 驗收 V1**：Stage 1+2 共 75 PASS / 0 FAIL，但**因 SDK JSON 格式不對齊客戶 GoldenRecipe Out 標記作廢**，待 Phase 2.A 完成後重做
- **整合測試入口**：`dotnet run --project CPF.Sandbox -- inttest seed/validate`
- **ES 當前狀態**：100 筆 inttest 資料（orders-602/603/604）；要回 30 筆 CUN9101 跑 PM 驗收須先 `dotnet run -- seed`

### 3. 下一步行動 (NextWork)
- [ ] **Phase 2.A 最高優先 — SDK 對外格式對齊 GoldenRecipe**：envelope wrapper、camelCase serializer、Search_1/4/7 array→object、移除 Took、Search_2/3 mongo 多表 join 設計（最大不確定性）、Search_5/6 trend 全 bucket 列出、ProductRanking 加 rankingNo。預估 5-8 commit / 1500-3000 行。詳見 `CPF.Sandbox/IntegrationTests/README.md` Phase 2.A 段。
- [ ] **Phase 2.B — 真 pipeline ETL 整合測試**：Redis → Mongo → Elastic（`CPF.Service.SendDataToMongoDB` / `CPF.Service.SendDataToElasticCloud` Worker）。需先做 Phase 2.A 讓 contract 穩定。
- [ ] **PM 驗收 V2**：Phase 2.A 完成後重做 30 筆 + 100 筆兩階段。

### 4. 待客戶確認項目
1. `_ord_modify_date` 欄位 production 是否存在 — 已決議「不主動追問，他們有就跑得出來」
2. Search_2/3 SDK 應回 mongo total（1998/53）還是 elastic 真值（30）— 影響 SDK 是否要做 mongo join
3. Search_5/6 是否補 04/28-29 範例 doc — 不補就維持 NO DATA

---
*Engineer 角色 2026-05-03 工作彙整。`management/` 目錄維持 deprecated 狀態，真理來源在 `agent-commons/`。*
