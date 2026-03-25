#!/usr/bin/env bash
# test-block-literal-paths.sh — Tests for block-literal-paths-in-templates.sh hook
#
# Tests the PreToolUse hook that blocks literal machine paths in template files.
# Covers: path detection, variable pass-through, file filtering, hard block behavior.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="block-literal-paths-in-templates.sh"

# ============================================================================
printf "${CYAN}${BOLD}block-literal-paths-in-templates.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Filtering tests (hook should ignore non-template files) ---

printf "\n${YELLOW}Filtering${RESET}\n"

begin_test "ignores files outside claude-backup/"
run_hook "$HOOK" "$(make_tool_input 'src/main.ts' 'M:\\CODE_COPY\\MyProject\\src')"
assert_exit_code 0
assert_silent "should ignore files not in claude-backup/"

begin_test "ignores command files in .claude/commands/ (runtime, not template)"
run_hook "$HOOK" "$(make_tool_input '.claude/commands/moin.md' 'M:\\CODE_COPY\\MyProject\\src')"
assert_exit_code 0
assert_silent "should ignore runtime command files"

begin_test "allows env.sh in claude-backup/hooks/ (exempt)"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/hooks/env.sh' 'PROJECT_REPO=M:\\CODE_COPY\\project')"
assert_exit_code 0
assert_silent "env.sh should be exempt from path checking"

# --- Blocking behavior ---

printf "\n${YELLOW}Hard block on literal paths${RESET}\n"

begin_test "blocks backslash Windows paths in template commands"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/moin.md' 'Read M:\\CODE_COPY\\MyProject\\file.ts')"
assert_exit_code 2 "should hard-block with exit code 2"
assert_stderr_contains "BLOCKED" "should report BLOCKED"

begin_test "blocks forward-slash Windows paths in template commands"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/ciao.md' 'Read M:/CODE_COPY/planning/file.md')"
assert_exit_code 2 "should block forward-slash paths too"
assert_stderr_contains "BLOCKED"

begin_test "blocks paths in template hooks (not just commands)"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/hooks/some-hook.sh' 'REPO=D:\\Projects\\myapp')"
assert_exit_code 2 "should block in hooks/ directory too"

begin_test "blocks various drive letters"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/test.md' 'path is D:/Work/planning here')"
assert_exit_code 2 "should block any drive letter, not just M:"

# --- Pass-through behavior ---

printf "\n${YELLOW}Allowing variable references${RESET}\n"

begin_test "allows \${PROJECT_REPO} variable references"
run_hook "$HOOK" '{"tool_name":"Write","tool_input":{"file_path":"claude-backup/commands/moin.md","content":"Read ${PROJECT_REPO}/file.ts"}}'
assert_exit_code 0
assert_silent "variable references should pass"

begin_test "allows \${PLANNING_REPO} variable references"
run_hook "$HOOK" '{"tool_name":"Write","tool_input":{"file_path":"claude-backup/commands/ciao.md","content":"Read ${PLANNING_REPO}/main/STATE.md"}}'
assert_exit_code 0
assert_silent "variable references should pass"

begin_test "allows content with no paths at all"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/test.md' 'Just some regular markdown content here')"
assert_exit_code 0
assert_silent "pathless content should pass"

# --- Error message quality ---

printf "\n${YELLOW}Error message quality${RESET}\n"

begin_test "block message mentions the file being written"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/moin.md' 'M:\\CODE\\project')"
assert_exit_code 2
assert_stderr_contains "moin.md" "should mention the specific file"

begin_test "block message suggests using variable references"
run_hook "$HOOK" "$(make_tool_input 'claude-backup/commands/moin.md' 'M:\\CODE\\project')"
assert_stderr_contains "PROJECT_REPO" "should suggest the correct variable"

# --- Report ---
report_results "test-block-literal-paths.sh"
