# Catch-Up Recap

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/recap`. Do NOT trigger from conversational mentions of "recap", "catch up", "catch me up", "what did I miss", or similar phrasing.

> **Paths**: This template uses <PROJECT_REPO> and <PLANNING_REPO> variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

For returning after multiple days away. Provides a broader summary than /moin.

## Steps

0. **Guard**: Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED`, refuse and direct to `/onboard`.
1. Read `${PLANNING_REPO}\main\STATE.md` (current state, all decisions)
2. Read the developer's handover file (`${PLANNING_REPO}\handovers\handover-{name}.md`)
3. Read `${PLANNING_REPO}\archive\completed-actions.md` — find actions completed since
   this developer's last handover date
4. Present:
   - What phase we're in now vs when you left
   - Actions completed since your last session (grouped by phase)
   - New decisions made since your last session
   - Current next actions and who owns them
   - Your handover notes (if any remain relevant)
5. Ask: **"What would you like to catch up on first?"**
