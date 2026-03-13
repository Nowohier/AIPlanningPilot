# AI-Assisted Project Management Framework

A file-based project management framework for Claude Code sessions.
Manages context, state, and decisions across sessions so each new conversation
picks up exactly where the last one left off.

---

## Getting Started

### Prerequisites

- [Claude Code](https://docs.anthropic.com/en/docs/claude-code) CLI installed
- Node.js on PATH (required by hook scripts)
- Git Bash (on Windows)

### Step 1 — Set up the two repos

You need two repositories: one for your project's source code, one for planning.

```
MyProject/              ← your source code (existing or new)
MyProject-planning/     ← copy of this template
```

Copy (or clone) this template as your planning repo. Keep the directory structure as-is.

### Step 2 — Onboard (`/onboard`)

Open Claude Code **in your project repo** (e.g., `MyProject/`), then type:

```
/onboard
```

The wizard asks three questions:
1. Your developer name
2. Path to your project repo
3. Path to your planning repo

It then configures everything automatically:
- Sets up `.claude/CLAUDE.md` with your identity and paths
- Deploys all commands and hooks from the planning template to `.claude/`
- Replaces `${PROJECT_REPO}` / `${PLANNING_REPO}` variables with your literal paths
- Creates your personal handover file

### Step 3 — Bootstrap the project (`/initial-planning`)

Still in Claude Code, run:

```
/initial-planning
```

This interactive wizard helps you define your project:
1. **Project info** — name, description, goals, codebase overview
2. **Phases** — choose between guided phase design, manual entry, or a minimal single phase
3. **File creation** — generates `STATE.md`, `PLAN.md`, `plan/overview.md`, one `plan/phase-*.md` per phase, and `decisions/INDEX.md`

After this, your planning repo is fully initialized and ready to use.

### Step 4 — Start working (`/moin`)

From now on, every session starts with:

```
/moin
```

This loads your project state, syncs the latest template updates, presents a briefing with your handover notes, and asks what you'd like to work on.

### Daily workflow

```
/moin                   Start of session — loads state, presents briefing
  ... work ...
  /decision             Record a decision (anytime)
  /checkpoint           Save mid-session progress (optional)
  /refresh              Re-anchor context if session runs long (optional)
  ... work ...
/ciao                   End of session — updates state, writes handover
```

### After a multi-day gap

```
/recap                  Broader catch-up — shows what changed since you left
```

### If something feels off

```
/healthcheck            Runs 12 environment checks, reports PASS/WARN/FAIL
```

### The lifecycle at a glance

```
/onboard → /initial-planning → /moin → work → /ciao
                                  ↑                |
                                  └────────────────┘
                                     (next session)
```

One-time setup is `/onboard` + `/initial-planning`. The daily cycle is `/moin` → work → `/ciao`.

---

## Reference

### How It Works

The framework manages context across Claude Code sessions using a file-based
state machine. Each session starts by reading state, works on tasks, and ends
by writing state back.

:::mermaid
flowchart TD
    subgraph First Time
        ON["/onboard"]
        ON -->|asks| Q1["Developer name?<br/>Repo paths?"]
        Q1 -->|configures| CF[".claude/CLAUDE.md<br/>command files<br/>handover file"]
        CF --> IP["/initial-planning"]
        IP -->|creates| PF["STATE.md<br/>PLAN.md<br/>plan files"]
        PF --> READY["Project ready"]
    end

    subgraph Daily Session
        START(["Session Start"])
        START --> MOIN["/moin"]
        MOIN -->|reads| STATE["STATE.md"]
        MOIN -->|reads| HO["handover-{name}.md"]
        MOIN -->|reads selective| DOCS["plan/ + analysis/"]
        MOIN --> BRIEF["Briefing presented"]
        BRIEF --> WORK["Development Work"]

        WORK -->|context drifts?| REFRESH["/refresh"]
        REFRESH -->|re-reads| STATE
        REFRESH --> WORK

        WORK -->|decision made?| DECISION["/decision"]
        DECISION -->|creates| ADR["decisions/{NNN}.md"]
        DECISION -->|updates| STATE
        DECISION --> WORK

        WORK -->|long session?| CHECK["/checkpoint"]
        CHECK -->|writes| HO
        CHECK --> WORK

        WORK --> CIAO["/ciao"]
        CIAO -->|proposes changes| REVIEW{"Developer<br/>confirms?"}
        REVIEW -->|yes| WRITE["Updates STATE.md<br/>+ archive<br/>+ handover file"]
        REVIEW -->|correct| CIAO
        WRITE --> END(["Session End"])
    end

    subgraph After Gap
        RECAP["/recap"]
        RECAP -->|reads| STATE
        RECAP -->|reads| ARCH["archive/<br/>completed-actions.md"]
        RECAP -->|reads| HO
        RECAP --> CATCHUP["Catch-up briefing"]
    end

    READY -.->|first session| MOIN
    END -.->|next day| MOIN
    END -.->|days later| RECAP
    CATCHUP -.-> WORK
:::

### Key Concepts

**STATE.md** is the single source of truth for current project state.
Every session reads it first. It stays lean — historical data lives in `archive/`.

**Handover files** are per-developer. Each developer owns their file exclusively —
no merge conflicts, no coordination needed.

**Selective context loading**: `/moin` reads `plan/overview.md` and selectively
loads phase plans, analysis, and decision files based on today's actions.
File discovery uses glob patterns — there is no static file inventory.

### Two-Repo Model

This framework uses two repositories:

- **Project repo** (`${PROJECT_REPO}`): Your source code. The `.claude/` directory here contains the deployed (personalized) commands and hooks.
- **Planning repo** (`${PLANNING_REPO}`): This repo. Contains the state, plans, decisions, and the template (`claude-backup/`) that is the single source of truth.

The `.claude/` directory in the project repo is typically git-ignored (contains per-developer config).
The `claude-backup/` directory here is the single source of truth — a developer-agnostic template.
`/onboard` and `/moin` deploy it to `.claude/`, replacing variables with literal paths.

See [CONFIG.md](main/CONFIG.md) for path variable definitions and setup instructions.

### Commands

| Command | When | What it does |
|---------|------|-------------|
| `/onboard` | First session ever | Setup wizard — configures paths, identity, creates handover file |
| `/initial-planning` | After onboarding | Interactive wizard — creates project plan, phases, STATE.md |
| `/moin` | Start of each day | Loads state, presents briefing, shows handover notes |
| `/refresh` | Mid-session | Lightweight context re-anchor (5 lines) |
| `/decision` | When a decision is made | Records ADR immediately, updates STATE.md |
| `/checkpoint` | Long sessions | Saves progress to handover file without ending session |
| `/ciao` | End of day | Proposes state updates, writes after confirmation |
| `/recap` | Returning after days off | Broader catch-up with completed actions and new decisions |
| `/healthcheck` | Something feels off | Runs 12 environment checks, reports PASS/WARN/FAIL with fixes |

### Directory Structure

```
planning-repo/
├── README.md                  ← You are here
│
├── main/                      Core orchestration documents
│   ├── PLAN.md                Table of contents — links to plan/ files
│   ├── STATE.md               Current state — phase, next actions, open decisions
│   └── CONFIG.md              Path variable definitions (per-developer)
│
├── handovers/                 Per-developer handover files
│   └── handover-{name}.md    Handover notes — one per developer
│
├── plan/                      Split plan files (context window optimized)
│   ├── overview.md            Vision, macro phases, risk register
│   └── phase-{N}-{slug}.md   One file per phase
│
├── decisions/                 Architecture Decision Records (ADRs)
│   ├── INDEX.md               Decisions index
│   └── {NNN}-{slug}.md       Individual decision records
│
├── analysis/                  Codebase analysis documents
│
├── archive/                   Historical data (not loaded during /moin)
│   ├── completed-actions.md   Actions completed via /ciao
│   └── state-snapshots/       Timestamped STATE.md backups
│
├── documents/                 Project-specific documents
│
├── claude-backup/             Git-tracked mirror of project/.claude/
│   ├── CLAUDE.md              Developer preferences + path variables
│   ├── settings.json          Hook registrations
│   ├── commands/              Claude Code slash commands (9 commands)
│   └── hooks/                 Claude Code hooks (validation & safety, 7 scripts)
│
└── tests/                     Automated tests
    └── hooks/                 Hook validation tests (bash)
        ├── run-tests.sh       Entry point
        ├── test-helper.sh     Shared test infrastructure
        ├── test-*.sh          Individual test files
        └── fixtures/          Test fixture files
```

### Testing

The hooks have automated bash tests in `tests/hooks/`. Run them from any bash shell:

```bash
cd tests/hooks
bash run-tests.sh                    # Run all tests
bash run-tests.sh validate-state     # Run only matching test files
```

Tests use temp directories with isolated `env.sh` — they don't touch real repo files.
Fixture files in `fixtures/` use `%%TODAY%%` placeholders replaced at runtime.
