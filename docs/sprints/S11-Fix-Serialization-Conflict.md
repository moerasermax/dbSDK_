# Sprint S11：修復 EventGiftModel.id 的 ObjectId 序列化衝突

## 任務目標
解決 `EventGiftModel.id` 被 MongoDB Driver AutoMap 誤判為 `_id` (ObjectId) 的序列化衝突，確保 Insert 操作不拋出 `BsonSerializationException`。

## 需求背景
MongoDB Driver 的 `AutoMap()` 有一個慣例：任何名為 `id`、`Id`、`_id` 的屬性都會被自動映射為文件的 `_id` 欄位，並預設使用 ObjectId 序列化器。`EventGiftModel.id` 是業務字串欄位（非資料庫主鍵），因此需要明確告知 Driver 不要將其視為 `_id`，改以普通 String 欄位儲存。

## 根本原因
- `EventGiftModel.id` 屬性名稱觸發 MongoDB Driver 的 `_id` 自動映射慣例
- `AutoMap()` 後，Driver 嘗試將字串值（如 `"EG123456"`）序列化為 24 位元 ObjectId，導致 `BsonSerializationException`
- `MongoMap.cs` 中雖已呼叫 `cm.MapMember(c => c.id).SetSerializer(new StringSerializer(BsonType.String))`，但 `AutoMap()` 已先將其設為 `_id`，`MapMember` 無法覆蓋 `_id` 的映射行為

## 任務清單

### 1. 修復 MongoMap.cs
- [x] 在 `EventGiftModel` 的 ClassMap 中，使用 `cm.SetIdMember(null)` 取消自動 `_id` 映射。
- [x] 改用 `cm.MapMember(c => c.id)` 將 `id` 作為普通欄位，並設定 `StringSerializer(BsonType.String)`。
- [x] 確保 MongoDB 原生 ObjectId 仍由 Driver 自動產生（不指定任何 `_id` 映射）。

### 2. 驗證
- [x] 執行 `dotnet build` 確認 0 個 `error CS`（無程式碼錯誤）。
- [x] `EventGiftModel` 序列化後 `id` 欄位為普通字串，不再被映射為 `_id`。

## 檢核點
- [x] `EventGiftModel` Insert 不拋出 `BsonSerializationException`（序列化衝突已解除）。
- [x] 資料庫文件包含原生 ObjectId 的 `_id` 欄位（由 Driver 自動產生）。
- [x] `id` 欄位以普通字串形式存在於文件中，不與 `_id` 重疊。

## 完成日期
2026-04-24
