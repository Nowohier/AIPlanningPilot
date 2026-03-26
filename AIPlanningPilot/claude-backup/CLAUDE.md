# Personal preferences

- **Developer**: UNCONFIGURED

> **ONBOARDING REQUIRED**: The Developer field is set to UNCONFIGURED.
> This means `/onboard` has not been run yet. All planning commands
> (`/moin`, `/ciao`, `/refresh`, `/decision`, `/checkpoint`, `/recap`)
> MUST refuse to execute and instead respond:
> **"You haven't onboarded yet. Run `/onboard` to set up your environment first."**
> Only `/onboard` and `/healthcheck` are allowed to run without a configured developer.

- Never commit on my behalf — I handle all git commits manually.

## Path Variables (machine-specific)

These variables are used in `.claude/commands/` and in the planning repo documentation.
Resolve them when following instructions in command files.

| Variable | Value |
|----------|-------|
| `${PROJECT_REPO}` | `UNCONFIGURED` |
| `${PLANNING_REPO}` | `UNCONFIGURED` |
| `${SOURCE_REPO}` | `UNCONFIGURED` |

> **Setup**: Each developer must update the values above to match their local paths.
> See also `${PLANNING_REPO}/main/CONFIG.md` for full documentation of the path variable convention.
