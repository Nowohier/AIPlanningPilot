#!/usr/bin/env bash
# validate-state.sh — Validate STATE.md schema after /ciao writes it
#
# Event:    PostToolUse (Edit|Write)
# Behavior: Advisory warning (exit 0 + stderr)
# Purpose:  Catch missing sections, empty action lists, stale dates
#           so the next /moin session doesn't silently degrade.
#
# Input:    Tool call JSON via stdin (PostToolUse provides tool_name + tool_input)
# Output:   Warnings on stderr (shown to Claude); nothing on stdout
#
# Can be sourced by post-edit-validate.sh for the validate_state() function,
# or run standalone (backward compatible).

set -euo pipefail
source "$(dirname "$0")/env.sh"

# --- Core validation function ---
# Takes a normalized (forward-slash) file path as $1.
# Emits warnings/errors on stderr. Always returns 0 (advisory).
validate_state() {
  local NORMALIZED_PATH="$1"

  if [[ ! -f "$NORMALIZED_PATH" ]]; then
    # Try the env.sh-derived path as fallback
    require_planning_repo || return 0
    NORMALIZED_PATH="$STATE_FILE"
  fi

  if [[ ! -f "$NORMALIZED_PATH" ]]; then
    return 0
  fi

  local CONTENT
  CONTENT=$(cat "$NORMALIZED_PATH")
  local ERRORS=""
  local WARNINGS=""

  # --- Required H2 sections (what /moin expects to read) ---
  for section in "Next Actions" "Phase Progress" "Decisions Made" "Open Decisions"; do
    if ! echo "$CONTENT" | grep -q "^## ${section}"; then
      ERRORS="${ERRORS}  ✗ Missing required section: '## ${section}'\n"
    fi
  done

  # --- Phase field should exist in header ---
  if ! echo "$CONTENT" | grep -qE '\*\*Phase\*\*:.*Phase [0-9]'; then
    WARNINGS="${WARNINGS}  ⚠ Header missing '**Phase**: Phase N ...' line\n"
  fi

  # --- Next Actions should have at least one item ---
  local ACTIONS_BLOCK
  ACTIONS_BLOCK=$(echo "$CONTENT" | sed -n '/^## Next Actions/,/^## /p' | head -n -1)
  if ! echo "$ACTIONS_BLOCK" | grep -qE '^\s*\|.*\|'; then
    # No table rows found
    if ! echo "$ACTIONS_BLOCK" | grep -qE '^\s*[-*0-9]'; then
      WARNINGS="${WARNINGS}  ⚠ Next Actions section appears empty (no table rows or list items)\n"
    fi
  fi

  # --- Last Updated date should exist ---
  if echo "$CONTENT" | grep -iE '\*\*Last Updated\*\*.*[0-9]{4}-[0-9]{2}-[0-9]{2}' >/dev/null 2>&1; then
    local LAST_DATE
    LAST_DATE=$(echo "$CONTENT" | grep -i 'Last Updated' | grep -oE '[0-9]{4}-[0-9]{2}-[0-9]{2}' | head -1)
    if [[ -n "$LAST_DATE" ]]; then
      local LAST_EPOCH NOW_EPOCH DAYS_OLD
      LAST_EPOCH=$(date -d "$LAST_DATE" +%s 2>/dev/null || echo "0")
      NOW_EPOCH=$(date +%s)
      if [[ "$LAST_EPOCH" != "0" ]]; then
        DAYS_OLD=$(( (NOW_EPOCH - LAST_EPOCH) / 86400 ))
        if [[ $DAYS_OLD -gt 3 ]]; then
          WARNINGS="${WARNINGS}  ⚠ Last Updated date is ${DAYS_OLD} days ago (${LAST_DATE})\n"
        fi
      fi
    fi
  else
    WARNINGS="${WARNINGS}  ⚠ No '**Last Updated**: YYYY-MM-DD' found in header\n"
  fi

  # --- Report via stderr (advisory) ---
  if [[ -n "$ERRORS" || -n "$WARNINGS" ]]; then
    echo "" >&2
    echo "━━━ STATE.md Validation ━━━" >&2
    if [[ -n "$ERRORS" ]]; then
      echo "ERRORS (sections missing — /moin may not work correctly):" >&2
      printf '%b' "$ERRORS" >&2
    fi
    if [[ -n "$WARNINGS" ]]; then
      echo "WARNINGS:" >&2
      printf '%b' "$WARNINGS" >&2
    fi
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━" >&2
  fi

  return 0
}

# --- Standalone execution (backward compatible) ---
# When run directly (not sourced), read stdin and filter by path.
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
  source "$(dirname "$0")/json-helper.sh"
  INPUT=$(cat)

  # Extract file_path from tool input JSON
  FILE_PATH=$(echo "$INPUT" | json_get "tool_input.file_path")
  if [[ -z "$FILE_PATH" ]]; then
    FILE_PATH=$(echo "$INPUT" | json_get "tool_input.path")
  fi

  # Only trigger for STATE.md
  case "$FILE_PATH" in
    *STATE.md) ;;
    *) exit 0 ;;
  esac

  # Also verify it's the project's STATE.md, not some random STATE.md
  case "$FILE_PATH" in
    *main*STATE.md|*main*STATE\.md) ;;
    *) exit 0 ;;
  esac

  # Normalize path: replace backslashes with forward slashes for bash
  NORMALIZED_PATH="${FILE_PATH//\\//}"

  validate_state "$NORMALIZED_PATH"
  exit 0
fi
