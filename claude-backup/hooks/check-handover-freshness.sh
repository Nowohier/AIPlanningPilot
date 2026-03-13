#!/usr/bin/env bash
# check-handover-freshness.sh — Warn about stale handover files when /moin runs
#
# Event:    UserPromptSubmit
# Behavior: Context injection via stdout (warning appears in Claude's context)
# Purpose:  If the developer forgot to run /ciao, the handover file will be stale.
#           This hook fires when /moin is invoked and alerts Claude before it
#           presents the briefing, so the staleness is visible in the output.
#
# Input:    User prompt JSON via stdin
# Output:   Warning on stdout (injected into Claude's context) if handover is stale

set -euo pipefail
source "$(dirname "$0")/env.sh"

# --- Read user prompt from stdin ---
source "$(dirname "$0")/json-helper.sh"
INPUT=$(cat)

# Extract the prompt text
PROMPT=$(echo "$INPUT" | json_get "prompt")

# Only trigger on /moin command
case "$PROMPT" in
  */moin*) ;;
  *) exit 0 ;;
esac

# Need the planning repo to check handover files
require_planning_repo || exit 0

# Need the developer name to find their handover file
if [[ -z "$DEVELOPER" || "$DEVELOPER" == "UNCONFIGURED" ]]; then
  exit 0
fi

DEV_LOWER=$(echo "$DEVELOPER" | tr '[:upper:]' '[:lower:]')
HANDOVER_FILE="${HANDOVER_DIR}/handover-${DEV_LOWER}.md"

if [[ ! -f "$HANDOVER_FILE" ]]; then
  # No handover file — /moin's own guard (Step 3) will handle this
  exit 0
fi

# Try to read "Last updated" date from file content
LAST_DATE=$(grep -i 'last.*updated' "$HANDOVER_FILE" 2>/dev/null | grep -oE '[0-9]{4}-[0-9]{2}-[0-9]{2}' | head -1)

if [[ -z "$LAST_DATE" ]]; then
  # Can't determine freshness — don't warn
  exit 0
fi

# Calculate age in hours
LAST_EPOCH=$(date -d "$LAST_DATE" +%s 2>/dev/null || echo "0")
NOW_EPOCH=$(date +%s)

if [[ "$LAST_EPOCH" == "0" ]]; then
  exit 0
fi

HOURS_OLD=$(( (NOW_EPOCH - LAST_EPOCH) / 3600 ))
DAYS_OLD=$(( HOURS_OLD / 24 ))

# Warn if older than 36 hours (covers a normal overnight gap + buffer)
if [[ $HOURS_OLD -gt 36 ]]; then
  echo ""
  echo "⚠️  STALE HANDOVER: ${DEVELOPER}'s handover file is ${DAYS_OLD} day(s) old (last updated: ${LAST_DATE})."
  echo "   The last session may not have run /ciao. Handover notes could be outdated."
  echo "   Review the handover content carefully before relying on it."
  echo ""
fi

exit 0
