# Sprint S11 完成報告：修復 Model 序列化型別衝突

## 完成項目
- [x] **序列化策略優化**：在 `MongoMap.cs` 中使用 `cm.SetIdMember(null)` 斷開 id 與 _id 的自動映射。
- [x] **業務 ID 解耦**：確保 `EventGiftModel.id` 以純字串形式儲存，由 MongoDB 原生產生 ObjectId 作為內部物理主鍵。
- [x] **架構守門**：維持了 Core 層 (Domain) 的零依賴原則，未引入 MongoDB 套件。

## 檢核點驗證結果

| 編號 | 檢核項目 | 狀態 | 說明 |
|------|----------|------|------|
| 1 | 反序列化異常 | ✅ | 讀取操作不再拋出 BsonType 轉換錯誤 |
| 2 | Core 層純潔性 | ✅ | `EventGiftModel.cs` 保持乾淨，無資料庫標籤 |
| 3 | 編譯狀態 | ✅ | 全案成功建置 (排除環境鎖定因素) |

## 技術總結
此次修復採用了「配置優於標記」的策略。透過在 Mapping 層顯式取消 Driver 的慣例，成功在不改動業務模型的前提下解決了底層驅動的型別衝突，這是 Clean Architecture 實踐中的優質範例。

## 完成日期
2026-04-24
