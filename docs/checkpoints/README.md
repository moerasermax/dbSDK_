# Checkpoints 系統

這個資料夾用於存放專案進度檢核點，讓 AI 助手能快速了解專案狀態。

## 使用方式

當你想要讓 AI 助手了解專案進度時，可以說：
- "請載入 checkpoint"
- "/checkpoints load"
- "查看 S16 進度"

## 現有 Checkpoints

| 檔案 | Sprint | 描述 |
|------|--------|------|
| `S16-Elastic-Enhancement-checkpoint.md` | S16 | Elastic 強型別濾鏡與 Mock 體系 |

## Checkpoint 格式規範

每個 checkpoint 應包含：
1. **專案結構概覽** - 關鍵檔案位置
2. **任務清單** - 已完成/進行中/待辦
3. **編譯狀態** - 各專案編譯結果
4. **已知問題** - 警告、錯誤、技術債
5. **下一步** - 接下來要做的事
