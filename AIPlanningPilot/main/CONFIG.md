# Configuration — Developer-Specific Settings

> **Purpose**: Central place for path variables that differ per developer machine.
> Each developer clones this planning repo and adjusts the values below to match their local setup.
>
> **Rule**: All other documents in this repo reference paths using the variable names defined here.
> Never hardcode absolute machine paths in any other file.

---

## Path Variables

| Variable | Default Value | Description |
|----------|---------------|-------------|
| `${PROJECT_REPO}` | `UNCONFIGURED` | Root of the project's source code repository |
| `${PLANNING_REPO}` | `UNCONFIGURED` | Root of this planning repo (where this file lives) |

### How to read path references in other documents

Throughout PLAN.md, STATE.md, commands, and decision records you will see two kinds of paths:

1. **Paths relative to the project repo** — prefixed with `${PROJECT_REPO}/` or shown as repo-relative.
   These refer to files inside the project's source code.

2. **Paths relative to this planning repo** — shown as repo-relative without prefix (e.g., `main/PLAN.md`, `plan/overview.md`).
   These refer to files inside this planning repository.

### Setup for a new developer

Run `/onboard` in Claude Code — it handles all path configuration automatically.

**What `/onboard` configures:**
1. `${PROJECT_REPO}/.claude/CLAUDE.md` — sets Developer name and path variable values
2. `.claude/commands/*.md` — replaces variable references with your literal paths
3. `.claude/hooks/env.sh` — sets resolved paths for hook scripts
4. Creates your handover file in `handovers/`

**Manual fallback** (if not using `/onboard`):
1. Clone this repo to your preferred location.
2. Update `${PROJECT_REPO}/.claude/CLAUDE.md` — set the path variables to your local paths.
3. Update command files in `.claude/commands/` — replace paths to match your machine.

> Note: The "Default Value" column in the table above shows `UNCONFIGURED` until `/onboard` is run.
> Do not edit CONFIG.md for per-developer setup — use `.claude/CLAUDE.md` instead.

### How the backup template works

`claude-backup/` is the **single source of truth** — a developer-agnostic template. It uses
`${PROJECT_REPO}` and `${PLANNING_REPO}` variable references instead of literal paths,
and has `Developer: UNCONFIGURED`.

- `/onboard` and `/moin` copy the template to `.claude/` and resolve variables to literal paths
- Changes to commands or hooks are made directly in `claude-backup/` (the template)
- Every `/moin` picks up the latest template, so developers always get updates automatically

### Path variables in the project repo

Claude Code reads `${PROJECT_REPO}/.claude/CLAUDE.md` at session start — **before** any command runs. The command files (e.g., `/moin`, `/ciao`) use `${PROJECT_REPO}` and `${PLANNING_REPO}` variables, so Claude needs to know the resolved values.

The `## Path Variables` section in `.claude/CLAUDE.md` provides this. It is the **runtime source** of the variables when Claude Code executes commands. This file (`CONFIG.md`) remains the **canonical documentation** of the convention.

**Both places must stay in sync** — update both when changing paths.

### Example

If your project checkout is at `D:\Projects\myapp`, update the table above:

| Variable | Your Value |
|----------|------------|
| `${PROJECT_REPO}` | `D:\Projects\myapp` |
| `${PLANNING_REPO}` | `D:\Projects\myapp-planning` |

---

## Relationship Between the Two Repos

```
${PROJECT_REPO}/                      The product — your source code
├── src/                              Source code (language/framework specific)
├── .claude/                          Claude Code config (typically git-ignored)
│   ├── CLAUDE.md                     Personal preferences + path variables
│   ├── commands/                     Claude slash commands (deployed from template)
│   ├── hooks/                        Claude hooks (deployed from template)
│   └── settings.json                 Hook configuration
└── CLAUDE.md                         Root project context for Claude

${PLANNING_REPO}/                     Planning & documentation — this repo
├── main/                             Core orchestration documents
│   ├── CONFIG.md                     THIS FILE — path variables
│   ├── PLAN.md                       Master plan (table of contents)
│   └── STATE.md                      Current state — read first every session
├── handovers/                        Per-developer handover files
│   └── handover-{name}.md
├── plan/                             Split plan files (context-optimized)
│   └── overview.md, phase-*.md
├── decisions/                        Architectural decision records
├── analysis/                         Analysis documents
├── archive/                          Historical data
└── claude-backup/                    Source of truth — template for ${PROJECT_REPO}/.claude/
    ├── CLAUDE.md
    ├── settings.json
    ├── commands/
    └── hooks/
```
