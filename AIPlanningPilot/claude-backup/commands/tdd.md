# Test-Driven Development

> **Origin**: Adapted from [everything-claude-code](https://github.com/affaan-m/everything-claude-code) `agents/tdd-guide.md` (2026-03-23)

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/tdd`. Do NOT trigger from conversational mentions of "test-driven", "TDD", "write tests first", or similar phrasing.

Guide the current implementation task through a strict Red-Green-Refactor TDD cycle. This command enforces test-first development as mandated by ADR 004 (80% test coverage for all new code).

## Arguments

`$ARGUMENTS` may contain:
- **Target**: class/method/component/feature to develop (required)
- **Stack**: `backend`, `frontend`, or `both` (default: inferred from context)

If no target is specified, ask the user what they want to build.

## TDD Workflow

### Phase 1: RED -- Write Failing Tests

Write tests BEFORE any implementation. The tests define the expected behavior.

**Backend (.NET 8)**:
- Framework: NUnit + FluentAssertions v7 + Moq with `MockBehavior.Strict`
- Naming: `[TestName]_When[Condition]_Should[ExpectedResult]`
- Pattern: Arrange-Act-Assert (AAA)
- Location: Corresponding `*.Tests` project
- Document test class with `<summary>` XML doc

**Frontend (Angular 20)**:
- Framework: Jest
- Naming: `describe('ComponentName') > it('should do X when Y')`
- Location: Co-located `.spec.ts` file
- Use `TestBed` for component tests, plain instantiation for services

### Phase 2: Verify RED

Run the tests and confirm they FAIL. This proves the tests are actually testing something.

**Backend**:
```bash
dotnet test <solution.sln> --filter "FullyQualifiedName~ClassName"
```

**Frontend**:
```bash
npx nx test <project> --testPathPattern="<file>"
```

If tests pass without implementation, the tests are wrong. Fix them first.

### Phase 3: GREEN -- Write Minimal Implementation

Write the minimum code needed to make all tests pass. No more, no less.

- Follow existing project conventions (vertical slices for backend, Nx libraries for frontend)
- NEVER edit files in `Generated/` directories
- Document all classes and methods (backend: XML docs with `<summary>`, `<param>`, `<returns>`; frontend: JSDoc/TSDoc)

### Phase 4: Verify GREEN

Run the tests again. ALL must pass.

### Phase 5: REFACTOR

With green tests as a safety net, improve the code:
- Remove duplication
- Improve naming
- Simplify logic
- Apply SOLID principles

Run tests after each refactoring step to ensure nothing breaks.

### Phase 6: Coverage Check

Verify 100% coverage for the new code (ADR 004 mandate).

**Backend**:
```bash
dotnet test <solution.sln> --filter "FullyQualifiedName~ClassName" --collect:"XPlat Code Coverage"
```

**Frontend**:
```bash
npx nx test <project> --coverage --testPathPattern="<file>"
```

## Edge Cases Checklist

Every implementation MUST have tests for:

1. **Null/empty input** -- null references, empty strings, empty collections
2. **Boundary values** -- min/max, off-by-one, zero
3. **Invalid input** -- wrong types, malformed data, out-of-range values
4. **Error paths** -- exceptions, HTTP errors, timeouts
5. **Concurrency** -- if applicable, race conditions and thread safety
6. **Special characters** -- Unicode, SQL-significant chars, XSS payloads (for input handling)

## Anti-Patterns to Avoid

- **Testing implementation details**: Test behavior (outputs, side effects), not internal state
- **Shared test state**: Each test must be independent. Use `[SetUp]`/`beforeEach` for fresh state
- **Asserting too little**: `Should.NotThrow()` alone is not a meaningful assertion
- **Not mocking external dependencies**: Use Moq (backend) or Jest mocks (frontend) for external services, databases, HTTP calls
- **Writing too many tests at once**: One test at a time through the Red-Green cycle
- **Skipping the RED phase**: If you write tests that immediately pass, they are not testing new behavior

## Output

After completing the TDD cycle, summarize:
- Tests written (count, names)
- Implementation files created/modified
- Coverage achieved
- Any edge cases deferred (with justification)
