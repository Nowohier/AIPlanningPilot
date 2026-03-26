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
4. **"Where is your AIPlanningPilot source repo clone?"** (optional, for `/subtree-push`. e.g., `M:\CODE_COPY\AIPlanningPilot`. Enter "skip" if you don't have one.)

## Step 2 — Validate paths

Verify both directories exist. If either is missing, report the error and
ask the developer to correct it before continuing.
If SOURCE_REPO was provided (not "skip"), verify it exists too.

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
   - `\${PROJECT_REPO}` → the project path from Step 1
   - `\${PLANNING_REPO}` → the planning path from Step 1

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
   - `M:\CODE_COPY\...` → `/m/CODE_COPY/...`
   - General rule: `{DRIVE}:\...` → `/{drive_lowercase}/...`
   Verify the converted path works: `bash -c "test -d '/m/...' && echo ok"`

3. Replace the variable placeholders with the bash-compatible literal values:
   - `PROJECT_REPO="\${PROJECT_REPO}"` → `PROJECT_REPO="/m/..."`
   - `PLANNING_REPO="\${PLANNING_REPO}"` → `PLANNING_REPO="/m/..."`
   - `SOURCE_REPO="\${SOURCE_REPO}"` → `SOURCE_REPO="/m/..."` (or `"UNCONFIGURED"` if skipped)
   - `DEVELOPER="UNCONFIGURED"` → `DEVELOPER="{developer name}"`

4. Keep the derived paths using bash variable expansion (e.g.,
   `PLANNING_DIR="\${PLANNING_REPO}"`) — they resolve automatically
   from the literal values set above.

5. Verify `${PROJECT_REPO}/.claude/settings.json` contains the planning hook registrations
   (this was handled in Step 4 item 2 — just confirm it's correct).

6. Ensure all `${PROJECT_REPO}/.claude/hooks/*.sh` files have Unix line endings (LF, not CRLF).

7. **Deploy hooks-config.sh** (project-specific guardrails):
   - Check if `${PLANNING_REPO}/main/hooks-config.sh` already exists.
   - If it does NOT exist, copy from `${PLANNING_REPO}/claude-backup/hooks-config.template.sh`
     to `${PLANNING_REPO}/main/hooks-config.sh` and ensure LF line endings.
   - Tell the developer:
     **"Created `main/hooks-config.sh` with empty defaults. Edit this file to configure
     guardrail hooks for your project (protected directories, git commit blocking).
     This file is git-tracked and shared by all developers."**
   - If it already exists, show the current values and say:
     **"Project guardrails are configured in `main/hooks-config.sh`."**

## Step 4c — Configure project-local permissions

Set up `${PROJECT_REPO}/.claude/settings.local.json` so that Claude Code can
access planning repo files without permission prompts when running from the
project repo.

1. Read `${PROJECT_REPO}/.claude/settings.local.json` (if it exists).
2. **Convert the planning repo path to MSYS format** for permission rules:
   - Run `cygpath -u "{planning repo Windows path}"` to get the MSYS path
     (e.g., `M:\CODE_COPY\MyProject-planning` → `/m/CODE_COPY/MyProject-planning`)
   - This is required because Claude Code's permission matcher on Windows (Git Bash)
     compares against MSYS-normalized paths, not native Windows paths.
3. **Merge** the following into the existing content (preserve any existing
   `permissions`, `allow` entries, or other settings the developer already has):
   - Add the planning repo path (Windows format, forward slashes) to the
     `additionalDirectories` array (create the array if it doesn't exist).
   - If already listed, skip — do not add duplicates.
   - Add Read/Edit/Write permission rules using the **MSYS path with `//` prefix**.
4. The result should look like (with existing settings preserved):
   ```json
   {
     "additionalDirectories": [
       "{planning repo path, Windows forward-slash format}"
     ],
     "permissions": {
       "allow": [
         "Bash(find:*)",
         "Read(//{msys planning repo path}/**)",
         "Edit(//{msys planning repo path}/**)",
         "Write(//{msys planning repo path}/**)"
       ]
     }
   }
   ```

   **Concrete example** for planning repo `M:\CODE_COPY\MyProject-planning`:
   ```json
   {
     "additionalDirectories": [
       "M:/CODE_COPY/MyProject-planning"
     ],
     "permissions": {
       "allow": [
         "Bash(find:*)",
         "Read(//m/CODE_COPY/MyProject-planning/**)",
         "Edit(//m/CODE_COPY/MyProject-planning/**)",
         "Write(//m/CODE_COPY/MyProject-planning/**)"
       ]
     }
   }
   ```

   **Permission path format rules**:
   - Use `cygpath -u` to convert Windows paths to MSYS format (`/m/...`)
   - Prepend `//` — this is the gitignore-style absolute-path prefix
   - Drive letters must be **lowercase** (`//m/` not `//M/`)
   - Always use forward slashes
   - **Wrong**: `Read(M:\\CODE_COPY\\...)` or `Read(//M:/CODE_COPY/...)`
   - **Right**: `Read(//m/CODE_COPY/...)`

   The `permissions.allow` entries above are defaults — preserve any additional
   entries the developer already has.
5. Show the proposed `settings.local.json` content and ask for confirmation
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
