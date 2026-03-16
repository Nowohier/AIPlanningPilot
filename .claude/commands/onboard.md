<!-- BOOTSTRAP COPY: This file exists so /onboard is available when
     Claude Code is first opened in the planning repo.
     Canonical version: claude-backup/commands/onboard.md — keep in sync.
     /moin does NOT touch this file (it syncs to the project repo only). -->

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

Read `${PROJECT_REPO}/.claude/CLAUDE.md` (create the directory and file from
`${PLANNING_REPO}/claude-backup/CLAUDE.md` if they don't exist). Update it:

- Set the `Developer` field to the name provided (add it if missing)
- Set the path variables table to the provided paths
- Preserve all other content (personal preferences, coding guidelines, etc.)

Show the proposed changes and ask for confirmation before writing.

## Step 4 — Deploy command files from template

The template files in `${PLANNING_REPO}/claude-backup/` are the
single source of truth. Copy them to `${PROJECT_REPO}/.claude/` and replace variable references
with the developer's literal paths.

1. Copy every file from `${PLANNING_REPO}/claude-backup/commands/` to `${PROJECT_REPO}/.claude/commands/`.
2. **Sync** hook registrations from `${PLANNING_REPO}/claude-backup/settings.json` into `${PROJECT_REPO}/.claude/settings.json`:
   - Read both files (create `${PROJECT_REPO}/.claude/settings.json` if it doesn't exist).
   - For each hook point (`PreToolUse`, `PostToolUse`, `UserPromptSubmit`):

     **Identify planning hooks** by command path: entries whose script filename
     matches any file in `claude-backup/hooks/`.

     **Sync**: Remove stale planning hooks not in template. Add missing ones.
     Preserve all non-planning hooks.

   - Preserve all non-hook settings unchanged.

3. In each `${PROJECT_REPO}/.claude/commands/*.md` file, replace every occurrence of:
   - `${PROJECT_REPO}` → the project path from Step 1
   - `${PLANNING_REPO}` → the planning path from Step 1

4. Update the header note in each command to:
   > **Paths**: This command uses literal paths for {developer name}'s machine.
   > If paths differ, update them here or run `/onboard`. Canonical docs: `main/CONFIG.md`.

Show a summary of files deployed and ask for confirmation before writing.

## Step 4b — Deploy and configure hooks

Copy the hook scripts from the template and configure `env.sh` with the
developer's paths.

1. Copy every file from `${PLANNING_REPO}/claude-backup/hooks/` to `${PROJECT_REPO}/.claude/hooks/`.

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

5. Verify `${PROJECT_REPO}/.claude/settings.json` contains the planning hook registrations
   (this was handled in Step 4 item 2 — just confirm it's correct).

6. Ensure all `${PROJECT_REPO}/.claude/hooks/*.sh` files have Unix line endings (LF, not CRLF).

## Step 4c — Configure project-local permissions

Set up `${PROJECT_REPO}/.claude/settings.local.json` so that Claude Code can
access planning repo files without permission prompts when running from the
project repo.

1. Read `${PROJECT_REPO}/.claude/settings.local.json` (if it exists).
2. **Merge** the following into the existing content (preserve any existing
   `permissions`, `allow` entries, or other settings the developer already has):
   - Add `${PLANNING_REPO}` to the `additionalDirectories` array (create the
     array if it doesn't exist).
   - If `${PLANNING_REPO}` is already listed, skip — do not add duplicates.
3. The result should look like (with existing settings preserved):
   ```json
   {
     "additionalDirectories": [
       "{literal planning repo path}"
     ],
     "permissions": {
       "allow": [
         "Bash(find:*)"
       ]
     }
   }
   ```
   The `permissions.allow` entries above are defaults — preserve any additional
   entries the developer already has.
4. Show the proposed `settings.local.json` content and ask for confirmation
   before writing.

> **Why**: Commands like `/moin`, `/ciao`, `/decision` run from the project repo
> but read and write files in the planning repo. Without `additionalDirectories`,
> Claude prompts for permission on every cross-repo file access.

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
   - If it does NOT exist, say: **"Environment configured! Close this session, open Claude Code in your project repo (`${PROJECT_REPO}`), and run `/initial-planning` to set up your project."**
     and stop here.
   - If it exists, continue:
2. Read `main/STATE.md`
3. Read `main/PLAN.md` (if it exists, read the first few sections for overview)
4. Read the most recent 2-3 decision records (if any)
5. Present:
   - Project summary (5 lines)
   - Current phase and what's been completed
   - Recommended reading for the current phase
6. Ask: **"You're all set up. Close this session, open Claude Code in your project repo (`${PROJECT_REPO}`), and run `/moin` to start working."**
