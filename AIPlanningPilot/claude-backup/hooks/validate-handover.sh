#!/usr/bin/env bash
# validate-handover.sh — Validate handover file structure after /ciao writes it
#
# Event:    PostToolUse (Edit|Write)
# Behavior: Advisory warning (exit 0 + stderr)
# Purpose:  Catch missing sections, missing metadata so the next /moin
#           session doesn't silently degrade.
#
# Input:    Tool call JSON via stdin (PostToolUse provides tool_name + tool_input)
# Output:   Warnings on stderr (shown to Claude); nothing on stdout
#
# Can be sourced by post-edit-validate.sh for the validate_handover() function,
# or run standalone (backward compatible).

set -euo pipefail
source "$(dirname "$0")/env.sh"

# --- Core validation function ---
# Takes a normalized (forward-slash) file path as $1.
# Emits warnings/errors on stderr. Always returns 0 (advisory).
validate_handover() {
  local NORMALIZED_PATH="$1"

  if [[ ! -f "$NORMALIZED_PATH" ]]; then
    return 0
  fi

  local CONTENT
  CONTENT=$(cat "$NORMALIZED_PATH")
  local ERRORS=""
  local WARNINGS=""

  # --- H1 title should exist ---
  if ! echo "$CONTENT" | grep -q "^# Handover Notes"; then
    WARNINGS="${WARNINGS}  ! Missing H1 title: '# Handover Notes -- {Name}'\n"
  fi

  # --- Last updated blockquote should exist ---
  if ! echo "$CONTENT" | grep -qE '>\s*Last updated:'; then
    ERRORS="${ERRORS}  x Missing required metadata: '> Last updated: YYYY-MM-DD'\n"
  fi

  # --- Required H2 sections ---
  for section in "For Next Session" "Decisions & Findings" "Open Threads" "From Last Session"; do
    if ! echo "$CONTENT" | grep -q "^## ${section}"; then
      ERRORS="${ERRORS}  x Missing required section: '## ${section}'\n"
    fi
  done

  # --- Each section should have content ---
  for section in "For Next Session" "Decisions & Findings" "Open Threads"; do
    local SECTION_BLOCK
    SECTION_BLOCK=$(echo "$CONTENT" | sed -n "/^## ${section}/,/^## /p" | head -n -1)
    if ! echo "$SECTION_BLOCK" | grep -qE '^\s*[-*]'; then
      WARNINGS="${WARNINGS}  ! Section '${section}' appears empty (no bullet items)\n"
    fi
  done

  # --- From Last Session should have content (bullets or paragraph) ---
  local IN_SECTION=false
  local HAS_CONTENT=false
  while IFS= read -r line; do
    if [[ "$line" == "## From Last Session"* ]]; then
      IN_SECTION=true
      continue
    fi
    if $IN_SECTION; then
      if [[ "$line" == "## "* ]]; then
        break
      fi
      local trimmed="${line#"${line%%[![:space:]]*}"}"
      if [[ -n "$trimmed" ]]; then
        HAS_CONTENT=true
        break
      fi
    fi
  done <<< "$CONTENT"
  if $IN_SECTION && ! $HAS_CONTENT; then
    WARNINGS="${WARNINGS}  ! Section 'From Last Session' appears empty\n"
  fi

  # --- Session Log format check (optional, advisory) ---
  if echo "$CONTENT" | grep -q "^## Session Log"; then
    local BAD_DATES
    BAD_DATES=$(echo "$CONTENT" | sed -n '/^## Session Log/,/^## /p' | grep '^### ' | grep -v '^### [0-9]\{4\}-[0-9]\{2\}-[0-9]\{2\}' || true)
    if [[ -n "$BAD_DATES" ]]; then
      WARNINGS="${WARNINGS}  ! Session Log has entries not in ### YYYY-MM-DD format: ${BAD_DATES}\n"
    fi
  fi

  # --- Report via stderr (advisory) ---
  if [[ -n "$ERRORS" || -n "$WARNINGS" ]]; then
    echo "" >&2
    echo "--- Handover Validation ---" >&2
    if [[ -n "$ERRORS" ]]; then
      echo "ERRORS (sections missing -- /moin may not work correctly):" >&2
      printf '%b' "$ERRORS" >&2
    fi
    if [[ -n "$WARNINGS" ]]; then
      echo "WARNINGS:" >&2
      printf '%b' "$WARNINGS" >&2
    fi
    echo "---------------------------" >&2
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

  # Only trigger for handover files
  case "$FILE_PATH" in
    *handovers/handover-*|*handovers\\handover-*) ;;
    *) exit 0 ;;
  esac

  # Normalize path: replace backslashes with forward slashes for bash
  NORMALIZED_PATH="${FILE_PATH//\\//}"

  validate_handover "$NORMALIZED_PATH"
  exit 0
fi
