#!/usr/bin/env bash
# test-validate-decision-record.sh — Tests for validate-decision-record.sh hook
#
# Tests the PostToolUse hook that validates decision record structure and numbering.
# Covers: path filtering, required fields, title/number matching, numbering gaps.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="validate-decision-record.sh"

# ============================================================================
printf "${CYAN}${BOLD}validate-decision-record.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Filtering ---

printf "\n${YELLOW}Filtering${RESET}\n"

begin_test "ignores non-decision files"
run_hook "$HOOK" "$(make_tool_input '/some/path/README.md')"
assert_exit_code 0
assert_silent "should ignore non-decision files"

begin_test "ignores INDEX.md in decisions directory"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/INDEX.md")"
assert_exit_code 0
assert_silent "should ignore INDEX.md (not a numbered decision)"

begin_test "ignores files not in decisions/ directory"
run_hook "$HOOK" "$(make_tool_input '/some/path/005-test.md')"
assert_exit_code 0
assert_silent "should ignore numbered files outside decisions/"

begin_test "triggers for numbered decision files"
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test-decision.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-test-decision.md")"
assert_exit_code 0 "should always exit 0 (advisory)"

# --- Valid decision record ---

printf "\n${YELLOW}Valid decision record${RESET}\n"

begin_test "valid decision record produces no warnings"
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test-decision.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-test-decision.md")"
assert_exit_code 0
assert_stderr_not_contains "Missing" "should have no missing field warnings"
assert_stderr_not_contains "Title" "should have no title warnings"

# --- Missing fields ---

printf "\n${YELLOW}Missing fields${RESET}\n"

begin_test "detects missing Date field"
cp "${FIXTURES_DIR}/decision-missing-fields.md" "${TEST_PLANNING_DIR}/decisions/005-bad.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-bad.md")"
assert_exit_code 0
assert_stderr_contains "Date" "should report missing Date field"

begin_test "detects missing Decision section"
cp "${FIXTURES_DIR}/decision-missing-fields.md" "${TEST_PLANNING_DIR}/decisions/005-bad.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-bad.md")"
assert_stderr_contains "Decision" "should report missing ## Decision section"

begin_test "detects missing Context section"
cp "${FIXTURES_DIR}/decision-missing-fields.md" "${TEST_PLANNING_DIR}/decisions/005-bad.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-bad.md")"
assert_stderr_contains "Context" "should report missing ## Context section"

# --- Title/number mismatch ---

printf "\n${YELLOW}Title/number matching${RESET}\n"

begin_test "warns when title number doesn't match filename"
# File is 005-*.md but content says "Decision 999:"
cp "${FIXTURES_DIR}/decision-numbering-gap.md" "${TEST_PLANNING_DIR}/decisions/005-wrong-title.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-wrong-title.md")"
assert_exit_code 0
assert_stderr_contains "Title" "should warn about title/filename mismatch"

begin_test "no title warning when title matches filename"
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test-decision.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-test-decision.md")"
assert_stderr_not_contains "Title" "should not warn when title matches"

# --- Numbering gaps ---

printf "\n${YELLOW}Numbering gaps${RESET}\n"

begin_test "detects numbering gap"
# Set up existing decisions 000-004 in the test dir
for i in $(seq 0 4); do
  num=$(printf '%03d' $i)
  cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/${num}-existing.md"
  # Fix the title to match the number
  sed -i "s/Decision 005/Decision ${i}/" "${TEST_PLANNING_DIR}/decisions/${num}-existing.md"
done
# Now try to create 010 (skipping 005-009)
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/010-gap.md"
sed -i "s/Decision 005/Decision 10/" "${TEST_PLANNING_DIR}/decisions/010-gap.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/010-gap.md")"
assert_exit_code 0
assert_stderr_contains "gap" "should warn about numbering gap"

begin_test "no gap warning for sequential numbering"
# Clean decisions dir, set up 000-004, test with 005
rm -f "${TEST_PLANNING_DIR}/decisions/"*.md
for i in $(seq 0 4); do
  num=$(printf '%03d' $i)
  cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/${num}-existing.md"
done
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-next.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-next.md")"
assert_stderr_not_contains "gap" "should not warn for sequential numbering"

# --- Windows path handling ---

printf "\n${YELLOW}Path handling${RESET}\n"

begin_test "handles Windows backslash paths in decisions"
rm -f "${TEST_PLANNING_DIR}/decisions/"*.md
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test.md"
WINPATH=$(echo "${TEST_PLANNING_DIR}/decisions/005-test.md" | sed 's|/|\\\\|g')
run_hook "$HOOK" "$(make_tool_input "$WINPATH")"
assert_exit_code 0 "should handle backslash paths"

# --- Report ---
report_results "test-validate-decision-record.sh"
