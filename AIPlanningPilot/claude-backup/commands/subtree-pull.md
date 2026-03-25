# Subtree Pull -- Sync Framework Updates from Source Repo

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/subtree-pull`. Do NOT trigger from conversational mentions of "pull", "sync", "update", or similar phrasing.

> **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

Pull the latest AIPlanningPilot framework changes (Dashboard code, template commands, hooks, scripts) from the source repository into this project's subtree.

## Step 0 -- Guard

1. Read the `Developer` field from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop immediately:
   **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**
2. Verify the working tree is clean:
   ```bash
   cd "${PROJECT_REPO}" && git status --porcelain
   ```
   If there are uncommitted changes, stop:
   **"Working tree has uncommitted changes. Commit or stash them before pulling."**

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

## Step 2 -- Verify remote

Check if the `aiplanning` remote exists:

```bash
git -C "${PROJECT_REPO}" remote get-url aiplanning
```

If the remote does not exist, add it:

```bash
git -C "${PROJECT_REPO}" remote add aiplanning https://github.com/Nowohier/AIPlanningPilot.git
```

## Step 3 -- Pull

Fetch and pull the latest changes:

```bash
cd "${PROJECT_REPO}"
git fetch aiplanning
git subtree pull --prefix="<PREFIX>" aiplanning master --squash
```

Replace `<PREFIX>` with the detected subtree prefix from Step 1.

## Step 4 -- Handle conflicts

If there are merge conflicts:
1. List the conflicting files
2. For project-specific files (STATE.md, handovers, decisions, plan files): keep the local version
3. For framework files (Dashboard code, scripts, hooks, commands): accept the upstream version
4. After resolving, complete the merge with `git commit`

## Step 5 -- Verify

Run the solution build to verify nothing is broken:

```bash
dotnet build "<PREFIX>/AIPlanningPilot.slnx"
```

## Step 6 -- Report

- If changes were pulled, show a summary of updated files
- If already up to date: **"Already up to date with the latest AIPlanningPilot framework."**
- Remind the user to run `/moin` to pick up any new commands or hooks via the template sync
