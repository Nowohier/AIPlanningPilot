#!/usr/bin/env bash
# test-block-protected-paths.sh -- Tests for block-protected-paths.sh hook
#
# Tests the PreToolUse hook that blocks Edit/Write to protected directories.
# Covers: PROTECTED_PATHS matching, PROTECTED_PATTERNS matching (case-insensitive),
#         path normalization (backslash/forward-slash), unconfigured fallback,
#         error message quality.
#
# Default test config: PROTECTED_PATHS="legacy server", PROTECTED_PATTERNS="generated"

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="block-protected-paths.sh"

# ============================================================================
printf "${CYAN}${BOLD}block-protected-paths.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Allowed paths (should pass silently) ---

printf "\n${YELLOW}Allowed paths${RESET}\n"

begin_test "allows frontend files"
run_hook "$HOOK" "$(make_tool_input 'frontend/libs/components/src/component.ts')"
assert_exit_code 0
assert_silent "should allow frontend files"

begin_test "allows root files"
run_hook "$HOOK" "$(make_tool_input 'CLAUDE.md')"
assert_exit_code 0
assert_silent "should allow root-level files"

begin_test "allows planning files"
run_hook "$HOOK" "$(make_tool_input 'restructuring/planning/main/STATE.md')"
assert_exit_code 0
assert_silent "should allow planning repo files"

begin_test "allows non-protected directories"
run_hook "$HOOK" "$(make_tool_input 'frontend/libs/shared/src/lib/manual/component.ts')"
assert_exit_code 0
assert_silent "should allow non-protected directories"

begin_test "allows .claude files"
run_hook "$HOOK" "$(make_tool_input '.claude/hooks/env.sh')"
assert_exit_code 0
assert_silent "should allow .claude files"

begin_test "allows files in unrelated directories"
run_hook "$HOOK" "$(make_tool_input 'docs/architecture/overview.md')"
assert_exit_code 0
assert_silent "should allow unrelated directories"

# --- PROTECTED_PATHS: first dir blocks ---

printf "\n${YELLOW}PROTECTED_PATHS: legacy/ blocks${RESET}\n"

begin_test "blocks protected dir root file"
run_hook "$HOOK" "$(make_tool_input 'legacy/SomeFile.cs')"
assert_exit_code 2 "should hard-block with exit code 2"
assert_stderr_contains "BLOCKED" "should report BLOCKED"

begin_test "blocks protected dir subfolder"
run_hook "$HOOK" "$(make_tool_input 'legacy/Tools/Editor/Models/Entity.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks absolute Windows path in protected dir"
run_hook "$HOOK" "$(make_tool_input 'D:\\Projects\\MyApp\\legacy\\file.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

# --- PROTECTED_PATHS: second dir blocks ---

printf "\n${YELLOW}PROTECTED_PATHS: server/ blocks${RESET}\n"

begin_test "blocks second protected dir root file"
run_hook "$HOOK" "$(make_tool_input 'server/MyApp.sln')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks second protected dir subfolder"
run_hook "$HOOK" "$(make_tool_input 'server/Api/Controllers/MyController.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks absolute Windows path in second protected dir"
run_hook "$HOOK" "$(make_tool_input 'D:\\Projects\\MyApp\\server\\file.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks forward-slash absolute path in protected dir"
run_hook "$HOOK" "$(make_tool_input 'D:/Projects/MyApp/server/file.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

# --- Generated/ blocks (case-insensitive pattern) ---

printf "\n${YELLOW}PROTECTED_PATTERNS: generated/ blocks${RESET}\n"

begin_test "blocks generated directory (lowercase)"
run_hook "$HOOK" "$(make_tool_input 'frontend/libs/domain/src/lib/generated/component.ts')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks generated directory in second protected dir"
run_hook "$HOOK" "$(make_tool_input 'server/Api.Financial/Generated/SomeFile.cs')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

begin_test "blocks case-insensitive Generated"
run_hook "$HOOK" "$(make_tool_input 'frontend/libs/domain/src/lib/Generated/File.ts')"
assert_exit_code 2
assert_stderr_contains "BLOCKED"

# --- Unconfigured (empty config) ---

printf "\n${YELLOW}Unconfigured project${RESET}\n"

begin_test "allows everything when config is empty"
# Override hooks-config.sh with empty values
cat > "${TEST_PLANNING_DIR}/main/hooks-config.sh" << 'EOF'
PROTECTED_PATHS=""
PROTECTED_PATTERNS=""
EOF
run_hook "$HOOK" "$(make_tool_input 'server/Api/SomeFile.cs')"
assert_exit_code 0
assert_silent "should allow everything when unconfigured"

# Restore config for remaining tests
cat > "${TEST_PLANNING_DIR}/main/hooks-config.sh" << 'EOF'
PROTECTED_PATHS="legacy server"
PROTECTED_PATTERNS="generated"
BLOCK_GIT_COMMIT="true"
EOF

# --- Error message quality ---

printf "\n${YELLOW}Error message quality${RESET}\n"

begin_test "block message mentions the file being edited"
run_hook "$HOOK" "$(make_tool_input 'legacy/Models/Entity.cs')"
assert_exit_code 2
assert_stderr_contains "Entity.cs" "should mention the specific file"

begin_test "block message identifies matched rule"
run_hook "$HOOK" "$(make_tool_input 'legacy/SomeFile.cs')"
assert_stderr_contains "legacy" "should identify the matched rule"

begin_test "block message identifies generated pattern"
run_hook "$HOOK" "$(make_tool_input 'frontend/libs/domain/src/lib/generated/component.ts')"
assert_stderr_contains "generated" "should identify the generated pattern"

# --- Report ---
report_results "test-block-protected-paths.sh"
