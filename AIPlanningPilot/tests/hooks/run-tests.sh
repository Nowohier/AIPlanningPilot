#!/usr/bin/env bash
# run-tests.sh — Entry point for all hook tests
#
# Discovers and runs all test-*.sh files in this directory.
# Aggregates results and exits non-zero if any test file failed.
#
# Usage:
#   bash run-tests.sh              # Run all tests
#   bash run-tests.sh <pattern>    # Run tests matching pattern (e.g. "validate-state")

set -euo pipefail

TESTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FILTER="${1:-}"

# --- Colors ---
if [[ -t 1 ]]; then
  RED='\033[0;31m'
  GREEN='\033[0;32m'
  CYAN='\033[0;36m'
  BOLD='\033[1m'
  RESET='\033[0m'
else
  RED=''; GREEN=''; CYAN=''; BOLD=''; RESET=''
fi

# --- Discover test files ---
TEST_FILES=()
for f in "${TESTS_DIR}"/test-*.sh; do
  [[ -f "$f" ]] || continue
  # Skip the helper (it's sourced, not run directly)
  [[ "$(basename "$f")" == "test-helper.sh" ]] && continue
  if [[ -n "$FILTER" ]]; then
    basename "$f" | grep -q "$FILTER" || continue
  fi
  TEST_FILES+=("$f")
done

if [[ ${#TEST_FILES[@]} -eq 0 ]]; then
  echo "No test files found${FILTER:+ matching '$FILTER'}."
  exit 1
fi

# --- Header ---
echo ""
printf "${CYAN}${BOLD}╔══════════════════════════════════════════════╗${RESET}\n"
printf "${CYAN}${BOLD}║           Hook Test Suite                    ║${RESET}\n"
printf "${CYAN}${BOLD}╚══════════════════════════════════════════════╝${RESET}\n"
echo ""
printf "Running ${BOLD}%d${RESET} test file(s)...\n" "${#TEST_FILES[@]}"
echo ""

# --- Run each test file ---
TOTAL_FILES=0
PASSED_FILES=0
FAILED_FILES=0
FAILED_NAMES=()

for test_file in "${TEST_FILES[@]}"; do
  test_name=$(basename "$test_file")
  printf "${CYAN}━━━ %s ━━━${RESET}\n" "$test_name"

  set +e
  bash "$test_file"
  exit_code=$?
  set -e

  ((TOTAL_FILES++)) || true
  if [[ $exit_code -eq 0 ]]; then
    ((PASSED_FILES++)) || true
  else
    ((FAILED_FILES++)) || true
    FAILED_NAMES+=("$test_name")
  fi
  echo ""
done

# --- Summary ---
printf "${CYAN}${BOLD}══════════════════════════════════════════════${RESET}\n"
printf "${BOLD}Summary:${RESET} %d file(s) run\n" "$TOTAL_FILES"
echo ""

if [[ $FAILED_FILES -eq 0 ]]; then
  printf "  ${GREEN}${BOLD}✅ All %d test file(s) passed${RESET}\n" "$PASSED_FILES"
else
  printf "  ${GREEN}✅ %d passed${RESET}\n" "$PASSED_FILES"
  printf "  ${RED}❌ %d failed:${RESET}\n" "$FAILED_FILES"
  for name in "${FAILED_NAMES[@]}"; do
    printf "     ${RED}→ %s${RESET}\n" "$name"
  done
fi

printf "${CYAN}${BOLD}══════════════════════════════════════════════${RESET}\n"
echo ""

exit $FAILED_FILES
