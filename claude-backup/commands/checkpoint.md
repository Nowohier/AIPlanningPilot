# Mid-Session Checkpoint

Save current session progress to your handover file without ending the session.

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/checkpoint`. Do NOT trigger from conversational mentions of the word "checkpoint", "save progress", or similar phrasing.

## Steps

0. **Guard**: Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED`, refuse and direct to `/onboard`.
1. Summarize what has been accomplished so far this session (from conversation context)
2. List any uncommitted decisions or open threads
3. Write to the developer's handover file (`${PLANNING_REPO}\handovers\handover-{name}.md`):
   - Derive filename from Developer field (e.g., "Chris" → `handover-chris.md`)
   - Prepend a "Checkpoint {time}" section above existing notes
   - Include: completed items, in-progress items, open questions
4. Confirm: **"Checkpoint saved. Continuing session."**

This does NOT update STATE.md — that happens during /ciao.
