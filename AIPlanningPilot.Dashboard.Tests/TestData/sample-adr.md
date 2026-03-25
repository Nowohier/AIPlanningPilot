# Decision 004: Quality standards

> **Date**: 2026-03-05
> **Phase**: Pre-Phase
> **Participants**: Chris, Claude

## Context

The new frontend replaces generated code with hand-written code. Without the model editor as source of truth, quality must be ensured through testing and documentation.

## Decision

Every component, service, store, and utility gets unit tests. Every feature gets integration tests. Every class, method, and exported type gets JSDoc/TSDoc documentation.

## Alternatives Considered

1. Partial testing -- rejected, too risky
2. AI-generated tests only -- rejected, needs human review

## Consequences

- Slower initial development but higher confidence
- AI must generate tests alongside code
