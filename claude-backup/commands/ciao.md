# End of Day

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/ciao`. Do NOT trigger from conversational mentions of "ciao", "goodbye", "end of day", "wrap up", or similar phrasing.

> **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

## Step 0 — Guard

Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED` or missing, refuse and direct to `/onboard`.

## Step 1 — Read current state

Read `${PLANNING_REPO}\main\STATE.md`

## Step 2 — Auto-extract session summary

Before asking the developer, scan the current conversation and extract:

1. **Files touched**: Files created, modified, or deleted during this session (from tool calls)
2. **Decisions discussed**: Architectural or technical decisions, even if not formally recorded via /decision
3. **Patterns & insights**: Technical patterns discovered, confirmed, or rejected
4. **Unresolved questions**: Questions raised but not answered; topics deferred
5. **Technical findings**: Bugs, performance insights, dependency issues, build/test results

Present this under **"Session summary (auto-extracted)"**, then ask:

**"Here's what I captured from our session. Anything to add, change, or remove before I write the handover?"**

## Step 3 — Propose updates

Present the proposed changes as a summary:
- Actions to mark as completed (moved to `archive/completed-actions.md`)
- New actions to add
- Phase progress changes
- New decisions to record
- Handover notes (incorporating auto-extracted summary, refined by developer feedback)

Ask: **"Does this look correct? Any changes before I write it?"**

## Step 3b — Backup STATE.md

Before writing any changes, create a timestamped backup:

1. Copy `${PLANNING_REPO}\main\STATE.md` to
   `${PLANNING_REPO}\archive\state-snapshots\STATE-{YYYY-MM-DD-HHmmss}.md`
   - Create the `archive/state-snapshots/` directory if it does not exist
2. Clean up: if more than 10 files in `state-snapshots/`, delete the oldest to keep only the 10 most recent.

This step is silent — only report if the backup fails.

## Step 4 — Write updates

After the developer confirms, apply changes:

1. **STATE.md** (`${PLANNING_REPO}\main\STATE.md`):
   - Move completed actions from "Next Actions" (they go to the archive file, not STATE.md)
   - Add new next actions discovered during the day
   - Update "Phase Progress" if a phase status changed
   - Add any new decisions to "Decisions Made" (with links to decision files if created)
   - Move resolved items out of "Open Decisions"
   - Update the day counter and last-updated date

2. **Archive** (`${PLANNING_REPO}\archive\completed-actions.md`):
   - Append completed actions with completion date

3. **Handover file** (`${PLANNING_REPO}\handovers\handover-{name}.md`):
   - Derive filename from Developer field (e.g., "Nowo" -> `handover-nowo.md`)
   - Replace content with a structured handover:
     - **For Next Session**: Concrete action items — what to continue, what to start
     - **Decisions & Findings**: Key decisions (formal or informal), technical discoveries
     - **Open Threads**: Unresolved questions, deferred topics, things to revisit
     - **From Last Session**: 2-3 sentences giving future-Claude enough context to resume
   - Set "Last updated" to today's date

## Step 5 — Present summary

Present a short end-of-day summary (max 10 lines):
- What got done today
- What's queued for tomorrow (from the handover notes just written)
- Any open blockers

Do NOT commit anything — the user handles git manually.
