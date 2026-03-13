# Initial Project Planning

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/initial-planning`. Do NOT trigger from conversational mentions of "initial planning", "set up project", "bootstrap", or similar phrasing.

Interactive wizard that bootstraps a new project's planning structure.
Creates STATE.md, PLAN.md, plan files, and decisions index.

## Step 0 — Guards

1. Read the `Developer` field from `.claude/CLAUDE.md`. If it is `UNCONFIGURED` or missing, stop:
   **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**

2. Check if `main/STATE.md` exists in `${PLANNING_REPO}`. If it does, stop:
   **"Project already initialized — STATE.md exists. Use `/moin` to start a session."**

## Step 1 — Gather project info

Ask these questions in a single message:

1. **"What's your project name?"** (e.g., "MyApp Redesign", "API Migration")
2. **"Describe the project in 1-2 sentences."** (What are you building/changing?)
3. **"What are the main goals?"** (Bullet points — what does success look like?)
4. **"Briefly describe the existing codebase, if any."** (Language, framework, size — or "greenfield" if starting fresh)

## Step 2 — Define phases

Present three approaches:

> **How would you like to plan your phases?**
>
> **A)** "Describe your plan and I'll suggest phases" — Tell me what you want to accomplish and I'll propose 3-6 phases with goals and tasks.
>
> **B)** "List your phases directly" — Give me your phase names and I'll create the structure.
>
> **C)** "Start with a single phase and define more later" — I'll create a minimal "Phase 1: Getting Started" and you can add phases as the project evolves.

### For option A:
- Ask the developer to describe their high-level plan
- Propose 3-6 phases, each with: name, goal (1 sentence), key tasks (3-5 bullets), success criteria
- Ask: **"Does this phase breakdown work? Any changes?"**
- Iterate until confirmed

### For option B:
- Accept the developer's phase list
- For each phase, ask for: goal (1 sentence), key tasks (3-5 bullets)
- Or accept minimal input and fill in reasonable defaults

### For option C:
- Create a single phase: "Phase 1: Getting Started"
- Goal: "Set up the project foundation and begin initial work"
- Tasks: "Define detailed objectives", "Set up development environment", "Begin initial implementation"

## Step 3 — Create plan files

After phases are confirmed, create:

### `plan/overview.md`
```markdown
# {Project Name} — Plan Overview

## Vision
{1-2 sentence project description from Step 1}

## Goals
{Bullet points from Step 1}

## Phase Summary

| Phase | Name | Goal | Status |
|-------|------|------|--------|
| 1 | {name} | {goal} | Not Started |
| ... | ... | ... | ... |

## Risk Register

| # | Risk | Impact | Mitigation | Status |
|---|------|--------|------------|--------|
| _No risks identified yet — add risks as they emerge._ |
```

### `plan/phase-{N}-{slug}.md` (one per phase)
```markdown
# Phase {N}: {Name}

**Goal**: {goal}
**Status**: Not Started

## Tasks

{Numbered task list from phase definition}

## Success Criteria

{Success criteria if defined, or "To be defined as work progresses."}

## Notes

_No notes yet._
```

### `main/PLAN.md`
```markdown
# {Project Name} — Master Plan

> Table of contents for plan files. Read `plan/overview.md` for the full picture.

## Plan Files

| File | Contents |
|------|----------|
| [overview.md](../plan/overview.md) | Vision, phase summary, risk register |
| [phase-1-{slug}.md](../plan/phase-1-{slug}.md) | {Phase 1 name} |
| ... | ... |
```

## Step 4 — Create STATE.md

Create `main/STATE.md`:

```markdown
# Project State — {Project Name}

**Last Updated**: {today's date} (Start of Project)
**Phase**: Phase 1 — {Phase 1 name}
**Day**: 1

## Next Actions

| # | Action | Owner | Status |
|---|--------|-------|--------|
| 1 | {First task from Phase 1} | {Developer} | Pending |
| 2 | {Second task from Phase 1} | {Developer} | Pending |
| 3 | {Third task from Phase 1} | {Developer} | Pending |

## Phase Progress

| Phase | Name | Status |
|-------|------|--------|
| 1 | {Phase 1 name} | In Progress |
| ... | ... | Not Started |

## Decisions Made

| # | Decision | Date | Link |
|---|----------|------|------|
| _No decisions recorded yet._ |

## Open Decisions

| # | Question | Context | Priority |
|---|----------|---------|----------|
| _No open decisions yet._ |

## Team & Branches

| Developer | Focus | Branch |
|-----------|-------|--------|
| {Developer} | Phase 1 | main |

## Key Files

> File discovery uses glob patterns — no static inventory needed.

## Handover Notes

| Developer | File |
|-----------|------|
| {Developer} | `handovers/handover-{lowercase name}.md` |

## Archive

Completed actions are moved to `archive/completed-actions.md` by `/ciao`.
```

## Step 5 — Create decisions/INDEX.md

Create `decisions/INDEX.md`:

```markdown
# Decisions Index

> Architecture Decision Records (ADRs) for the project.
> Use `/decision` to record a new decision.

## Formal Decisions

| # | Title | Date | Status |
|---|-------|------|--------|
| _No decisions recorded yet. Use `/decision` to create one._ |

## Informal Decisions

| Topic | Decision | Date | Context |
|-------|----------|------|---------|
| _Record informal decisions here during `/ciao`._ |
```

## Step 6 — Summary

Present a summary of everything created:

```
Project "{Project Name}" initialized!

Files created:
  main/STATE.md          — Current project state
  main/PLAN.md           — Plan table of contents
  plan/overview.md       — Vision, phases, risk register
  plan/phase-1-{slug}.md — Phase 1: {name}
  ...                    — (one file per phase)
  decisions/INDEX.md     — Decision record index

Next steps:
  Run `/moin` to start your first session.
```
