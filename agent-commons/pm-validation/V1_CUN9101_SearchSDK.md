# PM 驗收工作單｜dbSDK × CUN9101 ElasticSearch 搜尋功能

**版本**：S30　**日期**：2026-05-03  
**驗收範圍**：Sprint S23–S30（詳細驗收項目與簽核紀錄見 `agent-commons/capsules/S30_Final_Example_Tests.md`）

---

## 一、執行前確認

跑以下三項，全部 OK 再繼續：

| 項目 | 操作 | OK 條件 |
|------|------|---------|
| Docker | 終端機輸入 `docker ps` | 看到 ES / Kibana / Mongo / Redis 全部 Up |
| ES 狀態 | 開 http://localhost:5601 → Dev Tools → 輸入 `GET /_cluster/health` → 執行 | `"status": "green"` |
| 專案建置 | 終端機輸入 `dotnet build CPF.Sandbox` | 最後一行顯示 `0 個錯誤` |

---

## 二、步驟一：植入測試資料

終端機執行：

```
dotnet run --project CPF.Sandbox seed
```

Console 最後出現：
```
完成：成功 30 筆，失敗 0 筆
```

接著開 Kibana Dev Tools，執行：
```
GET orders-604/_count
```

**✅ 通過條件**：回傳 `"count": 30`

---

## 三、步驟二：執行查詢驗證

終端機執行：

```
dotnet run --project CPF.Sandbox validate
```

Console 會對每個情境逐一印出結果，每個欄位前有 `✅ PASS` 或 `❌ FAIL`。

**✅ 通過條件**：全部 PASS，沒有任何 FAIL

---

## 四、有 FAIL 時的回報格式

複製以下格式給 Engineer：

```
[FAIL] S2X — 情境名稱
欄位：xxxField
期望：（GoldenRecipe 的值）
實際：（Console 印出的值）
```

---

## 五、完成確認清單

| 項目 | 狀態 |
|------|------|
| Docker 全部 Up | ⬜ |
| ES status green | ⬜ |
| 資料植入 30 筆 | ⬜ |
| Kibana count = 30 | ⬜ |
| S23 全部 PASS | ⬜ |
| S24 全部 PASS | ⬜ |
| S25 全部 PASS | ⬜ |
| S26 全部 PASS | ⬜ |
| S27 全部 PASS | ⬜ |
| S28 全部 PASS | ⬜ |
| S29 全部 PASS | ⬜ |

**全部打勾 → Sprint S23–S30 驗收通過，通知 Engineer 結案。**
