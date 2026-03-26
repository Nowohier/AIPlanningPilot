# Subtree Push -- Push Framework Changes to Source Repo

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/subtree-push`. Do NOT trigger from conversational mentions of "push", "upstream", or similar phrasing.

> **Paths**: This template uses ${PROJECT_REPO}, ${PLANNING_REPO}, and ${SOURCE_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

Copy framework files from the project's subtree to the local AIPlanningPilot source repo clone.
The developer reviews the diff and commits/pushes from the source repo manually.

Only framework code is copied -- project-specific content (planning files, decisions,
handovers, analysis, archive, documents) is never included.

## Step 0 -- Guard

1. Read the `Developer` field from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop immediately:
   **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**
2. Read `${SOURCE_REPO}` from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop:
   **"SOURCE_REPO is not configured. Run `/onboard` to set it up, or add it to `.claude/CLAUDE.md`."**
3. Verify `${SOURCE_REPO}` directory exists. If not, stop:
   **"SOURCE_REPO points to a non-existent directory: `${SOURCE_REPO}`. Clone the AIPlanningPilot repo there first."**

## Step 1 -- Detect subtree prefix

Find the `AIPlanningPilot.slnx` file relative to the git root to determine the subtree prefix:

```bash
cd "${PROJECT_REPO}"
GIT_ROOT=$(git rev-parse --show-toplevel)
SLNX=$(find "$GIT_ROOT" -maxdepth 3 -name "AIPlanningPilot.slnx" -not -path "*/.git/*" | head -1)
```

If `AIPlanningPilot.slnx` is not found, stop:
**"Could not find AIPlanningPilot.slnx. Is this repo set up with a subtree?"**

The subtree prefix is the directory containing `AIPlanningPilot.slnx`, relative to the git root.

## Step 2 -- Show what will be copied

Show the user the framework directories that will be synced:

**"The following framework directories will be copied from `<subtree prefix>/` to `${SOURCE_REPO}/`:**

**Included (framework code):**
- `AIPlanningPilot.Dashboard/` -- WPF Dashboard app
- `AIPlanningPilot.Dashboard.Tests/` -- Dashboard tests
- `AIPlanningPilot/claude-backup/` -- Template commands, hooks, skills
- `AIPlanningPilot/scripts/` -- Sync and utility scripts
- `AIPlanningPilot/tests/` -- Hook tests
- Root files: `CLAUDE.md`, `.gitattributes`, `.gitignore`, `AIPlanningPilot.slnx`, `LICENSE`, `NuGet.Config`, `README.md`
- `docs/` -- Framework documentation/screenshots

**Excluded (project-specific, never copied):**
- `AIPlanningPilot/main/` -- STATE.md, PLAN.md, hooks-config.sh, etc.
- `AIPlanningPilot/decisions/` -- Decision records
- `AIPlanningPilot/handovers/` -- Handover files
- `AIPlanningPilot/plan/` -- Phase plans
- `AIPlanningPilot/analysis/` -- Analysis documents
- `AIPlanningPilot/archive/` -- Completed actions, state snapshots
- `AIPlanningPilot/documents/` -- Project documents
- `.claude/` -- Bootstrap command (project-specific)

**Proceed?**"

If the user does not confirm, abort.

## Step 3 -- Copy framework files

Use `rsync` (available in Git Bash) to sync each framework directory. The `--delete` flag
ensures files removed from the subtree are also removed from the source repo.

```bash
SUBTREE="${PROJECT_REPO}/<PREFIX>"
TARGET="${SOURCE_REPO}"

# Directories
rsync -av --delete "${SUBTREE}/AIPlanningPilot.Dashboard/" "${TARGET}/AIPlanningPilot.Dashboard/"
rsync -av --delete "${SUBTREE}/AIPlanningPilot.Dashboard.Tests/" "${TARGET}/AIPlanningPilot.Dashboard.Tests/"
rsync -av --delete "${SUBTREE}/AIPlanningPilot/claude-backup/" "${TARGET}/AIPlanningPilot/claude-backup/"
rsync -av --delete "${SUBTREE}/AIPlanningPilot/scripts/" "${TARGET}/AIPlanningPilot/scripts/"
rsync -av --delete "${SUBTREE}/AIPlanningPilot/tests/" "${TARGET}/AIPlanningPilot/tests/"
rsync -av --delete "${SUBTREE}/docs/" "${TARGET}/docs/"

# Root files (copy individually)
for f in CLAUDE.md .gitattributes .gitignore AIPlanningPilot.slnx LICENSE NuGet.Config README.md; do
  if [ -f "${SUBTREE}/${f}" ]; then
    cp "${SUBTREE}/${f}" "${TARGET}/${f}"
  fi
done
```

Use the bash-compatible paths (forward slashes) for the subtree and source repo.
Convert Windows paths from the command file if needed: `M:\...\` -> `/m/.../`.

## Step 4 -- Show diff in source repo

Run git status and diff in the source repo to show the developer what changed:

```bash
cd "${SOURCE_REPO}"
git status
git diff --stat
```

Present the output to the developer.

## Step 5 -- Report

**"Framework files synced to `${SOURCE_REPO}`.

To publish the changes:
1. Review the diff above
2. Open a terminal in `${SOURCE_REPO}`
3. Commit and push:
   ```
   git add -A
   git commit -m "your message"
   git push origin master
   ```

Other projects can then pick up the changes via `/subtree-pull`."**

Do NOT commit or push -- the developer handles that manually.
