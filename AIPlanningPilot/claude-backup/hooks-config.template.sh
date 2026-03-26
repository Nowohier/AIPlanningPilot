#!/usr/bin/env bash
# hooks-config.sh -- Project-specific hook configuration
#
# This file is sourced by env.sh to configure guardrail hooks.
# It lives at main/hooks-config.sh in the planning repo (git-tracked,
# shared by all developers on the project).
#
# Created by /onboard. Edit to match your project's needs.

# --- Protected paths ---
# Block Edit/Write to these directories (space-separated, relative to project root).
# Example: PROTECTED_PATHS="legacy server vendor"
PROTECTED_PATHS=""

# --- Protected patterns ---
# Block Edit/Write to paths matching these patterns (case-insensitive,
# matched anywhere in the path, space-separated).
# Example: PROTECTED_PATTERNS="generated auto-generated"
PROTECTED_PATTERNS=""

# --- Git commit blocking ---
# Set to "true" to prevent Claude from running git commit.
# The developer reviews and commits changes manually.
BLOCK_GIT_COMMIT="false"
