#!/usr/bin/env bash
# test-validate-handover.sh — Tests for validate-handover.sh hook
#
# Tests the PostToolUse hook that validates handover file structure after /ciao writes it.
# Covers: path filtering, required sections, metadata, empty sections, path handling.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="validate-handover.sh"

# ============================================================================
printf "${CYAN}${BOLD}validate-handover.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Filtering tests (hook should ignore non-matching files) ---

printf "\n${YELLOW}Filtering${RESET}\n"

begin_test "ignores non-handover files"
run_hook "$HOOK" "$(make_tool_input '/some/path/README.md')"
assert_exit_code 0
assert_silent "should be silent for non-handover files"

begin_test "ignores files not in handovers/ directory"
run_hook "$HOOK" "$(make_tool_input '/some/path/handover-chris.md')"
assert_exit_code 0
assert_silent "should be silent for handover outside handovers/"

begin_test "triggers for correct handovers/handover-*.md path"
prepare_fixture "handover-valid.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_exit_code 0 "should exit 0 (advisory, never blocks)"

# --- Valid handover ---

printf "\n${YELLOW}Valid handover${RESET}\n"

begin_test "valid handover produces no errors"
prepare_fixture "handover-valid.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_exit_code 0
assert_stderr_not_contains "Missing required section" "should have no missing section errors"
assert_stderr_not_contains "ERRORS" "should report no errors"

begin_test "valid handover produces no warnings"
prepare_fixture "handover-valid.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_stderr_not_contains "WARNINGS" "should report no warnings"

# --- Missing sections ---

printf "\n${YELLOW}Missing sections${RESET}\n"

begin_test "detects missing Decisions & Findings section"
prepare_fixture "handover-missing-sections.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_exit_code 0 "should still exit 0 (advisory)"
assert_stderr_contains "Decisions & Findings" "should report missing Decisions & Findings"

begin_test "detects missing From Last Session section"
prepare_fixture "handover-missing-sections.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_stderr_contains "From Last Session" "should report missing From Last Session"

# --- Missing metadata ---

printf "\n${YELLOW}Missing metadata${RESET}\n"

begin_test "detects missing H1 title"
prepare_fixture "handover-no-metadata.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_exit_code 0
assert_stderr_contains "Missing H1 title" "should warn about missing title"

begin_test "detects missing Last updated blockquote"
prepare_fixture "handover-no-metadata.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_stderr_contains "Last updated" "should report missing Last updated"

# --- Empty sections ---

printf "\n${YELLOW}Empty sections${RESET}\n"

begin_test "warns when For Next Session is empty"
prepare_fixture "handover-empty-sections.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_exit_code 0
assert_stderr_contains "For Next Session" "should warn about empty For Next Session"

begin_test "warns when From Last Session is empty"
prepare_fixture "handover-empty-sections.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/handovers/handover-testdev.md")"
assert_stderr_contains "From Last Session" "should warn about empty From Last Session"

# --- Windows path handling ---

printf "\n${YELLOW}Path handling${RESET}\n"

begin_test "handles Windows backslash paths"
prepare_fixture "handover-valid.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
WINPATH=$(echo "${TEST_PLANNING_DIR}/handovers/handover-testdev.md" | sed 's|/|\\\\|g')
run_hook "$HOOK" "$(make_tool_input "$WINPATH")"
assert_exit_code 0 "should handle backslash paths without crashing"

# --- Report ---
report_results "test-validate-handover.sh"
