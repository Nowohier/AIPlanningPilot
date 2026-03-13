# Context Refresh (Mid-Session)

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/refresh`. Do NOT trigger from conversational mentions of "refresh", "reload context", "re-anchor", or similar phrasing.

Lightweight context reload for when the session has been running long
and you need to re-anchor on project state.

## Steps

0. **Guard**: Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED`, refuse and direct to `/onboard`.
1. Read `${PLANNING_REPO}\main\STATE.md`
2. Present a 5-line max summary:
   - Current phase and day
   - Top 3 next actions
   - Any open blockers
3. Say: **"Context refreshed. Continuing."**

Do NOT read PLAN.md, plan/ files, analysis docs, CLAUDE.md files, or decision records.
This is a quick re-anchor, not a full briefing.
