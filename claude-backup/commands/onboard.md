# Developer Onboarding & Environment Setup

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/onboard`. Do NOT trigger from conversational mentions of "onboard", "onboarding", "set up environment", "get started", or similar phrasing.

Interactive setup wizard for a developer's first session. Configures all
per-developer files so that /moin, /ciao, and other commands work without
manual path or identity setup.

## Step 1 — Gather developer info

Ask these questions (in a single message):

1. **"What's your developer name?"** (e.g., "Nowo", "Alex", or any name)
2. **"Where is your project's source code repo?"** (e.g., `M:\CODE_COPY\MyProject`)
3. **"Where is your planning repo?"** (e.g., `M:\CODE_COPY\MyProject-planning`)

## Step 2 — Validate paths

Verify both directories exist. If either is missing, report the error and
ask the developer to correct it before continuing.

## Step 3 — Configure `.claude/CLAUDE.md`

Read the current `.claude/CLAUDE.md` in the project repo. Update it:

- Set the `Developer` field to the name provided (add it if missing)
- Set the path variables table to the provided paths
- Preserve all other content (personal preferences, coding guidelines, etc.)

Show the proposed changes and ask for confirmation before writing.

## Step 4 — Deploy command files from template

The template files in `${PLANNING_REPO}/claude-backup/` are the
single source of truth. Copy them to `.claude/` and replace variable references
with the developer's literal paths.

1. Copy every file from `claude-backup/commands/` to `.claude/commands/`.
2. **Sync** hook registrations from `claude-backup/settings.json` into `.claude/settings.json`:
   - Read both files (create `.claude/settings.json` if it doesn't exist).
   - For each hook point (`PreToolUse`, `PostToolUse`, `UserPromptSubmit`):

     **Identify planning hooks** by command path: entries whose script filename
     matches any file in `claude-backup/hooks/`.

     **Sync**: Remove stale planning hooks not in template. Add missing ones.
     Preserve all non-planning hooks.

   - Preserve all non-hook settings unchanged.

3. In each `.claude/commands/*.md` file, replace every occurrence of:
   - `${PROJECT_REPO}` → the project path from Step 1
   - `${PLANNING_REPO}` → the planning path from Step 1

4. Update the header note in each command to:
   > **Paths**: This command uses literal paths for {developer name}'s machine.
   > If paths differ, update them here or run `/onboard`. Canonical docs: `main/CONFIG.md`.

Show a summary of files deployed and ask for confirmation before writing.

## Step 4b — Deploy and configure hooks

Copy the hook scripts from the template and configure `env.sh` with the
developer's paths.

1. Copy every file from `claude-backup/hooks/` to `.claude/hooks/`.

2. **IMPORTANT — Bash path conversion**: The hooks run in Git Bash, which uses
   Unix-style paths. Convert the developer's Windows paths to bash-compatible format:
   - `M:\CODE_COPY\...` → `/mnt/m/CODE_COPY/...`
   - General rule: `{DRIVE}:\...` → `/mnt/{drive_lowercase}/...`
   Verify the converted path works: `bash -c "test -d '/mnt/m/...' && echo ok"`

3. Replace the variable placeholders with the bash-compatible literal values:
   - `PROJECT_REPO="${PROJECT_REPO}"` → `PROJECT_REPO="/mnt/m/..."`
   - `PLANNING_REPO="${PLANNING_REPO}"` → `PLANNING_REPO="/mnt/m/..."`
   - `DEVELOPER="UNCONFIGURED"` → `DEVELOPER="{developer name}"`

4. Keep the derived paths using bash variable expansion (e.g.,
   `PLANNING_DIR="${PLANNING_REPO}"`) — they resolve automatically
   from the literal values set above.

5. Verify `.claude/settings.json` contains the planning hook registrations
   (this was handled in Step 4 item 2 — just confirm it's correct).

6. Ensure all `.sh` files have Unix line endings (LF, not CRLF).

## Step 5 — Create handover file

Derive the filename from the developer name: `handover-{lowercase name}.md`
(e.g., "Nowo" → `handover-nowo.md`, "Alex" → `handover-alex.md`).

Check if the file already exists in the planning repo's `handovers/` directory.
If not, create it:

```markdown
# Handover Notes — {Name}

> Last updated: —

## For Next Session
_No handover notes yet — this is your first session._

## Open Threads
_None._
```

## Step 6 — Project briefing

Now that the environment is configured, give the project briefing:

1. Check if `main/STATE.md` exists in the planning repo.
   - If it does NOT exist, say: **"Environment configured! Run `/initial-planning` to set up your project."**
     and stop here.
   - If it exists, continue:
2. Read `main/STATE.md`
3. Read `main/PLAN.md` (if it exists, read the first few sections for overview)
4. Read the most recent 2-3 decision records (if any)
5. Present:
   - Project summary (5 lines)
   - Current phase and what's been completed
   - Recommended reading for the current phase
6. Ask: **"You're all set up. What would you like to start with?"**
