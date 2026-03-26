# Mid-Session Checkpoint

Save current session progress to your handover file without ending the session.

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/checkpoint`. Do NOT trigger from conversational mentions of the word "checkpoint", "save progress", or similar phrasing.

> **Paths**: This template uses <PROJECT_REPO> and <PLANNING_REPO> variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

## Steps

0. **Guard**: Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED`, refuse and direct to `/onboard`.
1. Summarize what has been accomplished so far this session (from conversation context)
2. List any uncommitted decisions or open threads
3. Write to the developer's handover file (`${PLANNING_REPO}\handovers\handover-{name}.md`):
   - Derive filename from Developer field (e.g., "Chris" -> `handover-chris.md`)
   - Update the top four sections (For Next Session, Decisions & Findings, Open Threads, From Last Session) with current progress
   - Update the "Last updated" date to today
   - **Preserve the `## Session Log` section unchanged** (session log entries are only written by /ciao at end of day)
   - The checkpoint is a mid-session snapshot of the top sections only
4. Confirm: **"Checkpoint saved. Continuing session."**

This does NOT update STATE.md or the Session Log -- those happen during /ciao.
