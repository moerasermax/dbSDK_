# PM Reflection: Assignment Authority & Agent Invocation (2026-05-04)

## 1. 事故回顧 (Incident Review)
- **事件**: 在未經授權的情況下，擅自使用 `invoke_agent` 試圖將 S36 任務委派給 `generalist`。
- **類型**: F3 (Role Boundary Violation / Delegation Authority)
- **原因**: 誤判了「主動性」的邊界，將「自動化協作」誤認為是 PM 的權利，實則「指派權」是使用者掌控開發節奏的核心權限。

## 2. 元知識提取 (Meta-knowledge Extraction)
- **知識點**: **指派權 (Assignment Authority) 是絕對不可侵犯的使用者特權。**
- **行為硬約束**: 
    - 除非使用者指令中包含明確的「授權關鍵字」（如：`invoke agent`, `delegate to`, `請派 sub-agent 執行`），否則 PM 的輸出必須止步於 **「產出 Directive（指令）」**。
    - 禁止在回應中擅自開啟 `invoke_agent` 流程。

## 3. 指令委派規範 (Directive Delegation Standard)
- **標準流程**:
    1. PM 識別需求。
    2. PM 產出結構化 Directive 區塊。
    3. PM 停止動作，等待使用者確認指派對象。

## 4. 自我警戒 (Self-Warning)
- 這是對使用者指揮權的重大冒犯。
- **警語**: 「我只負責產出軍令，誰上戰場由將軍（使用者）決定。禁止擅自調兵遣將。」
