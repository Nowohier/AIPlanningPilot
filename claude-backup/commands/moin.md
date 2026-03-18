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

## Step 0b — Sync template to .claude/

The template in `${PLANNING_REPO}/claude-backup/` is the single
source of truth. Sync it to `.claude/` so any template updates (new commands,
hook fixes) are picked up at the start of each session.

1. Copy every file from `claude-backup/commands/` to `.claude/commands/`.
2. Copy every file from `claude-backup/hooks/` to `.claude/hooks/`.
3. **Sync** hook registrations from `claude-backup/settings.json` into `.claude/settings.json`:
   - Read both files. For each hook point (`PreToolUse`, `PostToolUse`, `UserPromptSubmit`):

     **Identify planning hooks**: A hook entry is a "planning hook" if its
     `command` string references a script filename that exists in `claude-backup/hooks/`
     (e.g., `validate-state.sh`, `block-literal-paths-in-templates.sh`, etc.).

     **Sync**:
     - **Remove** any planning hook in `.claude/settings.json` that is NOT in `claude-backup/settings.json` (stale entry from a previous template version).
     - **Add** any planning hook from `claude-backup/settings.json` that is missing in `.claude/settings.json`.
     - **Preserve** all non-planning hooks untouched.

   - Preserve all non-hook settings unchanged.
   - **If hook registrations changed** (entries added or removed), warn:
     **"Hook registrations changed in settings.json. Please restart the session for them to take effect."**
     Updated `.sh` script content takes effect immediately — only registration changes need a restart.
4. Do **NOT** overwrite `.claude/CLAUDE.md` or `.claude/settings.local.json`.

5. In each `.claude/commands/*.md` file, replace:
   - `${PROJECT_REPO}` → the literal project path from CLAUDE.md
   - `${PLANNING_REPO}` → the literal planning path from CLAUDE.md
   Replace the template header note with **exactly these two lines** (both are required):
   > **Paths**: This command uses literal paths for {developer name}'s machine.
   > If paths differ, update them here or run `/onboard`. Canonical docs: `main/CONFIG.md`.

   **IMPORTANT — do NOT delegate this step to a sub-agent.** Perform all Edit
   tool calls directly. Sub-agents have truncated replacement text in the past,
   silently dropping the second header line from all command files.

   **IMPORTANT — use the Edit tool** for these replacements, NOT `sed` or `perl`
   via Bash. Windows paths contain backslashes which sed and perl interpret as
   escape characters in replacement strings, silently destroying paths
   (backslash + letter collapses to just the letter). The Edit tool handles
   literal strings correctly. After replacement, verify at least one file to
   confirm paths are intact.

6. In `.claude/hooks/env.sh`, replace variable placeholders with bash-format
   paths (see `/onboard` Step 4b for the Windows → bash conversion rules) and
   set `DEVELOPER` to the developer name from CLAUDE.md.

7. Ensure all `.sh` files have Unix line endings (LF, not CRLF).

Perform this step silently — only report errors, not success.

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
