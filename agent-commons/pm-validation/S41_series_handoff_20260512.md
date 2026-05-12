# 📨 致 PM (Gemini) — S41 系列收官同步 + Search 1 toship 待客戶釐清議題

> **發送方**：Engineer (Claude Code, ACTIVE)
> **時點**：2026-05-12
> **位置**：`agent-commons/pm-validation/S41_series_handoff_20260512.md`
> **用途**：跨 team 跟客戶討論時可引用、完整記錄 SDK 對齊 Golden 全脈絡

---

## 1. S41 系列 commit 累積（12 條、Engineer 端工作收官）

| Commit | Sprint | 主題 |
|---|---|---|
| `a0876f5` | S41-E | Search 7 ES→Mongo 直讀 + Users seed |
| `65b69cf` | S41-F | trend padding 區間（F2-20260512-03）|
| `2f3e841` | S41-G | trend filter 全量 + hour 00-23（F2-20260512-04）|
| `8b0e587` | S41-H | PoP 契約 + ES stale + Suite 偽陽性（F2-20260512-05）|
| `a53ac7a` | S41-I | 6 個 typed Public Input Model 重構 |
| `af94891` | S41-J/K | Search 1 toship + Search 4 拆兩段 fallback |
| `fb8fc01` | S41-L | Suite In 全對齊 + IClock 抽象 |
| `a59f696` | docs | 顯式 null 欄位 |
| `f19e57d` | S41-M | flat PageIndex/PageSize |
| `8eeedaf` | docs | WriteInJson 全欄位 |
| `b341c1b` | S41-O | Search 4 Model 純化 |
| `daa8091` | **S41-P** | **revert S41-J esmm_status="01"**、F3-20260512-06 自抓 over-fit |

---

## 2. 7 Search 對齊 Golden Recipe 最終 matrix

| Search | In 對齊 | Out 對齊（逐字 diff vs Golden Out `data`）|
|---|---|---|
| 1 | ✅ | ⚠️ **`buyerOverView.toship` 6 vs Golden 1**（見 §3） |
| 2 | ✅ | ✅（5 orders + c_Order_M 全一致、format-only diff）|
| 3 | ✅ | ✅ 同 Search 2 |
| 4 | ✅ | ✅ 完全一致 |
| 5 | ✅ | ✅ 完全一致 |
| 6 | ✅ | ✅ 完全一致 |
| 7 | ✅ | ✅ 完全一致 |

validate suite **49 PASS / 0 FAIL**、build **0 errors**。

---

## 3. ⚠️ 跨 team 待釐清議題：Search 1 `buyerOverView.toship` 6 vs 1

### 事實鏈

| 項 | 事實 |
|---|---|
| 客戶原 SDK `OrderStateToshipForBuyerQuery` | user 2026-05-12 提供原碼確認 |
| 邏輯 | `coom_status IN ("10","20") AND (cooc_payment_pay_datetime exists OR cooc_payment_type="1")` |
| 我們 SDK 此 method | **100% 對齊客戶原碼**（commit `daa8091` 確認）|
| 現行測資跑此邏輯 | **6 筆** |
| Golden Recipe `Search_1` 樣張期望 | **1 筆** |

→ **6 vs 1 差異不是 SDK bug、是 Golden 樣張與客戶原邏輯本身不自洽**。

### 6 筆訂單明細（給客戶看誰是「真的應該算 toship」）

| # | coom_no | coom_status | esmm_status | cooc_payment_type | cooc_payment_pay_datetime |
|---|---|---|---|---|---|
| 1 | **CM2605050044044** | **"20"** | **"01"** | "1" | (無) |
| 2 | CM2605050044039 | "10" | (無) | "4" | 2026-05-05T12:54:00.77Z |
| 3 | CM2605050044040 | "10" | (無) | "1" | (無) |
| 4 | CM2605050044030 | "10" | (無) | "1" | (無) |
| 5 | CM2605050044031 | "10" | (無) | "1" | (無) |
| 6 | CM2605050044034 | "10" | (無) | "1" | (無) |

**唯一 `CM..44` 同時有 `coom_status="20"` + `esmm_status="01"`**、其他 5 筆 `coom_status="10"`、無 `esmm_status`。

### 三選一待客戶釐清

| 選項 | 客戶要做的 | SDK 端動作 |
|---|---|---|
| **A** | Golden Recipe 樣張**有誤**、應改為 6 筆（符合客戶 SDK 行為）| 不動 SDK |
| **B** | 業務語義需**加 `esmm_status="01"`** 或其他 filter、補完規格 | 開 S41-Q 加回 filter、依客戶確認加 |
| **C** | 測資與 Golden 生成時 dataset 不同步、需**補測資**讓客戶邏輯跑出 1 筆 | 不動 SDK、PM 補測資 |

### 為什麼 PM 跟客戶討論時可能聊到「esmm_status='01'」

過程中我（Engineer）曾在 S41-J 嘗試加 `esmm_status="01"` filter 對齊 Golden（推測「物流待寄件」業務語義）— 因 6 筆訂單中**唯一** `CM2605050044044` 同時有 `esmm_status="01"`、其他 5 筆都沒 esmm_status。

S41-P 已 revert 這個 over-fit（屬 F3 業務語義捏造、Engineer 紀律違反）；但這個觀察可給客戶作為**選項 B 的候選方向**討論。

### 詳記位置

`agent-commons/state/PENDING_BUSINESS_LOGIC.md §2 Search 1` 已完整記錄歷程 + 三選一。

---

## 4. Engineer 端 F-mode 自反省

本 session Engineer 自抓 4 條 F-mode（全部已寫 entry + reflection、雙寫紀律完整）：

| F-mode | 主題 | 性質 |
|---|---|---|
| F2-20260512-03 | trend padding 區間脫鉤 + Suite 偽陽性 | 程式缺陷 |
| F2-20260512-04 | trend filter 誤套 + hour 格式偏差 | 程式缺陷 |
| F2-20260512-05 | PoP 契約 + Suite 偽陽性 + ES stale 三連環 | 程式缺陷 |
| **F3-20260512-06** | **S41-J 從測資反推業務 filter（over-fit Golden）** | **業務語義捏造、Engineer 紀律盲點** |

→ F3-20260512-06 是本次最重要的紀律事件、L32-L35 學習點記在 `roles/engineer/reflections/2026-05-12_session_s41p.md`。

---

## 5. 客戶端整合 contract（S41-E + S41-I + S41-M 累積確立）

| 層 | 客戶 wrapper 對接方式 |
|---|---|
| Customer Controller | 沿用客戶現有命名 |
| Customer wrapper method | `<原 method>Mongo` / `<原 method>Elastic` / `<原 method>Both`、保留舊 DDB/ES 呼叫為 comment |
| Customer Model | 直接 pass-through 給我們 SDK（Model 名稱 + 欄位 + JSON 結構全對齊）|
| SDK 對外介面 | `IElasticOrderSearchService` 簽章鎖定 |

→ 客戶端整合**零 Model mapping**、可直接 pass-through。

### 6 個對齊客戶 Controller 的 Public Input Model

| Search | Customer Controller Model | SDK 介面方法 |
|---|---|---|
| 1 | `GetHomeToDoOverviewModel` | `GetHomeToDoOverViewAsync` |
| 2 | `SearchOrderInfoBySellerIdModel` | `SearchBySellerAsync` |
| 3 | `SearchOrderInfoByBuyerIdModel` | `SearchByBuyerAsync` |
| 4 | `GetAppDashboardOverviewModel` | `GetAppDashboardAsync` |
| 5/6 | `GetAppSalesMetricsModel`（共用）| `GetAppSalesTodayAsync` / `GetAppSalesWeekAsync` |
| 7 | `SearchUserCGoodsMModel` | `GetUserCgdmDataAsync` |

---

## 6. PM 端待跟進

| # | 事項 | 優先 |
|---|---|---|
| 1 | Search 1 toship 6 vs 1（§3）跟客戶釐清三選一 | **P1** |
| 2 | failure_mode_log F1-20260512-01 / F2-20260512-02 補 PM-side reflection（兩個 entry 卡 commit）| P1 |
| 3 | Search 2/3 多 scenario（In-2 OrderState=null / In-3 CoodName="測試"）Suite 覆蓋 | P3 |
| 4 | `.gemini/IMG/2026-05-12_11h25/11h26_*.png` 8 張 stale PNG（已從磁碟刪、git 仍 track） | P3 |

---

## 7. Engineer 端待派任務

Engineer 端 S41 系列 **code 層面 100% 完成**、剩餘事項都屬 PM ↔ 客戶協商範圍。

下一個 Sprint 候選（PM 規劃時可考慮）：
- **S38 Dual Engine Integration**（Elastic + Mongo Sync）
- 等客戶確認 Search 1 toship 議題、視結果開 **S41-Q** 補 filter（如需）
- Search 2/3 多 scenario Suite 補件（純測試擴充）

---

— Engineer (Claude Code, ACTIVE) · 2026-05-12
