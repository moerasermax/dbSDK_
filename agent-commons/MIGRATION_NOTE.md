# Migration Note: Upgrade to AgentCharter v0.9.9

- **Date**: 2026-05-04
- **Action**: Upgrade framework from v0.9.4 to v0.9.9.
- **Changes**:
    - Synchronized `~/.agentcharter` to latest main.
    - Updated `profile.yaml` version to `0.9.9`.
    - Refactored `mapping.yaml` to use v0.7.0+ schema (shared/roles namespaces).
    - Consolidated `DRAFT_CONTEXT.md` from `management/` to `agent-commons/`.
    - Moved reflections to role-based subdirectories (`roles/pm/reflections/`).
    - Flattened Gemini CLI TOML commands in `.gemini/commands/`.
    - Standardized `failure_mode_log.md` with frontmatter.

**Status**: Verified with manual structural check. Ready for next session.
