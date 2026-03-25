# Start of Day

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/moin`. Do NOT trigger from conversational mentions of "moin", "good morning", "start of day", "start session", or similar phrasing.

> **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

## Context Budget

Total context loaded by /moin should not exceed ~20KB.
Load plan overview + current phase only. Deep references (full analysis, other phases) on demand during work.

## Step 0 — Guard

1. Read the `Developer` field from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop immediately:
   **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**
2. Verify paths resolve to existing directories: `${PROJECT_REPO}` and `${PLANNING_REPO}`. If either is missing, stop and alert:
   **"Path points to a non-existent directory: `<path>`. Update `.claude/CLAUDE.md` and run `/onboard` to reconfigure."**
3. Check that `main/STATE.md` exists in the planning repo. If not, stop:
   **"No project initialized. Run `/initial-planning` first."**
4. If all checks pass, use the developer name for the rest of the command and proceed silently.

## Step 0b — Sync template to .claude/ (optional)

Ask the user:
**"Refresh .claude/ commands & hooks from templates? (y/n)"**

If the user declines, skip this entire step and proceed to Step 1.

If the user accepts, run the sync script:

```bash
node ${PLANNING_REPO}/scripts/sync-claude.mjs
```

The script reads `.claude/CLAUDE.md` for developer config and path variables, then:
- Copies commands from `claude-backup/commands/` with path substitution and header replacement
- Copies hooks from `claude-backup/hooks/` with env.sh variable resolution and LF line endings
- Copies skills from `claude-backup/skills/` (preserves local-only skills)
- Merges hook registrations from `claude-backup/settings.json` (preserves non-planning hooks)
- Never touches `.claude/CLAUDE.md` or `.claude/settings.local.json`

If the script reports hook registration changes, relay the restart warning to the user.
Otherwise, proceed silently to Step 1.

## Step 1 — Load core context

Read these files in parallel:

1. `${PLANNING_REPO}\main\STATE.md` — morning briefing (phase, next actions, decisions)
2. `${PLANNING_REPO}\plan\overview.md` — vision, macro phases, risk register

Note: `.claude/CLAUDE.md` and root `CLAUDE.md` are auto-loaded by Claude Code — do NOT re-read them.

## Step 2 — Load selective context

Based on today's **Next Actions** from STATE.md, load additional files. Rules:

- **Always load**: STATE.md + plan/overview.md (done in Step 1)
- **Coding day** → read `${PROJECT_REPO}\CLAUDE.md` (project structure, build commands)
- **Current phase detail** → read the matching `plan/phase-{N}-*.md` file based on the phase in STATE.md
- **Analysis needed** → read files in `analysis/`
- **Specific decision needed** → read the individual `decisions/{NNN}-*.md` file
- **Multi-developer coordination** → read `plan/parallel-work.md` if it exists
- **Deep analysis needed** → load analysis files only when truly needed (they can be large)

The goal is to load the **minimum context needed** for today's tasks, not everything that exists.

## Step 3 — Load handover notes

Derive the handover filename from the Developer field: `handover-{lowercase name}.md`
(e.g., Developer "Nowo" → `handover-nowo.md`).

Read `${PLANNING_REPO}\handovers\handover-{name}.md`.

If the file does not exist, say:
**"No handover file found. Run `/onboard` to set up your environment first."** and stop.

## Step 4 — Present briefing

Present a compact summary (max 20 lines):

1. **Where we are**: Current phase, day number, what was completed last
2. **Your handover notes**: Show the handover notes from the developer's handover file
3. **Open questions**: Any items marked as needing discussion or decision
4. **Next actions**: The top 3-5 concrete actions from STATE.md
5. **Blockers**: Anything blocked or needing attention

End with: **"What would you like to work on?"**

## Important

- Do NOT load `archive/completed-actions.md` during start-of-day — that's historical data.
- Do NOT load the full `PLAN.md` — it's just a TOC. Load `plan/overview.md` + current phase file instead.
- Do NOT load large analysis files unless today's work specifically requires detailed data.
- Keep context lean. When in doubt, don't load it — it can always be loaded on demand later.
