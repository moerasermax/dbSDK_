---
description: Charter Doctor (人工健康檢查) - 對 agent-commons/ 進行完整 schema 驗證，支援 Mode A (人工) / Mode B (self-instantiation) / Mode C (Gap 修復)
argument-hint: "[mode] [fix]"
---

# /charter-doctor - AgentCharter 健康檢查

> **描述**：charter doctor 對本專案的 agent-commons/ 進行完整健康檢查
> 
> **模式**：
> - **Mode A** (人工健康檢查)：dry-run，顯示所有 errors/warnings
> - **Mode B** (self-instantiation)：init step 5 強制驗證，0 errors 才允許 step 6
> - **Mode C** (Gap 修復)：自動修復 W120x 問題
>
> **AI Vendor**: Claude Code (MiniMax M2.5)
> **Charter**: AgentCharter v0.9.9
> **Common Memory Root**: gent-commons/

---

## Mode A: 人工健康檢查 (dry-run)

執行以下檢查並回報：

### 1. 系統配置
`
# 檢查 profile.yaml
Read agent-commons/_config/profile.yaml
  - charter_version: ?
  - preset: (standard/strict)
  - enable_modes: (不應包含 F6)

# 檢查 mapping.yaml
Read agent-commons/_config/mapping.yaml
  - common_memory_root: ?
  - shared.* 佈局
  - roles.* 結構
`

### 2. Domain Axioms
`
# 檢查 primary axiom
Read agent-commons/protocols/DBSDK.md (或 mapping.domain_axioms.primary)
  - status 應為 USER-RATIFIED (非 AI-DRAFTED)
  - mutability_default 應為 APPEND-ONLY
`

### 3. Init Slash Commands
`
# 檢查各 AI vendor 的 init slash command
ls -la .claude/commands/*-init.md
ls -la .gemini/commands/*-init.toml
ls -la .cursor/rules/*-init.mdc
`

### 4. Roles 狀態
`
# 檢查各 role 的 _role.md
Read agent-commons/roles/pm/_role.md
  - Status: PROVISIONAL → ACTIVE
  
Read agent-commons/roles/engineer/_role.md
  - Status: PROVISIONAL → ACTIVE
`

### 5. Failure Modes
`
# 檢查 failure_mode_log
Read agent-commons/state/failure_mode_log.md
  - 確認有 F-mode entries
`

### 6. Output Mode
`
# 檢查 current output mode
cat agent-commons/state/output_mode
`

---

## Mode B: Self-Instantiation 驗證

用於 init step 5，強制驗證：

1. **E001/E002**: profile.yaml / mapping.yaml 存在
2. **E003**: profile schema 合法性
3. **E004**: charter_version 一致性
4. **E401**: domain-axiom 存在
5. **E605**: enable_modes 不含 F6 (standard/strict preset)
6. **E606**: axiom status = USER-RATIFIED

**規則**：0 errors 才能進入 step 6 簽名

---

## Mode C: Gap 修復

自動修復：

- **W1201**: 平行對話問題 (kiro-sync-point-*.md → sessions/)
- **W1202**: handoffs/ 結構問題
- **W1203**: capsules/ 結構問題
- **W1204**: institutional-memory/ 結構問題
- **W1205**: failure_mode_log 格式問題

---

## 健康檢查報告格式

`
# AgentCharter Health Report > <project-name>

> 日期：<UTC>
> Charter version：<X.Y.Z>
> Profile preset：<standard|strict>
> 異常：<N> errors / <M> warnings

## 1. 系統配置
[profile.yaml / mapping.yaml 內容]

## 2. Domain Axiom
[axiom frontmatter status]

## 3. Init Slash Commands
[各 vendor init 命令狀態]

## 4. Roles
[各 role _role.md 狀態]

## 5. Failure Modes
[failure_mode_log 摘要]

## 6. 異常清單
- E<XXX>: <描述>
- W<XXX>: <描述>
`

---

## 執行範例

`
/charter-doctor          # Mode A: 人工健康檢查
/charter-doctor mode-a   # 同上
/charter-doctor mode-b   # Self-instantiation 驗證
/charter-doctor mode-c   # Gap 自動修復
/charter-doctor fix      # Mode C 自動修復
`

---

## 版本資訊

- **2026-05-04 v1.0** - 初始版本，基於 AgentCharter v0.9.9 	ools/doctor-spec.md §2.1 模式 A
