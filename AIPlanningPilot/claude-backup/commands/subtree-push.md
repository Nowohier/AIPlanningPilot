# Subtree Push -- Push Framework Changes to Source Repo

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/subtree-push`. Do NOT trigger from conversational mentions of "push", "upstream", or similar phrasing.

> **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

> **Permissions**: Pushing requires write access to the AIPlanningPilot GitHub repository.
> If the push is denied, contact the repository owner to request collaborator access.

Push changes made to the AIPlanningPilot framework back to the source repository.
Use this only for shared framework changes (Dashboard code, hooks, command templates, scripts),
NOT for project-specific planning content.

## Step 0 -- Guard

1. Read the `Developer` field from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop immediately:
   **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**
2. Verify the working tree is clean:
   ```bash
   cd "${PROJECT_REPO}" && git status --porcelain
   ```
   If there are uncommitted changes, stop:
   **"Working tree has uncommitted changes. Commit or stash them before pushing."**

## Step 1 -- Safety warning

Show the user this warning and ask for confirmation:

**"Subtree push will extract commits touching the subtree prefix and push them
to the AIPlanningPilot source repository on GitHub. This affects all downstream
users of the framework.**

**Only push changes to shared framework code:**
- Dashboard app (AIPlanningPilot.Dashboard/)
- Dashboard tests (AIPlanningPilot.Dashboard.Tests/)
- Template commands (AIPlanningPilot/claude-backup/commands/)
- Template hooks (AIPlanningPilot/claude-backup/hooks/)
- Template skills (AIPlanningPilot/claude-backup/skills/)
- Scripts (AIPlanningPilot/scripts/)

**Do NOT push project-specific content:**
- Planning files (main/STATE.md, PLAN.md, MIGRATION.md)
- Handovers, decisions, analysis, archive
- Project-specific commands or skills

**Proceed with push?**"

If the user does not confirm, abort.

## Step 2 -- Detect subtree prefix

Find the `AIPlanningPilot.slnx` file relative to the git root to determine the subtree prefix:

```bash
cd "${PROJECT_REPO}"
GIT_ROOT=$(git rev-parse --show-toplevel)
SLNX=$(find "$GIT_ROOT" -maxdepth 3 -name "AIPlanningPilot.slnx" -not -path "*/.git/*" | head -1)
```

If `AIPlanningPilot.slnx` is not found, stop:
**"Could not find AIPlanningPilot.slnx. Is this repo set up with a subtree?"**

The subtree prefix is the directory containing `AIPlanningPilot.slnx`, relative to the git root.

## Step 3 -- Verify remote

Check if the `aiplanning` remote exists:

```bash
git -C "${PROJECT_REPO}" remote get-url aiplanning
```

If the remote does not exist, add it:

```bash
git -C "${PROJECT_REPO}" remote add aiplanning https://github.com/Nowohier/AIPlanningPilot.git
```

## Step 4 -- Push

```bash
cd "${PROJECT_REPO}"
git subtree push --prefix="<PREFIX>" aiplanning master
```

Replace `<PREFIX>` with the detected subtree prefix from Step 2.

## Step 5 -- Report

- If push succeeded: **"Framework changes pushed to AIPlanningPilot source repo."**
  Remind the user that other downstream repos can pick up changes via `/subtree-pull`.
- If push was rejected (non-fast-forward): Suggest running `/subtree-pull` first to merge upstream changes, then retry.
- If push was denied (permission error): **"Push denied. You need write access to the AIPlanningPilot GitHub repository. Contact the repository owner."**
