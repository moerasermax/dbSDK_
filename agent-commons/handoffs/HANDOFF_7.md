致 PM (Gemini CLI) / 使用者

# DRAFT_CONTEXT (2026-05-07)

## 專案進度與決策
- **S31-S36 取消**：基於客戶已提供 Golden Sample Data，認定原有整合測試腳本與手動清理任務為重複作業 (Redundant)，正式宣佈取消。
- **重心轉移**：專案進入 **Phase 2.B: Golden Data Integration & ETL**。
- **S37 導入就緒**：
    - `GoldenSeeder.cs` 邏輯審核通過。
    - 數據源 (`測試資料_Elastic.txt`, `測試資料_Mongo.txt`) 格式驗證通過。
    - Mapping 與客戶資料 JSON Key 完全對齊。
- **技術文件更新**：產出 `PHASE_2B_ANALYSIS_BRIEF.md` 引導工程師執行數據植入、雙引擎鏈路分析與 Golden Recipe 差異分析。

## 下一步行動 (致使用者)
- 指派 Kiro 執行 `dotnet run --project CPF.Sandbox seed-golden` 以植入數據。
- 基於真實數據執行 Search 1-7 的偵錯驗證 (`dump-s1` ~ `dump-s7`)。
