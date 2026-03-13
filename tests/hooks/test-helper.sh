#!/usr/bin/env bash
# test-helper.sh — Shared test infrastructure for hook tests
#
# Provides:
#   - Assertion functions (assert_exit, assert_stderr_contains, etc.)
#   - Test environment setup/teardown (temp dirs, fixture env.sh)
#   - Test runner (run_test, report_results)
#   - Fixture file preparation (date replacement)
#
# Usage (from a test file):
#   source "$(dirname "$0")/test-helper.sh"
#   setup_test_env
#   ... define tests ...
#   teardown_test_env
#   report_results

set -euo pipefail

# --- State ---
TESTS_PASS=0
TESTS_FAIL=0
TESTS_TOTAL=0
CURRENT_TEST=""

# --- Paths ---
TESTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FIXTURES_DIR="${TESTS_DIR}/fixtures"
HOOKS_DIR="$(cd "${TESTS_DIR}/../../claude-backup/hooks" && pwd)"

# --- Colors (if terminal supports them) ---
if [[ -t 1 ]]; then
  RED='\033[0;31m'
  GREEN='\033[0;32m'
  YELLOW='\033[0;33m'
  CYAN='\033[0;36m'
  BOLD='\033[1m'
  RESET='\033[0m'
else
  RED=''; GREEN=''; YELLOW=''; CYAN=''; BOLD=''; RESET=''
fi

# --- Test environment ---

# Creates a temporary directory structure that mimics the planning repo,
# generates a test env.sh pointing to it, and prepares hook copies.
setup_test_env() {
  TEST_TMPDIR=$(mktemp -d)
  TEST_HOOKS_DIR="${TEST_TMPDIR}/hooks"
  TEST_REPO_DIR="${TEST_TMPDIR}/planning"
  TEST_PLANNING_DIR="${TEST_REPO_DIR}"

  # Create directory structure
  mkdir -p "${TEST_HOOKS_DIR}"
  mkdir -p "${TEST_PLANNING_DIR}/main"
  mkdir -p "${TEST_PLANNING_DIR}/handovers"
  mkdir -p "${TEST_PLANNING_DIR}/decisions"
  mkdir -p "${TEST_PLANNING_DIR}/claude-backup/commands"

  # Copy all hook scripts to temp location
  cp "${HOOKS_DIR}"/*.sh "${TEST_HOOKS_DIR}/"

  # Generate test env.sh with paths pointing to temp dirs
  cat > "${TEST_HOOKS_DIR}/env.sh" << ENVEOF
#!/usr/bin/env bash
PROJECT_REPO="${TEST_TMPDIR}/project"
PLANNING_REPO="${TEST_REPO_DIR}"
DEVELOPER="TestDev"

PLANNING_DIR="\${PLANNING_REPO}"
STATE_FILE="\${PLANNING_DIR}/main/STATE.md"
HANDOVER_DIR="\${PLANNING_DIR}/handovers"
DECISIONS_DIR="\${PLANNING_DIR}/decisions"
BACKUP_COMMANDS_DIR="\${PLANNING_DIR}/claude-backup/commands"

require_planning_repo() {
  if [[ -z "\$PLANNING_REPO" || "\$PLANNING_REPO" == '\${PLANNING_REPO}' ]]; then
    echo "Hook skipped: env.sh not configured. Run /onboard." >&2
    return 1
  fi
  if [[ ! -d "\$PLANNING_REPO" ]]; then
    echo "Hook skipped: PLANNING_REPO not found at \$PLANNING_REPO" >&2
    return 1
  fi
  return 0
}
ENVEOF
}

teardown_test_env() {
  if [[ -n "${TEST_TMPDIR:-}" && -d "${TEST_TMPDIR}" ]]; then
    rm -rf "${TEST_TMPDIR}"
  fi
}

# Trap to ensure cleanup on exit
trap teardown_test_env EXIT

# --- Fixture helpers ---

# Prepare a fixture file: copies it to a target location,
# replacing %%TODAY%% with today's date.
prepare_fixture() {
  local fixture_name="$1"
  local target_path="$2"
  local today
  today=$(date +%Y-%m-%d)

  mkdir -p "$(dirname "$target_path")"
  sed "s/%%TODAY%%/${today}/g" "${FIXTURES_DIR}/${fixture_name}" > "$target_path"
}

# Prepare a fixture with a custom date (for testing stale dates)
prepare_fixture_with_date() {
  local fixture_name="$1"
  local target_path="$2"
  local custom_date="$3"

  mkdir -p "$(dirname "$target_path")"
  sed "s/%%TODAY%%/${custom_date}/g" "${FIXTURES_DIR}/${fixture_name}" > "$target_path"
}

# --- JSON helpers ---

# Build a tool input JSON for Write/Edit hooks
make_tool_input() {
  local file_path="$1"
  local content="${2:-}"
  if [[ -n "$content" ]]; then
    node -e 'console.log(JSON.stringify({tool_name:"Write",tool_input:{file_path:process.argv[1],content:process.argv[2]}}))' "$file_path" "$content"
  else
    node -e 'console.log(JSON.stringify({tool_name:"Write",tool_input:{file_path:process.argv[1]}}))' "$file_path"
  fi
}

# Build a prompt input JSON for UserPromptSubmit hooks
make_prompt_input() {
  local prompt="$1"
  node -e 'console.log(JSON.stringify({prompt:process.argv[1]}))' "$prompt"
}

# --- Run a hook ---

# Runs a hook script with given stdin, captures stdout, stderr, and exit code.
# Sets: HOOK_EXIT, HOOK_STDOUT, HOOK_STDERR
run_hook() {
  local hook_name="$1"
  local stdin_data="$2"
  local hook_path="${TEST_HOOKS_DIR}/${hook_name}"

  # Temp files for capturing streams
  local stdout_file stderr_file
  stdout_file=$(mktemp)
  stderr_file=$(mktemp)

  set +e
  echo "$stdin_data" | bash "$hook_path" >"$stdout_file" 2>"$stderr_file"
  HOOK_EXIT=$?
  set -e

  HOOK_STDOUT=$(cat "$stdout_file")
  HOOK_STDERR=$(cat "$stderr_file")
  rm -f "$stdout_file" "$stderr_file"
}

# --- Assertions ---

assert_exit_code() {
  local expected="$1"
  local message="${2:-exit code should be $expected}"
  if [[ "$HOOK_EXIT" -eq "$expected" ]]; then
    _pass "$message"
  else
    _fail "$message" "expected exit $expected, got $HOOK_EXIT"
  fi
}

assert_stderr_contains() {
  local pattern="$1"
  local message="${2:-stderr should contain '$pattern'}"
  if echo "$HOOK_STDERR" | grep -q "$pattern"; then
    _pass "$message"
  else
    _fail "$message" "stderr did not contain '$pattern'"
  fi
}

assert_stderr_not_contains() {
  local pattern="$1"
  local message="${2:-stderr should not contain '$pattern'}"
  if echo "$HOOK_STDERR" | grep -q "$pattern"; then
    _fail "$message" "stderr contained '$pattern' but should not"
  else
    _pass "$message"
  fi
}

assert_stdout_contains() {
  local pattern="$1"
  local message="${2:-stdout should contain '$pattern'}"
  if echo "$HOOK_STDOUT" | grep -q "$pattern"; then
    _pass "$message"
  else
    _fail "$message" "stdout did not contain '$pattern'"
  fi
}

assert_stdout_empty() {
  local message="${1:-stdout should be empty}"
  if [[ -z "$HOOK_STDOUT" ]]; then
    _pass "$message"
  else
    _fail "$message" "stdout was: '$HOOK_STDOUT'"
  fi
}

assert_stderr_empty() {
  local message="${1:-stderr should be empty}"
  if [[ -z "$HOOK_STDERR" ]]; then
    _pass "$message"
  else
    _fail "$message" "stderr was: '$HOOK_STDERR'"
  fi
}

assert_silent() {
  local message="${1:-hook should be silent (no stdout, no stderr)}"
  if [[ -z "$HOOK_STDOUT" && -z "$HOOK_STDERR" ]]; then
    _pass "$message"
  else
    local detail=""
    [[ -n "$HOOK_STDOUT" ]] && detail="stdout='${HOOK_STDOUT}'"
    [[ -n "$HOOK_STDERR" ]] && detail="${detail:+$detail, }stderr='${HOOK_STDERR}'"
    _fail "$message" "$detail"
  fi
}

# --- Test structure ---

begin_test() {
  CURRENT_TEST="$1"
  ((TESTS_TOTAL++)) || true
}

# --- Internal ---

_pass() {
  local message="$1"
  ((TESTS_PASS++)) || true
  printf "  ${GREEN}✅ %s${RESET}\n" "$message"
}

_fail() {
  local message="$1"
  local detail="${2:-}"
  ((TESTS_FAIL++)) || true
  printf "  ${RED}❌ %s${RESET}\n" "$message"
  if [[ -n "$detail" ]]; then
    printf "     ${RED}→ %s${RESET}\n" "$detail"
  fi
}

# --- Reporting ---

report_results() {
  local test_file_name="${1:-tests}"
  echo ""
  if [[ $TESTS_FAIL -eq 0 ]]; then
    printf "${GREEN}${BOLD}All %d assertions passed${RESET} in %s\n" "$TESTS_PASS" "$test_file_name"
  else
    printf "${RED}${BOLD}%d of %d assertions FAILED${RESET} in %s\n" "$TESTS_FAIL" "$((TESTS_PASS + TESTS_FAIL))" "$test_file_name"
  fi
  echo ""
  return $TESTS_FAIL
}
