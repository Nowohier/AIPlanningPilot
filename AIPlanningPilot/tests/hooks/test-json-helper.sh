#!/usr/bin/env bash
# test-json-helper.sh — Tests for json-helper.sh shared helper
#
# Tests the json_get() function for robust JSON field extraction.
# Covers: simple strings, escaped quotes, newlines, missing fields,
# invalid JSON, flat structures, Windows paths.

source "$(dirname "$0")/test-helper.sh"
setup_test_env

# Source json-helper.sh from the test hooks directory (copied during setup)
source "${TEST_HOOKS_DIR}/json-helper.sh"

# ============================================================================
printf "${CYAN}${BOLD}json-helper.sh${RESET}\n"
printf "${CYAN}──────────────────────────────────────${RESET}\n"
# ============================================================================

# --- Simple string extraction ---

printf "\n${YELLOW}Simple extraction${RESET}\n"

begin_test "extracts tool_input.file_path"
RESULT=$(echo '{"tool_name":"Write","tool_input":{"file_path":"/some/path/STATE.md"}}' | json_get "tool_input.file_path")
if [[ "$RESULT" == "/some/path/STATE.md" ]]; then
  _pass "tool_input.file_path extracted correctly"
else
  _fail "tool_input.file_path extraction" "expected '/some/path/STATE.md', got '$RESULT'"
fi

begin_test "extracts nested tool_input.content"
RESULT=$(echo '{"tool_name":"Write","tool_input":{"file_path":"/f.md","content":"hello world"}}' | json_get "tool_input.content")
if [[ "$RESULT" == "hello world" ]]; then
  _pass "tool_input.content extracted correctly"
else
  _fail "tool_input.content extraction" "expected 'hello world', got '$RESULT'"
fi

# --- Content with escaped quotes ---

printf "\n${YELLOW}Escaped quotes${RESET}\n"

begin_test "handles content with escaped quotes"
RESULT=$(echo '{"tool_input":{"content":"say \"hello\" here"}}' | json_get "tool_input.content")
if [[ "$RESULT" == 'say "hello" here' ]]; then
  _pass "escaped quotes handled correctly"
else
  _fail "escaped quotes handling" "expected 'say \"hello\" here', got '$RESULT'"
fi

# --- Content with newlines ---

printf "\n${YELLOW}Newlines in values${RESET}\n"

begin_test "handles content with newlines"
RESULT=$(echo '{"tool_input":{"content":"line1\nline2\nline3"}}' | json_get "tool_input.content")
EXPECTED=$'line1\nline2\nline3'
if [[ "$RESULT" == "$EXPECTED" ]]; then
  _pass "newlines in content handled correctly"
else
  _fail "newline handling" "expected multiline content, got '$RESULT'"
fi

# --- Missing field ---

printf "\n${YELLOW}Missing fields${RESET}\n"

begin_test "missing field returns empty"
RESULT=$(echo '{"tool_input":{"file_path":"/f.md"}}' | json_get "tool_input.content")
if [[ -z "$RESULT" ]]; then
  _pass "missing field returns empty string"
else
  _fail "missing field" "expected empty, got '$RESULT'"
fi

begin_test "missing nested path returns empty"
RESULT=$(echo '{"tool_input":{"file_path":"/f.md"}}' | json_get "nonexistent.deep.path")
if [[ -z "$RESULT" ]]; then
  _pass "missing nested path returns empty string"
else
  _fail "missing nested path" "expected empty, got '$RESULT'"
fi

# --- Invalid JSON ---

printf "\n${YELLOW}Invalid JSON${RESET}\n"

begin_test "invalid JSON returns empty"
RESULT=$(echo 'not valid json at all' | json_get "tool_input.file_path")
if [[ -z "$RESULT" ]]; then
  _pass "invalid JSON returns empty string"
else
  _fail "invalid JSON handling" "expected empty, got '$RESULT'"
fi

begin_test "empty input returns empty"
RESULT=$(echo '' | json_get "tool_input.file_path")
if [[ -z "$RESULT" ]]; then
  _pass "empty input returns empty string"
else
  _fail "empty input handling" "expected empty, got '$RESULT'"
fi

# --- Flat structure (UserPromptSubmit) ---

printf "\n${YELLOW}Flat structure${RESET}\n"

begin_test "extracts prompt from flat structure"
RESULT=$(echo '{"prompt":"/moin"}' | json_get "prompt")
if [[ "$RESULT" == "/moin" ]]; then
  _pass "flat prompt field extracted correctly"
else
  _fail "flat prompt extraction" "expected '/moin', got '$RESULT'"
fi

begin_test "extracts prompt with spaces"
RESULT=$(echo '{"prompt":"/moin some extra text"}' | json_get "prompt")
if [[ "$RESULT" == "/moin some extra text" ]]; then
  _pass "prompt with spaces extracted correctly"
else
  _fail "prompt with spaces" "expected '/moin some extra text', got '$RESULT'"
fi

# --- Windows backslash paths ---

printf "\n${YELLOW}Windows paths${RESET}\n"

begin_test "handles Windows backslash paths in values"
RESULT=$(echo '{"tool_input":{"file_path":"M:\\CODE_COPY\\Planning\\main\\STATE.md"}}' | json_get "tool_input.file_path")
if [[ "$RESULT" == 'M:\CODE_COPY\Planning\main\STATE.md' ]]; then
  _pass "Windows backslash path extracted correctly"
else
  _fail "Windows path handling" "expected Windows path, got '$RESULT'"
fi

# --- Report ---
report_results "test-json-helper.sh"
