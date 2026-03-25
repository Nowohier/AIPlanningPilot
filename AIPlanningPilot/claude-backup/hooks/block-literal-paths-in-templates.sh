#!/usr/bin/env bash
# block-literal-paths-in-templates.sh — Prevent literal machine paths in template files
#
# Event:    PreToolUse (Edit|Write)
# Behavior: Hard block (exit 2) if literal Windows drive paths detected
# Purpose:  Template files in claude-backup/ must use ${PROJECT_REPO} and
#           ${PLANNING_REPO} variables, not literal paths like M:\...\project.
#           Literal paths belong only in project/.claude/ (the developer's local copy).
#
# Input:    Tool call JSON via stdin (PreToolUse provides tool_name + tool_input)
# Output:   Block message on stderr (exit 2) or silent pass (exit 0)

set -euo pipefail

# --- Read tool input from stdin ---
INPUT=$(cat)

# --- Fast-path: extract file_path with grep (no Node.js) ---
# ~99% of edits don't touch claude-backup/ — exit immediately for those.
FAST_PATH=$(echo "$INPUT" | grep -oE '"file_path"\s*:\s*"[^"]*"' | head -1 | sed 's/.*"file_path"\s*:\s*"//;s/"$//')

# Only check files in claude-backup/commands/ or claude-backup/hooks/
case "$FAST_PATH" in
  *claude-backup/commands/*|*claude-backup\\commands\\*) ;;
  *claude-backup/hooks/*|*claude-backup\\hooks\\*) ;;
  *) exit 0 ;;
esac

# Allow env.sh — it's the ONE file that's supposed to have literal paths
# (but in the template version it should have ${VAR}, so we still check)
BASENAME=$(basename "$FAST_PATH")
if [[ "$BASENAME" == "env.sh" ]]; then
  exit 0
fi

# --- Slow path: only reached for claude-backup/ files, need Node.js for content ---
source "$(dirname "$0")/json-helper.sh"

# Extract the content being written
# For Write: "content" field; for Edit: "new_string" field
CONTENT=$(echo "$INPUT" | json_get "tool_input.content")
if [[ -z "$CONTENT" ]]; then
  CONTENT=$(echo "$INPUT" | json_get "tool_input.new_string")
fi

if [[ -z "$CONTENT" ]]; then
  exit 0
fi

# Check for literal Windows drive paths — templates should never contain absolute paths
# Match patterns like: M:\CODE_COPY\... or D:/Projects/...
if echo "$CONTENT" | grep -qE '[A-Z]:[/\\]'; then
  echo "BLOCKED: Template file contains literal machine paths." >&2
  echo "" >&2
  echo "  File: $FAST_PATH" >&2
  echo "" >&2
  echo "  Files in claude-backup/ must use \${PROJECT_REPO} and \${PLANNING_REPO}" >&2
  echo "  variable references, not literal paths." >&2
  echo "" >&2
  echo "  Literal paths belong in project/.claude/ (the developer's local copy)." >&2
  echo "  /moin and /onboard deploy from the template and personalize automatically." >&2
  exit 2
fi

exit 0
