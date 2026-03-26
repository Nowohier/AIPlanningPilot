#!/usr/bin/env bash
# block-protected-paths.sh -- Block Edit/Write to protected directories
#
# Event:    PreToolUse (Edit|Write)
# Behavior: Hard block (exit 2) if file is in a protected directory
# Purpose:  Prevents Claude from editing files in directories that are
#           out of scope or contain generated code.
#           Which directories are protected is configured per project.
#
# Configuration: PROTECTED_PATHS and PROTECTED_PATTERNS in hooks-config.sh
# Input:    Tool call JSON via stdin
# Output:   Block message on stderr (exit 2) or silent pass (exit 0)

set -euo pipefail

# --- Source config ---
source "$(dirname "$0")/env.sh"

# Fast exit if no protection configured
if [[ -z "$PROTECTED_PATHS" && -z "$PROTECTED_PATTERNS" ]]; then
  exit 0
fi

# --- Read stdin and extract file_path via grep (no Node.js) ---
INPUT=$(cat)
FILE_PATH=$(echo "$INPUT" | grep -oE '"file_path"\s*:\s*"[^"]*"' | head -1 | sed 's/.*"file_path"\s*:\s*"//;s/"$//')

if [[ -z "$FILE_PATH" ]]; then
  exit 0
fi

# --- Normalize path: backslashes to forward slashes ---
NORMALIZED="${FILE_PATH//\\//}"

# --- Check PROTECTED_PATHS (directory names) ---
for dir in $PROTECTED_PATHS; do
  # Match: starts with dir/ or contains /dir/
  if [[ "$NORMALIZED" == "${dir}/"* ]] || [[ "$NORMALIZED" == *"/${dir}/"* ]]; then
    echo "BLOCKED: Cannot edit files in protected directory." >&2
    echo "" >&2
    echo "  File: $FILE_PATH" >&2
    echo "  Matched rule: ${dir}/" >&2
    echo "" >&2
    echo "  Protected directories: $PROTECTED_PATHS" >&2
    [[ -n "$PROTECTED_PATTERNS" ]] && echo "  Protected patterns: $PROTECTED_PATTERNS (case-insensitive)" >&2
    echo "  Configure in: \$PLANNING_DIR/main/hooks-config.sh" >&2
    exit 2
  fi
done

# --- Check PROTECTED_PATTERNS (case-insensitive, anywhere in path) ---
NORMALIZED_LOWER=$(echo "$NORMALIZED" | tr '[:upper:]' '[:lower:]')
for pattern in $PROTECTED_PATTERNS; do
  pattern_lower=$(echo "$pattern" | tr '[:upper:]' '[:lower:]')
  if [[ "$NORMALIZED_LOWER" == *"/${pattern_lower}/"* ]]; then
    echo "BLOCKED: Cannot edit files in protected directory." >&2
    echo "" >&2
    echo "  File: $FILE_PATH" >&2
    echo "  Matched rule: */${pattern}/ (case-insensitive)" >&2
    echo "" >&2
    echo "  Protected directories: $PROTECTED_PATHS" >&2
    [[ -n "$PROTECTED_PATTERNS" ]] && echo "  Protected patterns: $PROTECTED_PATTERNS (case-insensitive)" >&2
    echo "  Configure in: \$PLANNING_DIR/main/hooks-config.sh" >&2
    exit 2
  fi
done

exit 0
