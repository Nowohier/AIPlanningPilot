#!/usr/bin/env bash
# validate-decision-record.sh — Validate decision records after /decision creates them
#
# Event:    PostToolUse (Write)
# Behavior: Advisory warning (exit 0 + stderr)
# Purpose:  Catch missing required fields (Date, Decision section) and numbering
#           gaps in ADR files. Fires only for files in decisions/ directory.
#
# Input:    Tool call JSON via stdin
# Output:   Warnings on stderr if validation issues found
#
# Can be sourced by post-edit-validate.sh for the validate_decision_record() function,
# or run standalone (backward compatible).

set -euo pipefail
source "$(dirname "$0")/env.sh"

# --- Core validation function ---
# Takes a normalized (forward-slash) file path as $1.
# Emits warnings on stderr. Always returns 0 (advisory).
validate_decision_record() {
  local NORMALIZED_PATH="$1"

  if [[ ! -f "$NORMALIZED_PATH" ]]; then
    return 0
  fi

  local CONTENT
  CONTENT=$(cat "$NORMALIZED_PATH")
  local BASENAME
  BASENAME=$(basename "$NORMALIZED_PATH")
  local WARNINGS=""

  # --- Required fields ---
  # Date is always expected
  if ! echo "$CONTENT" | grep -qi '\*\*Date\*\*.*:'; then
    WARNINGS="${WARNINGS}  ✗ Missing '**Date**:' field\n"
  fi

  # Must have a ## Decision section
  if ! echo "$CONTENT" | grep -q '^## Decision'; then
    WARNINGS="${WARNINGS}  ✗ Missing '## Decision' section\n"
  fi

  # Must have a ## Context section
  if ! echo "$CONTENT" | grep -q '^## Context'; then
    WARNINGS="${WARNINGS}  ✗ Missing '## Context' section\n"
  fi

  # Title should match the number in filename
  if [[ "$BASENAME" =~ ^([0-9]+)- ]]; then
    local FILE_NUM="${BASH_REMATCH[1]}"
    local FILE_NUM_INT=$((10#$FILE_NUM))

    # Check title matches: "# Decision NNN:"
    if ! echo "$CONTENT" | grep -qE "^# Decision (${FILE_NUM_INT}|${FILE_NUM}):"; then
      WARNINGS="${WARNINGS}  ⚠ Title doesn't match filename number (expected 'Decision ${FILE_NUM_INT}:' or 'Decision ${FILE_NUM}:')\n"
    fi

    # --- Check for numbering gaps ---
    require_planning_repo || {
      # Can't check numbering without the decisions dir — skip silently
      if [[ -n "$WARNINGS" ]]; then
        echo "" >&2
        echo "━━━ Decision Record Validation: $BASENAME ━━━" >&2
        printf '%b' "$WARNINGS" >&2
        echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" >&2
      fi
      return 0
    }

    # Get all existing decision numbers, excluding the current file
    local EXISTING_NUMS
    EXISTING_NUMS=$(ls "$DECISIONS_DIR"/[0-9]*.md 2>/dev/null | \
      xargs -I{} basename {} | \
      grep -v "^${BASENAME}$" | \
      grep -oE '^[0-9]+' | \
      sort -n || true)

    if [[ -n "$EXISTING_NUMS" ]]; then
      local HIGHEST HIGHEST_INT
      HIGHEST=$(echo "$EXISTING_NUMS" | tail -1)
      HIGHEST_INT=$((10#$HIGHEST))

      # Check for gap: new file number should be at most highest + 1
      if [[ $FILE_NUM_INT -gt $((HIGHEST_INT + 1)) ]]; then
        local EXPECTED
        EXPECTED=$(printf '%03d' $((HIGHEST_INT + 1)))
        WARNINGS="${WARNINGS}  ⚠ Numbering gap: expected next file to be ${EXPECTED}, got ${FILE_NUM}\n"
      fi
    fi
  fi

  # --- Report ---
  if [[ -n "$WARNINGS" ]]; then
    echo "" >&2
    echo "━━━ Decision Record Validation: $BASENAME ━━━" >&2
    printf '%b' "$WARNINGS" >&2
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" >&2
  fi

  return 0
}

# --- Standalone execution (backward compatible) ---
# When run directly (not sourced), read stdin and filter by path.
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
  source "$(dirname "$0")/json-helper.sh"
  INPUT=$(cat)

  # Extract file_path
  FILE_PATH=$(echo "$INPUT" | json_get "tool_input.file_path")
  if [[ -z "$FILE_PATH" ]]; then
    FILE_PATH=$(echo "$INPUT" | json_get "tool_input.path")
  fi

  # Only trigger for numbered decision files (NNN-*.md)
  case "$FILE_PATH" in
    *decisions/[0-9]*|*decisions\\[0-9]*) ;;
    *) exit 0 ;;
  esac

  # Normalize path
  NORMALIZED_PATH="${FILE_PATH//\\//}"

  validate_decision_record "$NORMALIZED_PATH"
  exit 0
fi
