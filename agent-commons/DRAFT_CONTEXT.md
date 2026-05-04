# DRAFT_CONTEXT (2026-05-03)

## 1. 當前狀態 (Current State)
- **PM 狀態**: ACTIVE (Gemini)
- **解耦評分**: 10 / 10
- **驗收狀態**: **V1 已廢棄 (INVALIDATED)**
- **主要風險**: SDK 輸出 JSON 格式與客戶 GoldenRecipe 不匹配 (Shape/Casing/Wrapper)。

## 2. 進行中任務 (Active Sprints)
- **Phase 2.A (正式啟動)**: SDK 格式與資料全對齊 (方案 B)。
- **目標**: 實作 `ApiResponseWrapper<T>`，全面對齊 `camelCase`，並透過 Mongo 補強 Search_2/3 資料。
- **治理**: 依據 2026-05-03 修正版 P2 Tracker 執行。

## 3. 領域公理校驗 (Axiom Verification)
- [x] 兩階段初始化
- [x] 日期雙軌制
- [x] 層次邊界 (Axiom F 已由 Engineer 徹底淨化)

---
*V1 Acceptance invalidated due to JSON Contract mismatch.*
