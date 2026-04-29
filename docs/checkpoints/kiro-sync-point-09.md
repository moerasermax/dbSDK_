# Kiro 技術同步節點 - 09

## [當前狀態]
- **任務編號**：S19 (取號事件欄位缺失修正)
- **優先級**：P0 (阻礙物流流程閉環測試)
- **同步日期**：2026-04-28

## [技術診斷總結]
1. **斷點**：`UpdateSellerGetNumberEvent` 在從 Redis 轉入 MongoDB 時，因 `SellerGetNumberArgs` 定義過窄且 `Worker.cs` 映射硬編碼，導致 `e_shipment_m` 僅存入 `esmm_ship_no` 與 `esmm_status`。
2. **影響範圍**：物流子單號 (`esmm_ship_no_a`) 與驗證碼 (`esmm_ship_no_auth_code`) 丟失。

## [防禦性檢核結論]
- **安全性**：`MongoRepository` 的 `Flatten` 邏輯已確認支援點符號路徑更新，擴充 `E_Shipment_M` 映射不會覆蓋其他欄位（個資、明細）。
- **日期處理**：`Recursive Normalization` 支援字串日期轉 `BsonDateTime`，相容 Redis 傳入的 ISO 格式。

## [待執行行動 (Next Actions)]
1. 擴充 `CPF.Services.Redis.Post` 的事件 Model。
2. 補齊 `AddOrderEventRandomDataGenerator` 的 Mock 欄位。
3. 修改 `Worker.cs` 使用全物件映射。
