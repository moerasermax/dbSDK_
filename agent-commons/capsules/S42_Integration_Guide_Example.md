# Sprint S42：SDK 串接教學與 Mongo 進階更新範例 (Integration Guide & Advanced Samples)

## 任務目標
提供完整的 SDK 串接手冊，並實作具備業務參考價值的 MongoDB 局部更新 ($set/$unset) 範例。

## 需求背景
dbSDK 已具備強大的雙引擎查詢與 Mongo 扁平化更新能力，但缺乏對外展示如何「正確串接」與「處理複雜更新」的範例，這將直接影響交付後的開發者體驗。

## 任務清單
- [ ] **手冊更新 (`docs/SDK_QuickStart.md`)**：
    - 更新「註冊範例」，改用 S45 實作的 `AddDbSdk()`。
    - 新增「雙引擎 (Dual Engine) 查詢模式」說明，解釋 Search 1-7 的資料流向。
    - 增加「MongoDB 局部更新指南」，包含 `$set` 與 `$unset` 的業務情境。
- [ ] **實作進階範例 (`CPF.Sandbox/Scenarios/AdvancedUpdateScenario.cs`)**：
    - 建立一個新的 Scenario 類別。
    - 範例情境：當訂單狀態從「待支付」變為「已支付」，需 `$set` 狀態與支付時間，同時 `$unset` (移除) 原本的「支付逾期提醒」欄位。
    - 演示如何使用 `MongoRepository.UpdateData` 一次性執行複合操作。
- [ ] **範例整合**：
    - 將此新 Scenario 加入 `SandboxRunner` 的選單中，方便使用者一鍵執行。

## 檢核點 (VCP)
- [ ] **文件一致性**：`SDK_QuickStart.md` 的範例碼可通過建置。
- [ ] **功能實證**：執行 `AdvancedUpdateScenario`，並透過 `mongo` shell 或 `Studio 3T` 觀察欄位是否確實被 $set 與 $unset。
- [ ] **健壯性**：確認 `FlattenBsonDocument` 仍能正確忽略 null 欄位。

## 完成日期
2026-05-14
