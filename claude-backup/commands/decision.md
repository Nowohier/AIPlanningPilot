# Record a Decision

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/decision`. Do NOT trigger from conversational mentions of "decision", "decided", "let's decide", or similar phrasing.

Immediately capture an architecture/technology decision so it's not lost.

## Steps

0. **Guard**: Read `Developer` from `.claude/CLAUDE.md`. If `UNCONFIGURED`, refuse and direct to `/onboard`.

1. Ask (if $ARGUMENTS is empty):
   - "What was decided?" (one sentence)
   - "Why?" (one sentence)
   - "What alternatives were considered?" (optional)

2. Determine the next decision number by reading the `decisions/` directory
   in `${PLANNING_REPO}\decisions\`

3. Create `decisions/{NNN}-{slug}.md` with this template:
   ```
   # Decision {NNN}: {Title}

   **Date**: {today}
   **Status**: Decided
   **Decided by**: {developer from .claude/CLAUDE.md}

   ## Context
   {Why this decision was needed}

   ## Decision
   {What was decided}

   ## Alternatives Considered
   {If provided}

   ## Consequences
   {What this means for the project}
   ```

4. Update STATE.md — add a row to "Decisions Made" table

5. Confirm: **"Decision {NNN} recorded: {title}"**
