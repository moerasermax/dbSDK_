致 首席工程師 (Claude) / 使用者

# DRAFT_CONTEXT (2026-05-07) - Post S40

## 專案狀態 (Project State)
- **Sprint S40 (公用模型重構)**：✅ 已結案 (CLOSED)。完成 12 層巢狀模型對齊與 camelCase 格式化。
- **Sprint S41 (Golden Recipe 驗收)**：🚀 啟動中 (ACTIVE)。目標為數值校準、時區修正與 Search 1 邏輯修復。

## 關鍵改動 (Key Changes)
- **Infrastructure**: 新增 `SearchOrderInfoDataModel.cs`，重構 `ConverToExtension.cs`。
- **Governance**: 修正 PM 激活協定與 S40 結案簽核。
- **Tools**: `dump-s2/s3` 現支援 verbose 輸出。

## 下一步行動 (Next Actions)
- 首席工程師 (Claude) 執行 S41 模型擴充與 DateTime 格式化。
- 修正 Search 1 統計誤差。
