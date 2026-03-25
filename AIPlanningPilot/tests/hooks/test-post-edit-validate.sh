#!/usr/bin/env bash
# test-post-edit-validate.sh — Tests for the merged post-edit-validate.sh hook
#
# Tests the merged PostToolUse hook that combines STATE.md + decision record
# validation into a single process with bash-native fast-path extraction.
# Covers: fast-path exit for irrelevant files, STATE.md routing, decision routing,
#         Windows backslash paths in fast-path.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="post-edit-validate.sh"

# ============================================================================
printf "${CYAN}${BOLD}post-edit-validate.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Fast-path exit (irrelevant files — should never invoke Node.js) ---

printf "\n${YELLOW}Fast-path exit (irrelevant files)${RESET}\n"

begin_test "fast-path exits silently for random file"
run_hook "$HOOK" "$(make_tool_input '/some/path/README.md')"
assert_exit_code 0
assert_silent "should be completely silent for irrelevant files"

begin_test "fast-path exits silently for .ts file"
run_hook "$HOOK" "$(make_tool_input '/repo/src/app/component.ts')"
assert_exit_code 0
assert_silent "should be silent for .ts files"

begin_test "fast-path exits silently for STATE.md outside main/"
run_hook "$HOOK" "$(make_tool_input '/some/other/STATE.md')"
assert_exit_code 0
assert_silent "should be silent for STATE.md outside main/"

begin_test "fast-path exits silently for non-decisions numbered file"
run_hook "$HOOK" "$(make_tool_input '/some/path/005-something.md')"
assert_exit_code 0
assert_silent "should be silent for numbered files outside decisions/"

# --- STATE.md routing ---

printf "\n${YELLOW}STATE.md routing${RESET}\n"

begin_test "routes to STATE.md validation for valid file"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0 "should exit 0 (advisory)"

begin_test "STATE.md validation detects missing sections"
prepare_fixture "state-missing-sections.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Decisions Made" "should report missing Decisions Made via merged hook"

begin_test "STATE.md validation detects empty actions"
prepare_fixture "state-empty-actions.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "empty" "should warn about empty actions via merged hook"

begin_test "STATE.md validation detects missing date"
prepare_fixture "state-no-date.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Last Updated" "should warn about missing date via merged hook"

begin_test "STATE.md validation detects missing phase"
prepare_fixture "state-no-phase.md" "${TEST_PLANNING_DIR}/main/STATE.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/main/STATE.md")"
assert_exit_code 0
assert_stderr_contains "Phase" "should warn about missing phase via merged hook"

# --- Decision record routing ---

printf "\n${YELLOW}Decision record routing${RESET}\n"

begin_test "routes to decision validation for numbered decision file"
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test-decision.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-test-decision.md")"
assert_exit_code 0 "should exit 0 (advisory)"

begin_test "decision validation detects missing fields"
cp "${FIXTURES_DIR}/decision-missing-fields.md" "${TEST_PLANNING_DIR}/decisions/005-bad.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/005-bad.md")"
assert_exit_code 0
assert_stderr_contains "Date" "should report missing Date field via merged hook"
assert_stderr_contains "Decision" "should report missing ## Decision section via merged hook"

begin_test "decision validation detects numbering gap"
rm -f "${TEST_PLANNING_DIR}/decisions/"*.md
for i in $(seq 0 4); do
  num=$(printf '%03d' $i)
  cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/${num}-existing.md"
  sed -i "s/Decision 005/Decision ${i}/" "${TEST_PLANNING_DIR}/decisions/${num}-existing.md"
done
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/010-gap.md"
sed -i "s/Decision 005/Decision 10/" "${TEST_PLANNING_DIR}/decisions/010-gap.md"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/010-gap.md")"
assert_exit_code 0
assert_stderr_contains "gap" "should warn about numbering gap via merged hook"

begin_test "ignores INDEX.md in decisions directory"
run_hook "$HOOK" "$(make_tool_input "${TEST_PLANNING_DIR}/decisions/INDEX.md")"
assert_exit_code 0
assert_silent "should ignore INDEX.md (not a numbered decision)"

# --- Windows backslash paths in fast-path ---

printf "\n${YELLOW}Windows path handling${RESET}\n"

begin_test "fast-path handles Windows backslash path for STATE.md"
prepare_fixture "state-valid.md" "${TEST_PLANNING_DIR}/main/STATE.md"
WINPATH=$(echo "${TEST_PLANNING_DIR}/main/STATE.md" | sed 's|/|\\\\|g')
run_hook "$HOOK" "$(make_tool_input "$WINPATH")"
assert_exit_code 0 "should handle backslash paths"

begin_test "fast-path handles Windows backslash path for decisions"
rm -f "${TEST_PLANNING_DIR}/decisions/"*.md
cp "${FIXTURES_DIR}/decision-valid.md" "${TEST_PLANNING_DIR}/decisions/005-test.md"
WINPATH=$(echo "${TEST_PLANNING_DIR}/decisions/005-test.md" | sed 's|/|\\\\|g')
run_hook "$HOOK" "$(make_tool_input "$WINPATH")"
assert_exit_code 0 "should handle backslash paths for decisions"

# --- Report ---
report_results "test-post-edit-validate.sh"
