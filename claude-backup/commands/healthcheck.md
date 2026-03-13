# Environment Health Check

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/healthcheck`. Do NOT trigger from conversational mentions of "health check", "check environment", "is everything working", or similar phrasing.

Verify that the development environment is correctly configured.
Run this when something feels off, after `/onboard`, or to verify a new machine setup.

> **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
> Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.

## Step 0 — Guard (relaxed)

Read the `Developer` field from `.claude/CLAUDE.md`. Unlike other commands, do **NOT** refuse
if `UNCONFIGURED` — instead, record it as the first FAIL and continue checking what else works.
The point of `/healthcheck` is to diagnose problems, not refuse to run when problems exist.

Also read the path variables table from `.claude/CLAUDE.md` to resolve `${PROJECT_REPO}` and
`${PLANNING_REPO}` for subsequent checks. If the table is missing or values look like
unresolved placeholders, record that context and continue — later checks that depend on these
paths will naturally fail.

## Step 1 — Run all checks

Run checks 1–12 below. For each check:
- Attempt the check
- Record **PASS**, **WARN**, or **FAIL** with a short detail string
- On FAIL/WARN, add a one-line remediation hint (→ prefix)
- Continue to the next check regardless of result

Run file reads and bash commands **in parallel** where independent.

### Check 1 — Developer configured

Read the `Developer` field from `.claude/CLAUDE.md`.

- **PASS**: Field exists, is not `UNCONFIGURED`, and is not empty. Show the developer name.
- **FAIL**: Field is `UNCONFIGURED` or missing.
  → Run `/onboard` to configure your developer identity.

### Check 2 — PROJECT_REPO exists

Read the `${PROJECT_REPO}` value from the path variables table in `.claude/CLAUDE.md`,
then run `bash: test -d "<path>"`.

- **PASS**: Directory exists. Show the path.
- **FAIL**: Directory does not exist or path variable is unresolved.
  → Update the path in `.claude/CLAUDE.md` or run `/onboard`.

### Check 3 — PLANNING_REPO exists

Same as Check 2 but for `${PLANNING_REPO}`.

- **PASS**: Directory exists. Show the path.
- **FAIL**: Directory does not exist or path variable is unresolved.
  → Update the path in `.claude/CLAUDE.md` or run `/onboard`.

### Check 4 — env.sh configured

Read `.claude/hooks/env.sh`. Check the `PROJECT_REPO=` and `DEVELOPER=` lines.

- **PASS**: No `${...}` placeholders remain, and `DEVELOPER` is not `UNCONFIGURED`.
- **FAIL**: Placeholders like `${PROJECT_REPO}` still present, or `DEVELOPER` is `UNCONFIGURED`.
  → Run `/onboard` to resolve env.sh placeholders, or run `/moin` to sync from template.

### Check 5 — Node.js available

Run `bash: node --version`.

- **PASS**: Exits 0, prints version. Show the version string.
- **FAIL**: Command not found or non-zero exit.
  → json-helper.sh (used by all hooks) requires Node.js on PATH.

### Check 6 — Hook registrations complete

Read `.claude/settings.json` and verify all 3 hook points from `claude-backup/settings.json`
are present:
- `PreToolUse`: hook command referencing `block-literal-paths-in-templates.sh`
- `PostToolUse`: hook command referencing `post-edit-validate.sh`
- `UserPromptSubmit`: hook command referencing `check-handover-freshness.sh`

- **PASS**: All 3 present. Show "3/3".
- **FAIL**: Any missing. Show which are missing.
  → Run `/moin` to sync hook registrations from template, then restart the session.

### Check 7 — Hook scripts exist

For each `.sh` file referenced in `.claude/settings.json` hook commands, plus the files
they source (`env.sh`, `json-helper.sh`, `validate-state.sh`, `validate-decision-record.sh`),
run `bash: test -f` on the full path under `.claude/hooks/`.

Expected files:
- `.claude/hooks/env.sh`
- `.claude/hooks/json-helper.sh`
- `.claude/hooks/block-literal-paths-in-templates.sh`
- `.claude/hooks/check-handover-freshness.sh`
- `.claude/hooks/post-edit-validate.sh`
- `.claude/hooks/validate-state.sh`
- `.claude/hooks/validate-decision-record.sh`

- **PASS**: All files exist. Show count (e.g., "7/7").
- **FAIL**: Any missing. List the missing filenames.
  → Run `/moin` to sync hook files from template.

### Check 8 — LF line endings on .sh files

Run `bash: file .claude/hooks/*.sh` or check each file for `\r\n` bytes (e.g.,
`grep -cP '\r$' .claude/hooks/*.sh`).

- **PASS**: No CRLF detected on any .sh file.
- **FAIL**: CRLF detected. List the affected files.
  → Run `/moin` (which fixes line endings during sync) or manually: `sed -i 's/\r$//' .claude/hooks/*.sh`

### Check 9 — Handover file exists

Derive the handover filename from the Developer field: `handover-{lowercase name}.md`.
Check that `${PLANNING_REPO}/handovers/handover-{name}.md` exists.

If Developer is `UNCONFIGURED` (Check 1 failed), skip this check and mark as **FAIL**
with note "cannot check — developer not configured".

- **PASS**: File exists. Show the filename.
- **FAIL**: File does not exist.
  → Run `/onboard` to create your handover file.

### Check 10 — STATE.md valid

Read `${PLANNING_REPO}/main/STATE.md` and verify these 4 required
sections exist:
- `## Next Actions`
- `## Phase Progress`
- `## Decisions Made`
- `## Open Decisions`

If PLANNING_REPO is unresolved (Check 3 failed), skip and mark as **FAIL**
with note "cannot check — PLANNING_REPO not configured".

- **PASS**: All 4 sections present. Show "4/4".
- **FAIL**: Any sections missing. List which are missing.
  → STATE.md may be corrupted. Check the file manually or restore from `archive/state-snapshots/`.

### Check 11 — STATE.md freshness

Extract the `Last Updated` date from STATE.md (typically in the header metadata).

- **PASS**: Updated within the last 5 days. Show how many days ago.
- **WARN**: Updated 4–5 days ago. Show the date.
  → Consider running `/ciao` to update, or verify STATE.md is current.
- **FAIL**: Updated more than 5 days ago, or date not found.
  → STATE.md is stale. Run `/ciao` to persist current state.

### Check 12 — Template sync

Compare file lists:
- `claude-backup/commands/` vs `.claude/commands/`
- `claude-backup/hooks/` vs `.claude/hooks/`

Only check for files **missing from `.claude/`** that exist in `claude-backup/`.
Do NOT compare file contents (templates have variables, runtime has literal paths).

- **PASS**: No files missing. Show "OK (no missing files)".
- **FAIL**: Files in template not found in `.claude/`. List the missing files.
  → Run `/moin` to sync template files to `.claude/`.

## Step 2 — Present report

Present the formatted report using this layout:

```
Health Check Report
═══════════════════

 [PASS]  Developer configured: {name}
 [PASS]  PROJECT_REPO exists: {path}
 [PASS]  PLANNING_REPO exists: {path}
 [PASS]  env.sh configured (no placeholders)
 [PASS]  Node.js available: {version}
 [PASS]  Hook registrations complete (3/3)
 [PASS]  Hook scripts exist (7/7)
 [PASS]  LF line endings on all .sh files
 [PASS]  Handover file exists: handover-{name}.md
 [PASS]  STATE.md sections valid (4/4)
 [PASS]  STATE.md fresh (updated {N} day(s) ago)
 [PASS]  Template sync OK (no missing files)

Result: {passed}/12 checks passed. {summary}
```

For failures and warnings, add a remediation hint on the next line:
```
 [FAIL]  Node.js available: command not found
         → json-helper.sh (used by all hooks) requires Node.js on PATH
 [WARN]  STATE.md fresh: updated 4 days ago (2026-03-06)
         → Consider running /ciao to update, or verify STATE.md is current
```

End with one of:
- **All PASS**: "Environment is healthy."
- **Any WARN but no FAIL**: "Environment is functional but has warnings."
- **Any FAIL**: "Environment has issues. Run `/onboard` to reconfigure, or fix the items above."
