# Security Review

> **Origin**: Adapted from [everything-claude-code](https://github.com/affaan-m/everything-claude-code) `agents/security-reviewer.md` (2026-03-23)

> **Trigger rule**: Only run this command when the user explicitly invokes it via `/security-review`. Do NOT trigger from conversational mentions of "security", "vulnerability", "OWASP", or similar phrasing.

Perform a security-focused code review covering OWASP Top 10 and project-specific concerns. This command reviews both backend (.NET 8) and frontend (Angular 20) code.

## Arguments

`$ARGUMENTS` may contain:
- **Scope**: file path, directory, feature name, or `full` for entire codebase (default: changed files in current branch)
- **Focus**: specific concern like `auth`, `input-validation`, `secrets` (default: all areas)

## Analysis Areas

### Backend (.NET 8)

#### 1. Authentication & Authorization
- `[Authorize]` attribute present on all non-public controller actions
- Policy-based authorization correctly configured where needed
- JWT middleware properly validating tokens (issuer, audience, expiry)
- No endpoints accidentally exposed without auth
- Role/claim checks not bypassed through direct object references

#### 2. Input Validation
- Model validation attributes (`[Required]`, `[MaxLength]`, etc.) on all DTOs
- FluentValidation rules for complex validation scenarios
- No raw user input passed to file paths, process execution, or reflection
- Request size limits configured

#### 3. EF Core Query Safety
- All queries use parameterized LINQ (no string concatenation in queries)
- `FromSqlRaw`/`FromSqlInterpolated` used correctly (parameterized)
- No raw SQL with user-controlled input
- Bulk operations properly scoped (no mass update/delete without filters)

#### 4. Secret Management
- No secrets in `appsettings.json` (only in `appsettings.Development.json` or env vars)
- Connection strings use environment variables in production config
- No API keys, passwords, or tokens hardcoded in source code
- Logging does not expose sensitive data (passwords, tokens, PII)

#### 5. Error Disclosure
- Custom error handling middleware in place (no stack traces in production)
- Exception details not leaked to API responses
- Error messages do not reveal internal structure

#### 6. CORS Configuration
- CORS policy properly scoped (not `AllowAnyOrigin` with credentials)
- Allowed origins explicitly listed for production

#### 7. Dependency Vulnerabilities
- Check for known CVEs in NuGet packages

### Frontend (Angular 20)

#### 8. DOM Security
- No `bypassSecurityTrustHtml`, `bypassSecurityTrustScript`, `bypassSecurityTrustUrl` usage without justification
- No `innerHTML` binding with user-controlled content
- No `document.write()` or `eval()` calls
- Template interpolation (`{{ }}`) used instead of `[innerHTML]` where possible

#### 9. Route Guard Coverage
- All protected routes have appropriate guards (auth, role-based)
- Guards check actual permissions, not just login status
- No sensitive views accessible without authentication

#### 10. Client-Side Data Storage
- No tokens, passwords, or PII in `localStorage`/`sessionStorage` without encryption
- Sensitive data cleared on logout
- No sensitive data in URL query parameters

#### 11. CSRF & HTTP Security
- CSRF token handling via Angular's `HttpClientXsrfModule` or custom interceptor
- `HttpOnly` and `Secure` flags on auth cookies
- CSP headers configured

#### 12. Nx Boundary Enforcement
- No cross-domain imports that could leak sensitive data between feature areas
- Import boundaries in `nx.json`/`project.json` correctly enforced

### Known Issues (Project-Specific)

- Auth interceptor 401 handling is commented out (Day 1 finding) -- flag if still present
- Double `provideRouter()` call in app config (Day 1 finding) -- flag if still present

## Severity Levels

- **Critical**: Exploitable vulnerability, data breach risk, authentication bypass
- **High**: Security weakness that could be exploited with effort, missing auth checks, XSS vectors
- **Medium**: Defense-in-depth gaps, missing headers, overly permissive configs
- **Low**: Best practice deviations, informational findings

## Output Format

For each finding:

- **File**: path to the affected file
- **Issue**: clear description of the vulnerability
- **Category**: which analysis area (1-12) it falls under
- **Severity**: Critical / High / Medium / Low
- **Fix**: concrete, actionable remediation steps with code example

Summarize totals at the end grouped by severity and category.

## Common False Positives

- Environment variables in `.env.example` or `appsettings.Development.json` (not actual secrets if clearly development-only)
- Test credentials in test files (if clearly marked as test data)
- SHA256/MD5 used for checksums or content hashing (not password hashing)
- `[AllowAnonymous]` on intentionally public endpoints (login, health check)

Always verify context before flagging.
