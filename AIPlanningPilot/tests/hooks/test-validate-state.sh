#!/usr/bin/env bash
# test-validate-state.sh — Tests for validate-state.sh hook
#
# Tests the PostToolUse hook that validates STATE.md schema after /ciao writes it.
# Covers: section detection, phase header, action items, date validation, path filtering.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="validate-state.sh"

# ============================================================================
printf "${CYAN}${BOLD}validate-state.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Filtering tests (hook should ignore non-matching files) ---

printf "\n${YELLOW}Filtering${RESET}\n"

begin_test "ignores non-STATE.md files"
run_hook "$HOOK" "$(make_tool_input '/some/path/README.md')"
assert_exit_code 0
assert_silent "should be silent for non-STATE files"

begin_test "ignores STATE.md outside main/"
run_hook "$HOOK" "$(make_tool_input '/some/path/STATE.md')"
assert_exit_code 0
assert_silent "should be silent for STATE.md in wrong location"

begin_test "ignores STATE.md in wrong subdirectory"
run_hook "$HOOK" "$(make_tool_input '/repo/other/STATE.md')"
assert_exit_code 0
assert_silent "should be silent for STATE.md not in main/"

begin_test "triggers for correct main/STATE.md path"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0 "should exit 0 (advisory, never blocks)"

# --- Valid STATE.md ---

printf "\n${YELLOW}Valid STATE.md${RESET}\n"

begin_test "valid STATE.md produces no errors"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_not_contains "Missing required section" "should have no missing section errors"
assert_stderr_not_contains "ERRORS" "should report no errors"

# --- Missing sections ---

printf "\n${YELLOW}Missing sections${RESET}\n"

begin_test "detects missing Decisions Made section"
prepare_fixture "state-missing-sections.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0 "should still exit 0 (advisory)"
assert_stderr_contains "Decisions Made" "should report missing Decisions Made"

begin_test "detects missing Open Decisions section"
prepare_fixture "state-missing-sections.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_stderr_contains "Open Decisions" "should report missing Open Decisions"

# --- Empty actions ---

printf "\n${YELLOW}Empty actions${RESET}\n"

begin_test "warns when Next Actions has no items"
prepare_fixture "state-empty-actions.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "empty" "should warn about empty actions"

# --- Date validation ---

printf "\n${YELLOW}Date validation${RESET}\n"

begin_test "warns when Last Updated date is missing"
prepare_fixture "state-no-date.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Last Updated" "should warn about missing date"

begin_test "warns when Last Updated date is stale (>3 days)"
prepare_fixture "state-stale-date.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "days ago" "should warn about stale date"

begin_test "no date warning when date is today"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_stderr_not_contains "days ago" "should not warn about today's date"

# --- Phase header ---

printf "\n${YELLOW}Phase header${RESET}\n"

begin_test "warns when Phase header line is missing"
prepare_fixture "state-no-phase.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Phase" "should warn about missing phase"

# --- Table structure: valid 3-column ---

printf "\n${YELLOW}Table structure (valid 3-column)${RESET}\n"

begin_test "valid 3-column Phase Progress produces no column warnings"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_not_contains "expected 3-4 columns" "should not warn about Phase Progress columns"
assert_stderr_not_contains "expected at least 4 columns" "should not warn about Next Actions columns"
assert_stderr_not_contains "expected 3 columns" "should not warn about Open Decisions columns"
assert_stderr_not_contains "unknown status" "should not warn about valid statuses"

# --- Table structure: valid 4-column (PlanFile) ---

printf "\n${YELLOW}Table structure (valid 4-column PlanFile)${RESET}\n"

begin_test "valid 4-column Phase Progress (with PlanFile) produces no column warnings"
prepare_fixture "state-valid-4col.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_not_contains "expected 3-4 columns" "should not warn about 4-column Phase Progress"
assert_stderr_not_contains "unknown status" "should not warn about valid statuses (Done, Day 1 done, Not started)"

# --- PlanFile path resolution ---

printf "\n${YELLOW}PlanFile path validation${RESET}\n"

begin_test "no PlanFile warning when referenced files exist"
prepare_fixture "state-valid-4col.md" "${TEST_PLANNING_DIR}/main/STATE.md"
# Create the plan files where the hook should look (relative to PLANNING_DIR, NOT main/)
mkdir -p "${TEST_PLANNING_DIR}/plan"
echo "# Overview" > "${TEST_PLANNING_DIR}/plan/overview.md"
echo "# Phase 1" > "${TEST_PLANNING_DIR}/plan/phase-1.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_not_contains "PlanFile.*not found" "should not warn when plan files exist at correct paths"

begin_test "warns when PlanFile references non-existent files"
prepare_fixture "state-valid-4col.md" "${TEST_PLANNING_DIR}/main/STATE.md"
# Remove the plan files so they don't exist
rm -rf "${TEST_PLANNING_DIR}/plan"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "PlanFile.*overview.md.*not found" "should warn about missing plan/overview.md"
assert_stderr_contains "PlanFile.*phase-1.md.*not found" "should warn about missing plan/phase-1.md"

begin_test "no PlanFile warning when PlanFile column is empty"
prepare_fixture "state-valid-4col.md" "${TEST_PLANNING_DIR}/main/STATE.md"
# Phase 2 has empty PlanFile -- should not produce a warning
rm -rf "${TEST_PLANNING_DIR}/plan"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
# Should warn about overview.md and phase-1.md but NOT about empty PlanFile for Phase 2
assert_stderr_not_contains "Phase Progress row 3.*PlanFile" "should not warn about empty PlanFile in row 3"

# --- Table structure: bad column counts ---

printf "\n${YELLOW}Table structure (bad column counts)${RESET}\n"

begin_test "warns about insufficient columns in all tables"
prepare_fixture "state-bad-columns.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Phase Progress row.*expected 3-4 columns" "should warn about Phase Progress columns"
assert_stderr_contains "Next Actions row.*expected at least 4 columns" "should warn about Next Actions columns"
assert_stderr_contains "Open Decisions row.*expected 3 columns" "should warn about Open Decisions columns"

# --- Table structure: unknown status values ---

printf "\n${YELLOW}Unknown status values${RESET}\n"

begin_test "warns about unknown Phase Progress status values"
prepare_fixture "state-unknown-status.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "unknown status.*WIP" "should warn about 'WIP' status"
assert_stderr_contains "unknown status.*Almost there" "should warn about 'Almost there' status"

# --- Windows backslash paths ---

printf "\n${YELLOW}Path handling${RESET}\n"

begin_test "handles Windows backslash paths"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
WINPATH=$(echo "${TEST_PLANNING_DIR}/main/STATE.md" | sed 's|/|\\\\|g')
run_hook "$HOOK" "$(make_tool_input "$WINPATH")"
assert_exit_code 0 "should handle backslash paths without crashing"

# --- Report ---
report_results "test-validate-state.sh"
