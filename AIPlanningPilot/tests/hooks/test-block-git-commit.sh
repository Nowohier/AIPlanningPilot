#!/usr/bin/env bash
# test-block-git-commit.sh -- Tests for block-git-commit.sh hook
#
# Tests the PreToolUse hook that blocks git commit commands.
# Covers: various git commit forms, allowed git commands,
#         non-git commands, disabled config, error message quality.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="block-git-commit.sh"

# ============================================================================
printf "${CYAN}${BOLD}block-git-commit.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Blocked commands ---

printf "\n${YELLOW}Blocked git commit commands${RESET}\n"

begin_test "blocks plain git commit"
run_hook "$HOOK" "$(make_bash_input 'git commit')"
assert_exit_code 2 "should hard-block with exit code 2"
assert_stderr_contains "BLOCKED" "should report BLOCKED"

begin_test "blocks git commit with message"
run_hook "$HOOK" "$(make_bash_input 'git commit -m "fix bug"')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks git commit with add+message"
run_hook "$HOOK" "$(make_bash_input 'git commit -am "fix"')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks git commit --amend"
run_hook "$HOOK" "$(make_bash_input 'git commit --amend')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks chained git commit"
run_hook "$HOOK" "$(make_bash_input 'git add . && git commit -m "msg"')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

# --- Allowed commands ---

printf "\n${YELLOW}Allowed commands${RESET}\n"

begin_test "allows git status"
run_hook "$HOOK" "$(make_bash_input 'git status')"
assert_exit_code 0
assert_silent "should allow git status"

begin_test "allows git diff"
run_hook "$HOOK" "$(make_bash_input 'git diff HEAD')"
assert_exit_code 0
assert_silent "should allow git diff"

begin_test "allows git log"
run_hook "$HOOK" "$(make_bash_input 'git log --oneline -5')"
assert_exit_code 0
assert_silent "should allow git log"

begin_test "allows git add"
run_hook "$HOOK" "$(make_bash_input 'git add CLAUDE.md')"
assert_exit_code 0
assert_silent "should allow git add"

begin_test "allows git push"
run_hook "$HOOK" "$(make_bash_input 'git push origin feature/my-branch')"
assert_exit_code 0
assert_silent "should allow git push"

begin_test "allows non-git commands"
run_hook "$HOOK" "$(make_bash_input 'npm run test-all')"
assert_exit_code 0
assert_silent "should allow non-git commands"

begin_test "allows echo mentioning commit"
run_hook "$HOOK" "$(make_bash_input 'echo "commit message"')"
assert_exit_code 0
assert_silent "should allow echo with commit in string"

# --- Disabled config ---

printf "\n${YELLOW}Disabled configuration${RESET}\n"

begin_test "allows git commit when BLOCK_GIT_COMMIT=false"
# Override hooks-config.sh to disable
cat > "${TEST_PLANNING_DIR}/main/hooks-config.sh" << 'EOF'
PROTECTED_PATHS="legacy server"
PROTECTED_PATTERNS="generated"
BLOCK_GIT_COMMIT="false"
EOF
run_hook "$HOOK" "$(make_bash_input 'git commit -m "test"')"
assert_exit_code 0
assert_silent "should allow commit when disabled"

# Restore config for remaining tests
cat > "${TEST_PLANNING_DIR}/main/hooks-config.sh" << 'EOF'
PROTECTED_PATHS="legacy server"
PROTECTED_PATTERNS="generated"
BLOCK_GIT_COMMIT="true"
EOF

# --- Error message quality ---

printf "\n${YELLOW}Error message quality${RESET}\n"

begin_test "block message mentions manual handling"
run_hook "$HOOK" "$(make_bash_input 'git commit')"
assert_exit_code 2
assert_stderr_contains "manually" "should mention developer handles commits manually"

# --- Report ---
report_results "test-block-git-commit.sh"
