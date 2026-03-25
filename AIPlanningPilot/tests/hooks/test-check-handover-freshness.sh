#!/usr/bin/env bash
# test-check-handover-freshness.sh — Tests for check-handover-freshness.sh hook
#
# Tests the UserPromptSubmit hook that warns about stale handover files on /moin.
# Covers: prompt filtering, fresh/stale detection, developer name derivation,
#         missing file handling, UNCONFIGURED developer.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

HOOK="check-handover-freshness.sh"

# ============================================================================
printf "${CYAN}${BOLD}check-handover-freshness.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Prompt filtering ---

printf "\n${YELLOW}Prompt filtering${RESET}\n"

begin_test "ignores non-/moin prompts"
run_hook "$HOOK" "$(make_prompt_input 'help me with this code')"
assert_exit_code 0
assert_silent "should not trigger for non-/moin prompts"

begin_test "ignores empty prompt"
run_hook "$HOOK" "$(make_prompt_input '')"
assert_exit_code 0
assert_silent "should not trigger for empty prompt"

begin_test "triggers on /moin prompt"
prepare_fixture "handover-fresh.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0 "should always exit 0 (context injection, not blocking)"

begin_test "triggers on /moin with extra text"
prepare_fixture "handover-fresh.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin please')"
assert_exit_code 0

# --- Fresh handover ---

printf "\n${YELLOW}Fresh handover${RESET}\n"

begin_test "no warning for fresh handover (today)"
prepare_fixture "handover-fresh.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0
assert_stdout_empty "should not inject warning for fresh handover"

# --- Stale handover ---

printf "\n${YELLOW}Stale handover${RESET}\n"

begin_test "warns for stale handover (months old)"
prepare_fixture "handover-stale.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0
assert_stdout_contains "STALE" "should inject stale warning on stdout"
assert_stdout_contains "TestDev" "should mention the developer name"

begin_test "stale warning mentions the date"
prepare_fixture "handover-stale.md" "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_stdout_contains "2025-01-01" "should show the last updated date"

# --- Missing handover file ---

printf "\n${YELLOW}Missing handover${RESET}\n"

begin_test "silent when handover file does not exist"
rm -f "${TEST_PLANNING_DIR}/handovers/handover-testdev.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0
assert_stdout_empty "should not warn when file is missing (/moin handles this)"

# --- UNCONFIGURED developer ---

printf "\n${YELLOW}Unconfigured developer${RESET}\n"

begin_test "silent when developer is UNCONFIGURED"
# Override env.sh to set DEVELOPER=UNCONFIGURED
cat > "${TEST_HOOKS_DIR}/env.sh" << 'EOF'
PLANNING_REPO="__PLACEHOLDER__"
DEVELOPER="UNCONFIGURED"
HANDOVER_DIR="${PLANNING_REPO}/handovers"
require_planning_repo() { return 0; }
EOF
sed -i "s|__PLACEHOLDER__|${TEST_REPO_DIR}|" "${TEST_HOOKS_DIR}/env.sh"

run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0
assert_stdout_empty "should not check handover for unconfigured developer"

# Restore normal test env.sh
setup_test_env

# --- Developer name case handling ---

printf "\n${YELLOW}Developer name case handling${RESET}\n"

begin_test "derives lowercase filename from mixed-case developer name"
# Override env.sh with mixed-case developer
cat > "${TEST_HOOKS_DIR}/env.sh" << 'EOF'
PLANNING_REPO="__PLACEHOLDER__"
DEVELOPER="ChRiS"
PLANNING_DIR="${PLANNING_REPO}"
HANDOVER_DIR="${PLANNING_DIR}/handovers"
require_planning_repo() { return 0; }
EOF
sed -i "s|__PLACEHOLDER__|${TEST_REPO_DIR}|" "${TEST_HOOKS_DIR}/env.sh"

prepare_fixture "handover-stale.md" "${TEST_PLANNING_DIR}/handovers/handover-chris.md"
run_hook "$HOOK" "$(make_prompt_input '/moin')"
assert_exit_code 0
assert_stdout_contains "STALE" "should find handover-chris.md from developer 'ChRiS'"

# --- Report ---
report_results "test-check-handover-freshness.sh"
