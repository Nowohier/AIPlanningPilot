#!/usr/bin/env bash
# post-edit-validate.sh — Merged PostToolUse validator (STATE.md + decision records)
#
# Event:    PostToolUse (Edit|Write)
# Behavior: Advisory warning (exit 0 + stderr)
# Purpose:  Replaces running validate-state.sh and validate-decision-record.sh as
#           separate hook processes. Single stdin read, single path extraction,
#           bash-native fast-path to skip Node.js for irrelevant files.
#
# Performance: ~99% of edits touch neither STATE.md nor decision records.
#   The fast-path extracts file_path via grep (no Node.js) and exits immediately
#   for irrelevant files, eliminating two Node.js invocations per edit.
#
# Input:    Tool call JSON via stdin
# Output:   Warnings on stderr if validation issues found

set -euo pipefail

HOOK_DIR="$(dirname "$0")"

# Read stdin once
INPUT=$(cat)

# --- Fast-path: extract file_path with grep (no Node.js) ---
FAST_PATH=$(echo "$INPUT" | grep -oE '"file_path"\s*:\s*"[^"]*"' | head -1 | sed 's/.*"file_path"\s*:\s*"//;s/"$//')

# Route by path — exit immediately for irrelevant files
case "$FAST_PATH" in
  *main*STATE.md|*main*STATE\.md)
    # STATE.md validation — need full parse + validation
    source "${HOOK_DIR}/json-helper.sh"
    FILE_PATH=$(echo "$INPUT" | json_get "tool_input.file_path")
    if [[ -z "$FILE_PATH" ]]; then
      FILE_PATH=$(echo "$INPUT" | json_get "tool_input.path")
    fi

    # Normalize path: replace backslashes with forward slashes
    NORMALIZED_PATH="${FILE_PATH//\\//}"

    # Source validate-state.sh for the validate_state() function
    source "${HOOK_DIR}/validate-state.sh"
    validate_state "$NORMALIZED_PATH"
    ;;
  *decisions/[0-9]*|*decisions\\[0-9]*)
    # Decision record validation — need full parse + validation
    source "${HOOK_DIR}/json-helper.sh"
    FILE_PATH=$(echo "$INPUT" | json_get "tool_input.file_path")
    if [[ -z "$FILE_PATH" ]]; then
      FILE_PATH=$(echo "$INPUT" | json_get "tool_input.path")
    fi

    # Normalize path: replace backslashes with forward slashes
    NORMALIZED_PATH="${FILE_PATH//\\//}"

    # Source validate-decision-record.sh for the validate_decision_record() function
    source "${HOOK_DIR}/validate-decision-record.sh"
    validate_decision_record "$NORMALIZED_PATH"
    ;;
  *handovers/handover-*|*handovers\\handover-*)
    # Handover file validation — check required sections and metadata
    source "${HOOK_DIR}/json-helper.sh"
    FILE_PATH=$(echo "$INPUT" | json_get "tool_input.file_path")
    if [[ -z "$FILE_PATH" ]]; then
      FILE_PATH=$(echo "$INPUT" | json_get "tool_input.path")
    fi

    # Normalize path: replace backslashes with forward slashes
    NORMALIZED_PATH="${FILE_PATH//\\//}"

    # Source validate-handover.sh for the validate_handover() function
    source "${HOOK_DIR}/validate-handover.sh"
    validate_handover "$NORMALIZED_PATH"
    ;;
  *)
    # ~99% of edits — fast exit, no Node.js spawned
    exit 0
    ;;
esac

exit 0
