#!/usr/bin/env bash
# validate-state.sh — Validate STATE.md schema after /ciao writes it
#
# Event:    PostToolUse (Edit|Write)
# Behavior: Advisory warning (exit 0 + stderr)
# Purpose:  Catch missing sections, empty action lists, stale dates,
#           and table structure issues so the next /moin session
#           doesn't silently degrade.
#
# Input:    Tool call JSON via stdin (PostToolUse provides tool_name + tool_input)
# Output:   Warnings on stderr (shown to Claude); nothing on stdout
#
# Can be sourced by post-edit-validate.sh for the validate_state() function,
# or run standalone (backward compatible).

set -euo pipefail
source "$(dirname "$0")/env.sh"

# --- Table helper functions ---

# Extract the section block between a given H2 heading and the next H2 heading.
# Usage: extract_section "$CONTENT" "Section Name"
# Outputs the lines between "## Section Name" and the next "## " (exclusive).
extract_section() {
  local content="$1" heading="$2"
  echo "$content" | sed -n "/^## ${heading}/,/^## /p" | head -n -1
}

# Count pipe-delimited columns in a table row.
# Strips leading/trailing whitespace, then counts pipes minus outer ones.
# Usage: count_columns "| a | b | c |"  => 3
count_columns() {
  local row="$1"
  # Remove leading/trailing whitespace
  row=$(echo "$row" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
  # Count pipes, subtract 1 for leading pipe (trailing pipe makes it balance)
  local pipe_count
  pipe_count=$(echo "$row" | tr -cd '|' | wc -c)
  # Columns = pipes - 1 (a row like |a|b|c| has 4 pipes and 3 columns)
  echo $(( pipe_count - 1 ))
}

# Get data rows from a section's table (skip header row and separator row).
# Usage: get_table_data_rows "$SECTION_BLOCK"
# Outputs only data rows (lines matching |...|, skipping first two such lines).
get_table_data_rows() {
  local block="$1"
  echo "$block" | grep -E '^\s*\|.*\|' | tail -n +3
}

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

  # --- Table structure: Phase Progress (3-4 columns) ---
  local PHASE_BLOCK
  PHASE_BLOCK=$(extract_section "$CONTENT" "Phase Progress")
  if echo "$PHASE_BLOCK" | grep -qE '^\s*\|.*\|'; then
    local PHASE_DATA_ROWS
    PHASE_DATA_ROWS=$(get_table_data_rows "$PHASE_BLOCK")
    if [[ -n "$PHASE_DATA_ROWS" ]]; then
      local LINE_NUM=0
      while IFS= read -r row; do
        LINE_NUM=$((LINE_NUM + 1))
        local COL_COUNT
        COL_COUNT=$(count_columns "$row")
        if [[ $COL_COUNT -lt 3 ]]; then
          WARNINGS="${WARNINGS}  ⚠ Phase Progress row ${LINE_NUM}: expected 3-4 columns, found ${COL_COUNT}\n"
        fi

        # Validate Status column (column 2) for known values
        local STATUS_VAL
        STATUS_VAL=$(echo "$row" | awk -F'|' '{print $3}' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//;s/\*//g')
        if [[ -n "$STATUS_VAL" ]]; then
          case "$STATUS_VAL" in
            "Not started"|"In Progress"|"Done"|"Complete"|"Completed") ;;
            *)
              if ! echo "$STATUS_VAL" | grep -qE '^Day [0-9]+ done$'; then
                WARNINGS="${WARNINGS}  ⚠ Phase Progress row ${LINE_NUM}: unknown status '${STATUS_VAL}'\n"
              fi
              ;;
          esac
        fi

        # Validate PlanFile column (column 4) if present and PLANNING_DIR is available
        if [[ $COL_COUNT -ge 4 ]]; then
          local PLAN_FILE_VAL
          PLAN_FILE_VAL=$(echo "$row" | awk -F'|' '{print $5}' | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
          # Strip markdown link syntax: [text](path) -> path
          if echo "$PLAN_FILE_VAL" | grep -qE '\[.*\]\(.*\)'; then
            PLAN_FILE_VAL=$(echo "$PLAN_FILE_VAL" | sed 's/.*\[.*\](\([^)]*\)).*/\1/')
          fi
          if [[ -n "$PLAN_FILE_VAL" && -n "${PLANNING_DIR:-}" && "$PLANNING_DIR" != '${PLANNING_REPO}' && -d "$PLANNING_DIR" ]]; then
            local RESOLVED_PATH="${PLANNING_DIR}/${PLAN_FILE_VAL}"
            if [[ ! -f "$RESOLVED_PATH" ]]; then
              WARNINGS="${WARNINGS}  ⚠ Phase Progress row ${LINE_NUM}: PlanFile '${PLAN_FILE_VAL}' not found\n"
            fi
          fi
        fi
      done <<< "$PHASE_DATA_ROWS"
    fi
  fi

  # --- Table structure: Next Actions (at least 4 columns) ---
  if echo "$ACTIONS_BLOCK" | grep -qE '^\s*\|.*\|'; then
    local ACTIONS_DATA_ROWS
    ACTIONS_DATA_ROWS=$(get_table_data_rows "$ACTIONS_BLOCK")
    if [[ -n "$ACTIONS_DATA_ROWS" ]]; then
      local LINE_NUM=0
      while IFS= read -r row; do
        LINE_NUM=$((LINE_NUM + 1))
        local COL_COUNT
        COL_COUNT=$(count_columns "$row")
        if [[ $COL_COUNT -lt 4 ]]; then
          WARNINGS="${WARNINGS}  ⚠ Next Actions row ${LINE_NUM}: expected at least 4 columns (#, Action, Owner, Status), found ${COL_COUNT}\n"
        fi
      done <<< "$ACTIONS_DATA_ROWS"
    fi
  fi

  # --- Table structure: Open Decisions (3 columns) ---
  local OPEN_DECISIONS_BLOCK
  OPEN_DECISIONS_BLOCK=$(extract_section "$CONTENT" "Open Decisions")
  if echo "$OPEN_DECISIONS_BLOCK" | grep -qE '^\s*\|.*\|'; then
    local OD_DATA_ROWS
    OD_DATA_ROWS=$(get_table_data_rows "$OPEN_DECISIONS_BLOCK")
    if [[ -n "$OD_DATA_ROWS" ]]; then
      local LINE_NUM=0
      while IFS= read -r row; do
        LINE_NUM=$((LINE_NUM + 1))
        local COL_COUNT
        COL_COUNT=$(count_columns "$row")
        if [[ $COL_COUNT -lt 3 ]]; then
          WARNINGS="${WARNINGS}  ⚠ Open Decisions row ${LINE_NUM}: expected 3 columns (Decision, When, Impact), found ${COL_COUNT}\n"
        fi
      done <<< "$OD_DATA_ROWS"
    fi
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
