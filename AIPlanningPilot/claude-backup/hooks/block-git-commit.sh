#!/usr/bin/env bash
# block-git-commit.sh -- Block git commit commands
#
# Event:    PreToolUse (Bash)
# Behavior: Hard block (exit 2) if command contains git commit
# Purpose:  Prevents Claude from committing on behalf of the developer.
#           The developer reviews and commits changes manually.
#
# Configuration: BLOCK_GIT_COMMIT in hooks-config.sh
# Input:    Tool call JSON via stdin
# Output:   Block message on stderr (exit 2) or silent pass (exit 0)

set -euo pipefail

# --- Source config ---
source "$(dirname "$0")/env.sh"

# Fast exit if commit blocking is disabled
if [[ "$BLOCK_GIT_COMMIT" != "true" ]]; then
  exit 0
fi

# --- Read stdin and extract command via grep (no Node.js) ---
INPUT=$(cat)
COMMAND=$(echo "$INPUT" | grep -oE '"command"\s*:\s*"[^"]*"' | head -1 | sed 's/.*"command"\s*:\s*"//;s/"$//')

if [[ -z "$COMMAND" ]]; then
  exit 0
fi

# --- Check for git commit ---
if echo "$COMMAND" | grep -qE '\bgit\s+commit\b'; then
  echo "BLOCKED: Git commits are handled manually by the developer." >&2
  echo "" >&2
  echo "  Do not run git commit. The developer will review and commit changes themselves." >&2
  exit 2
fi

exit 0
