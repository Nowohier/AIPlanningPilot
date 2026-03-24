/**
 * Tests for sync-claude.mjs
 *
 * Run: node --test restructuring/scripts/sync-claude.test.mjs
 *
 * Uses Node.js built-in test runner (node:test) and assertion library (node:assert).
 * Each test creates a temp directory, populates fixtures, runs the function, and asserts results.
 */

import { describe, it, beforeEach, afterEach } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtempSync, mkdirSync, writeFileSync, readFileSync, existsSync, rmSync, symlinkSync } from 'node:fs';
import { join } from 'node:path';
import { tmpdir } from 'node:os';

import {
  winToBashPath,
  parseConfig,
  syncCommands,
  syncHooks,
  syncSkills,
  copyDirRecursive,
  mergeHookRegistrations,
  runSync,
} from './sync-claude.mjs';

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/** Creates a temp directory and returns its path. */
function createTempDir() {
  return mkdtempSync(join(tmpdir(), 'sync-claude-test-'));
}

/** Writes a minimal .claude/CLAUDE.md config file. */
function writeClaudeMd(dir, { developer = 'TestDev', projectRepo = 'M:\\TestProject', planningRepo = 'M:\\TestProject\\restructuring' } = {}) {
  const content = `# Personal preferences

- **Developer**: ${developer}

## Path Variables (machine-specific)

| Variable | Value |
|----------|-------|
| \`\${PROJECT_REPO}\` | \`${projectRepo}\` |
| \`\${PLANNING_REPO}\` | \`${planningRepo}\` |
`;
  const claudeDir = join(dir, '.claude');
  mkdirSync(claudeDir, { recursive: true });
  writeFileSync(join(claudeDir, 'CLAUDE.md'), content, 'utf-8');
}

// ---------------------------------------------------------------------------
// winToBashPath
// ---------------------------------------------------------------------------

describe('winToBashPath', () => {
  it('converts a standard Windows path', () => {
    assert.equal(winToBashPath('M:\\CODE_COPY\\MyOrg'), '/m/Projects/MyApp');
  });

  it('handles a different drive letter', () => {
    assert.equal(winToBashPath('D:\\Projects\\ExampleProject'), '/d/Projects/ExampleProject');
  });

  it('handles trailing backslash', () => {
    assert.equal(winToBashPath('M:\\CODE_COPY\\'), '/m/CODE_COPY/');
  });

  it('passes through Unix paths unchanged', () => {
    assert.equal(winToBashPath('/already/unix'), '/already/unix');
  });

  it('handles forward slashes in Windows path', () => {
    assert.equal(winToBashPath('M:/Projects/MyApp'), '/m/Projects/MyApp');
  });
});

// ---------------------------------------------------------------------------
// parseConfig
// ---------------------------------------------------------------------------

describe('parseConfig', () => {
  let tempDir;

  beforeEach(() => { tempDir = createTempDir(); });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('parses a valid CLAUDE.md', () => {
    writeClaudeMd(tempDir, {
      developer: 'Chris',
      projectRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject',
      planningRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject\\restructuring',
    });

    const config = parseConfig(join(tempDir, '.claude', 'CLAUDE.md'));
    assert.equal(config.developer, 'Chris');
    assert.equal(config.projectRepo, 'M:\\CODE_COPY\\MyOrg\\ExampleProject');
    assert.equal(config.planningRepo, 'M:\\CODE_COPY\\MyOrg\\ExampleProject\\restructuring');
  });

  it('derives bash paths correctly', () => {
    writeClaudeMd(tempDir, {
      projectRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject',
      planningRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject\\restructuring',
    });

    const config = parseConfig(join(tempDir, '.claude', 'CLAUDE.md'));
    assert.equal(config.bashProjectPath, '/m/CODE_COPY/ExampleOrg/ExampleProject');
    assert.equal(config.bashPlanningPath, '/m/CODE_COPY/ExampleOrg/ExampleProject/restructuring');
  });

  it('rejects UNCONFIGURED developer', () => {
    writeClaudeMd(tempDir, { developer: 'UNCONFIGURED' });
    assert.throws(
      () => parseConfig(join(tempDir, '.claude', 'CLAUDE.md')),
      /not configured/
    );
  });

  it('rejects missing path variables', () => {
    const claudeDir = join(tempDir, '.claude');
    mkdirSync(claudeDir, { recursive: true });
    writeFileSync(join(claudeDir, 'CLAUDE.md'), '- **Developer**: Chris\n', 'utf-8');

    assert.throws(
      () => parseConfig(join(claudeDir, 'CLAUDE.md')),
      /not found/
    );
  });

  it('rejects missing file', () => {
    assert.throws(
      () => parseConfig(join(tempDir, 'nonexistent', 'CLAUDE.md')),
      /Cannot parse/
    );
  });
});

// ---------------------------------------------------------------------------
// syncCommands
// ---------------------------------------------------------------------------

describe('syncCommands', () => {
  let tempDir;
  let backupDir;
  let targetDir;
  const config = {
    developer: 'Chris',
    projectRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject',
    planningRepo: 'M:\\CODE_COPY\\MyOrg\\ExampleProject\\restructuring',
  };

  beforeEach(() => {
    tempDir = createTempDir();
    backupDir = join(tempDir, 'backup', 'commands');
    targetDir = join(tempDir, 'target', 'commands');
    mkdirSync(backupDir, { recursive: true });
    mkdirSync(targetDir, { recursive: true });
  });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('replaces variant A header (${...} style)', () => {
    const template = `# Test Command

> **Paths**: This template uses \${PROJECT_REPO} and \${PLANNING_REPO} variables.
> Run \`/onboard\` to configure literal paths for your machine. See \`main/CONFIG.md\`.

Read \${PLANNING_REPO}\\main\\STATE.md
`;
    writeFileSync(join(backupDir, 'test.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'test.md'), 'utf-8');
    assert.ok(result.includes("This command uses literal paths for Chris's machine."));
    assert.ok(result.includes('Canonical docs: `main/CONFIG.md`.'));
    assert.ok(!result.includes('This template uses'));
  });

  it('replaces variant B header (<...> style)', () => {
    const template = `# Test Command

> **Paths**: This template uses <PROJECT_REPO> and <PLANNING_REPO> variables.
> Run \`/onboard\` to configure literal paths for your machine. See \`main/CONFIG.md\`.

Some body text.
`;
    writeFileSync(join(backupDir, 'test.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'test.md'), 'utf-8');
    assert.ok(result.includes("This command uses literal paths for Chris's machine."));
    assert.ok(!result.includes('This template uses'));
  });

  it('substitutes path variables in body', () => {
    const template = `Read \${PLANNING_REPO}\\main\\STATE.md and \${PROJECT_REPO}\\CLAUDE.md\n`;
    writeFileSync(join(backupDir, 'test.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'test.md'), 'utf-8');
    assert.ok(result.includes('M:\\CODE_COPY\\MyOrg\\ExampleProject\\restructuring\\main\\STATE.md'));
    assert.ok(result.includes('M:\\CODE_COPY\\MyOrg\\ExampleProject\\CLAUDE.md'));
    assert.ok(!result.includes('${PROJECT_REPO}'));
    assert.ok(!result.includes('${PLANNING_REPO}'));
  });

  it('preserves local-only commands', () => {
    writeFileSync(join(backupDir, 'synced.md'), '# Synced\n', 'utf-8');
    writeFileSync(join(targetDir, 'local-only.md'), '# Local\n', 'utf-8');

    const result = syncCommands(config, backupDir, targetDir);

    assert.ok(existsSync(join(targetDir, 'local-only.md')));
    assert.ok(existsSync(join(targetDir, 'synced.md')));
    assert.equal(result.localPreserved, 1);
  });

  it('does not replace indented Paths references', () => {
    // Simulates the moin.md case where the header appears indented as documentation
    const template = `# Moin

> **Paths**: This template uses \${PROJECT_REPO} and \${PLANNING_REPO} variables.
> Run \`/onboard\` to configure literal paths for your machine. See \`main/CONFIG.md\`.

5. In each command file, replace the header with:
   > **Paths**: This command uses literal paths for {developer name}'s machine.
   > If paths differ, update them here or run \`/onboard\`. Canonical docs: \`main/CONFIG.md\`.
`;
    writeFileSync(join(backupDir, 'moin.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'moin.md'), 'utf-8');
    // The real header (at line start) should be replaced
    assert.ok(result.includes("This command uses literal paths for Chris's machine."));
    // The indented documentation should be preserved as-is
    assert.ok(result.includes('{developer name}'));
  });

  it('preserves escaped \\${...} references as literal text', () => {
    const template = [
      '# Healthcheck',
      '',
      'Resolve ${PROJECT_REPO}/some/path.',
      'Placeholders like `\\${PROJECT_REPO}` still present.',
      'Replace `\\${PLANNING_REPO}` with the path.',
      '',
    ].join('\n');
    writeFileSync(join(backupDir, 'test.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'test.md'), 'utf-8');
    // The real reference should be substituted
    assert.ok(result.includes('M:\\CODE_COPY\\MyOrg\\ExampleProject/some/path'));
    // The escaped references should be preserved as literal ${...}
    assert.ok(result.includes('`${PROJECT_REPO}`'));
    assert.ok(result.includes('`${PLANNING_REPO}`'));
  });

  it('handles files without a header', () => {
    const template = `# TDD Command\n\nDo TDD stuff with \${PROJECT_REPO}.\n`;
    writeFileSync(join(backupDir, 'tdd.md'), template, 'utf-8');

    syncCommands(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'tdd.md'), 'utf-8');
    assert.ok(result.includes('M:\\CODE_COPY\\MyOrg\\ExampleProject'));
    assert.ok(!result.includes('${PROJECT_REPO}'));
  });

  it('returns correct counts', () => {
    writeFileSync(join(backupDir, 'a.md'), '# A\n', 'utf-8');
    writeFileSync(join(backupDir, 'b.md'), '# B\n', 'utf-8');
    writeFileSync(join(targetDir, 'local.md'), '# Local\n', 'utf-8');

    const result = syncCommands(config, backupDir, targetDir);
    assert.equal(result.synced, 2);
    assert.equal(result.localPreserved, 1);
  });
});

// ---------------------------------------------------------------------------
// syncHooks
// ---------------------------------------------------------------------------

describe('syncHooks', () => {
  let tempDir;
  let backupDir;
  let targetDir;
  const config = {
    developer: 'Chris',
    bashProjectPath: '/m/CODE_COPY/ExampleOrg/ExampleProject',
    bashPlanningPath: '/m/CODE_COPY/ExampleOrg/ExampleProject/restructuring',
  };

  beforeEach(() => {
    tempDir = createTempDir();
    backupDir = join(tempDir, 'backup', 'hooks');
    targetDir = join(tempDir, 'target', 'hooks');
    mkdirSync(backupDir, { recursive: true });
    mkdirSync(targetDir, { recursive: true });
  });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('resolves env.sh variable placeholders', () => {
    const template = `#!/usr/bin/env bash
PROJECT_REPO="\${PROJECT_REPO}"
PLANNING_REPO="\${PLANNING_REPO}"
DEVELOPER="UNCONFIGURED"

PLANNING_DIR="\${PLANNING_REPO}"
STATE_FILE="\${PLANNING_DIR}/main/STATE.md"
`;
    writeFileSync(join(backupDir, 'env.sh'), template, 'utf-8');

    syncHooks(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'env.sh'), 'utf-8');
    assert.ok(result.includes('PROJECT_REPO="/m/CODE_COPY/ExampleOrg/ExampleProject"'));
    assert.ok(result.includes('PLANNING_REPO="/m/CODE_COPY/ExampleOrg/ExampleProject/restructuring"'));
    assert.ok(result.includes('DEVELOPER="Chris"'));
  });

  it('does not modify PLANNING_DIR or other derived variables', () => {
    const template = `#!/usr/bin/env bash
PROJECT_REPO="\${PROJECT_REPO}"
PLANNING_REPO="\${PLANNING_REPO}"
DEVELOPER="UNCONFIGURED"

PLANNING_DIR="\${PLANNING_REPO}"
STATE_FILE="\${PLANNING_DIR}/main/STATE.md"
`;
    writeFileSync(join(backupDir, 'env.sh'), template, 'utf-8');

    syncHooks(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'env.sh'), 'utf-8');
    // PLANNING_DIR should still use bash variable expansion
    assert.ok(result.includes('PLANNING_DIR="${PLANNING_REPO}"'));
    assert.ok(result.includes('STATE_FILE="${PLANNING_DIR}/main/STATE.md"'));
  });

  it('converts CRLF to LF', () => {
    const template = '#!/usr/bin/env bash\r\necho "hello"\r\necho "world"\r\n';
    writeFileSync(join(backupDir, 'test.sh'), template, 'utf-8');

    syncHooks(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'test.sh'), 'utf-8');
    assert.ok(!result.includes('\r'));
    assert.equal(result, '#!/usr/bin/env bash\necho "hello"\necho "world"\n');
  });

  it('copies non-env hooks with only line ending changes', () => {
    const template = '#!/usr/bin/env bash\necho "validate"\n';
    writeFileSync(join(backupDir, 'validate-state.sh'), template, 'utf-8');

    syncHooks(config, backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'validate-state.sh'), 'utf-8');
    assert.equal(result, template);
  });

  it('returns correct count', () => {
    writeFileSync(join(backupDir, 'a.sh'), '#!/bin/bash\n', 'utf-8');
    writeFileSync(join(backupDir, 'b.sh'), '#!/bin/bash\n', 'utf-8');

    const result = syncHooks(config, backupDir, targetDir);
    assert.equal(result.synced, 2);
  });
});

// ---------------------------------------------------------------------------
// syncSkills
// ---------------------------------------------------------------------------

describe('syncSkills', () => {
  let tempDir;
  let backupDir;
  let targetDir;

  beforeEach(() => {
    tempDir = createTempDir();
    backupDir = join(tempDir, 'backup', 'skills');
    targetDir = join(tempDir, 'target', 'skills');
    mkdirSync(backupDir, { recursive: true });
    mkdirSync(targetDir, { recursive: true });
  });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('copies skill directory recursively', () => {
    const skillDir = join(backupDir, 'angular-component');
    const refsDir = join(skillDir, 'references');
    mkdirSync(refsDir, { recursive: true });
    writeFileSync(join(skillDir, 'SKILL.md'), '# Angular Component\n', 'utf-8');
    writeFileSync(join(refsDir, 'patterns.md'), '# Patterns\n', 'utf-8');

    syncSkills(backupDir, targetDir);

    assert.ok(existsSync(join(targetDir, 'angular-component', 'SKILL.md')));
    assert.ok(existsSync(join(targetDir, 'angular-component', 'references', 'patterns.md')));
  });

  it('overwrites existing skill files', () => {
    // Create old version in target
    const targetSkill = join(targetDir, 'build-fix');
    mkdirSync(targetSkill, { recursive: true });
    writeFileSync(join(targetSkill, 'SKILL.md'), '# Old\n', 'utf-8');

    // Create new version in backup
    const backupSkill = join(backupDir, 'build-fix');
    mkdirSync(backupSkill, { recursive: true });
    writeFileSync(join(backupSkill, 'SKILL.md'), '# New\n', 'utf-8');

    syncSkills(backupDir, targetDir);

    const result = readFileSync(join(targetDir, 'build-fix', 'SKILL.md'), 'utf-8');
    assert.equal(result, '# New\n');
  });

  it('preserves local-only skills', () => {
    // Local skill not in backup
    const localSkill = join(targetDir, 'my-custom-skill');
    mkdirSync(localSkill, { recursive: true });
    writeFileSync(join(localSkill, 'SKILL.md'), '# Custom\n', 'utf-8');

    // Backup skill
    const backupSkill = join(backupDir, 'angular-signals');
    mkdirSync(backupSkill, { recursive: true });
    writeFileSync(join(backupSkill, 'SKILL.md'), '# Signals\n', 'utf-8');

    const result = syncSkills(backupDir, targetDir);

    assert.ok(existsSync(join(targetDir, 'my-custom-skill', 'SKILL.md')));
    assert.equal(result.localPreserved, 1);
    assert.equal(result.synced, 1);
  });

  it('returns correct counts', () => {
    mkdirSync(join(backupDir, 'skill-a'), { recursive: true });
    mkdirSync(join(backupDir, 'skill-b'), { recursive: true });
    writeFileSync(join(backupDir, 'skill-a', 'SKILL.md'), '# A\n', 'utf-8');
    writeFileSync(join(backupDir, 'skill-b', 'SKILL.md'), '# B\n', 'utf-8');

    const result = syncSkills(backupDir, targetDir);
    assert.equal(result.synced, 2);
    assert.equal(result.localPreserved, 0);
  });
});

// ---------------------------------------------------------------------------
// mergeHookRegistrations
// ---------------------------------------------------------------------------

describe('mergeHookRegistrations', () => {
  let tempDir;
  let backupSettingsPath;
  let targetSettingsPath;
  const hookFileNames = new Set(['block-literal-paths.sh', 'post-edit-validate.sh', 'check-handover.sh']);

  const backupSettings = {
    hooks: {
      PreToolUse: [{
        matcher: 'Edit|Write',
        hooks: [{ type: 'command', command: 'bash "hooks/block-literal-paths.sh"' }],
      }],
      PostToolUse: [{
        matcher: 'Edit|Write',
        hooks: [{ type: 'command', command: 'bash "hooks/post-edit-validate.sh"' }],
      }],
      UserPromptSubmit: [{
        matcher: '',
        hooks: [{ type: 'command', command: 'bash "hooks/check-handover.sh"' }],
      }],
    },
  };

  beforeEach(() => {
    tempDir = createTempDir();
    backupSettingsPath = join(tempDir, 'backup-settings.json');
    targetSettingsPath = join(tempDir, 'target-settings.json');
    writeFileSync(backupSettingsPath, JSON.stringify(backupSettings, null, 2), 'utf-8');
  });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('adds missing planning hooks', () => {
    writeFileSync(targetSettingsPath, '{}', 'utf-8');

    const result = mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames);
    assert.equal(result.changed, true);

    const merged = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    assert.equal(merged.hooks.PreToolUse.length, 1);
    assert.equal(merged.hooks.PostToolUse.length, 1);
    assert.equal(merged.hooks.UserPromptSubmit.length, 1);
  });

  it('preserves non-planning hooks', () => {
    const targetSettings = {
      hooks: {
        PreToolUse: [
          {
            matcher: 'Bash',
            hooks: [{ type: 'command', command: 'bash "my-custom-hook.sh"' }],
          },
        ],
      },
    };
    writeFileSync(targetSettingsPath, JSON.stringify(targetSettings, null, 2), 'utf-8');

    mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames);

    const merged = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    // Should have the custom hook + the planning hook
    assert.equal(merged.hooks.PreToolUse.length, 2);
    assert.ok(merged.hooks.PreToolUse[0].hooks[0].command.includes('my-custom-hook.sh'));
    assert.ok(merged.hooks.PreToolUse[1].hooks[0].command.includes('block-literal-paths.sh'));
  });

  it('removes stale planning hooks', () => {
    const targetSettings = {
      hooks: {
        PreToolUse: [
          {
            matcher: 'Edit|Write',
            hooks: [{ type: 'command', command: 'bash "hooks/old-deleted-hook.sh"' }],
          },
        ],
      },
    };
    // old-deleted-hook.sh is NOT in hookFileNames, so it's a non-planning hook
    // and should be preserved. But let's test a scenario where a planning hook
    // was in backup before but is now removed.

    // Add old-deleted-hook to the fingerprint set to simulate it being a planning hook
    const extendedNames = new Set([...hookFileNames, 'old-deleted-hook.sh']);
    writeFileSync(targetSettingsPath, JSON.stringify(targetSettings, null, 2), 'utf-8');

    // Now the backup does NOT have old-deleted-hook.sh, so it should be removed
    mergeHookRegistrations(backupSettingsPath, targetSettingsPath, extendedNames);

    const merged = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    // Only the backup's planning hook should remain (block-literal-paths.sh)
    assert.equal(merged.hooks.PreToolUse.length, 1);
    assert.ok(merged.hooks.PreToolUse[0].hooks[0].command.includes('block-literal-paths.sh'));
  });

  it('returns false when unchanged', () => {
    // Target already matches backup
    writeFileSync(targetSettingsPath, JSON.stringify(backupSettings, null, 2), 'utf-8');

    const result = mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames);
    assert.equal(result.changed, false);
  });

  it('preserves non-hook settings', () => {
    const targetSettings = {
      someOtherSetting: 'value',
      hooks: {},
    };
    writeFileSync(targetSettingsPath, JSON.stringify(targetSettings, null, 2), 'utf-8');

    mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames);

    const merged = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    assert.equal(merged.someOtherSetting, 'value');
  });

  it('creates settings.json if missing', () => {
    // Don't create the target file -- it should be created by the function
    assert.ok(!existsSync(targetSettingsPath));

    const result = mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames);

    assert.ok(existsSync(targetSettingsPath));
    assert.equal(result.changed, true);
    const merged = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    assert.ok(merged.hooks.PreToolUse);
  });
});

// ---------------------------------------------------------------------------
// copyDirRecursive
// ---------------------------------------------------------------------------

describe('copyDirRecursive', () => {
  let tempDir;

  beforeEach(() => { tempDir = createTempDir(); });
  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('copies nested directories and files', () => {
    const src = join(tempDir, 'src');
    const dest = join(tempDir, 'dest');
    mkdirSync(join(src, 'sub'), { recursive: true });
    writeFileSync(join(src, 'a.txt'), 'A', 'utf-8');
    writeFileSync(join(src, 'sub', 'b.txt'), 'B', 'utf-8');

    copyDirRecursive(src, dest);

    assert.equal(readFileSync(join(dest, 'a.txt'), 'utf-8'), 'A');
    assert.equal(readFileSync(join(dest, 'sub', 'b.txt'), 'utf-8'), 'B');
  });
});

// ---------------------------------------------------------------------------
// Integration: runSync (end-to-end)
// ---------------------------------------------------------------------------

describe('runSync (integration)', () => {
  let tempDir;
  let planningDir;

  beforeEach(() => {
    tempDir = createTempDir();

    // Set up .claude/CLAUDE.md -- planningRepo points to where claude-backup/ lives
    planningDir = join(tempDir, 'restructuring', 'planning');
    writeClaudeMd(tempDir, {
      developer: 'TestDev',
      projectRepo: 'D:\\MyProject',
      planningRepo: planningDir,
    });

    // Set up claude-backup structure inside the planning directory
    const backupBase = join(planningDir, 'claude-backup');
    mkdirSync(join(backupBase, 'commands'), { recursive: true });
    mkdirSync(join(backupBase, 'hooks'), { recursive: true });
    mkdirSync(join(backupBase, 'skills', 'test-skill', 'references'), { recursive: true });

    // Command template
    writeFileSync(join(backupBase, 'commands', 'test-cmd.md'), `# Test

> **Paths**: This template uses \${PROJECT_REPO} and \${PLANNING_REPO} variables.
> Run \`/onboard\` to configure literal paths for your machine. See \`main/CONFIG.md\`.

Read \${PLANNING_REPO}\\main\\STATE.md
`, 'utf-8');

    // Hook template
    writeFileSync(join(backupBase, 'hooks', 'env.sh'), `#!/usr/bin/env bash
PROJECT_REPO="\${PROJECT_REPO}"
PLANNING_REPO="\${PLANNING_REPO}"
DEVELOPER="UNCONFIGURED"
PLANNING_DIR="\${PLANNING_REPO}"
`, 'utf-8');

    writeFileSync(join(backupBase, 'hooks', 'validate.sh'), '#!/usr/bin/env bash\necho ok\n', 'utf-8');

    // Skill
    writeFileSync(join(backupBase, 'skills', 'test-skill', 'SKILL.md'), '# Test Skill\n', 'utf-8');
    writeFileSync(join(backupBase, 'skills', 'test-skill', 'references', 'ref.md'), '# Ref\n', 'utf-8');

    // Settings
    writeFileSync(join(backupBase, 'settings.json'), JSON.stringify({
      hooks: {
        PreToolUse: [{
          matcher: 'Edit|Write',
          hooks: [{ type: 'command', command: 'bash "hooks/validate.sh"' }],
        }],
      },
    }, null, 2), 'utf-8');

    // Create a local-only command in target
    mkdirSync(join(tempDir, '.claude', 'commands'), { recursive: true });
    writeFileSync(join(tempDir, '.claude', 'commands', 'local-cmd.md'), '# Local\n', 'utf-8');
  });

  afterEach(() => { rmSync(tempDir, { recursive: true, force: true }); });

  it('performs full end-to-end sync', () => {
    const result = runSync(tempDir);

    // Commands
    assert.equal(result.commands.synced, 1);
    assert.equal(result.commands.localPreserved, 1);
    const cmd = readFileSync(join(tempDir, '.claude', 'commands', 'test-cmd.md'), 'utf-8');
    assert.ok(cmd.includes("literal paths for TestDev's machine"));
    assert.ok(cmd.includes(join(planningDir, 'main', 'STATE.md')));
    assert.ok(!cmd.includes('${PLANNING_REPO}'));

    // Local command preserved
    assert.ok(existsSync(join(tempDir, '.claude', 'commands', 'local-cmd.md')));

    // Hooks
    assert.equal(result.hooks.synced, 2);
    const envSh = readFileSync(join(tempDir, '.claude', 'hooks', 'env.sh'), 'utf-8');
    assert.ok(envSh.includes('PROJECT_REPO="/d/MyProject"'));
    assert.ok(envSh.includes(`PLANNING_REPO="${winToBashPath(planningDir)}"`));
    assert.ok(envSh.includes('DEVELOPER="TestDev"'));
    assert.ok(envSh.includes('PLANNING_DIR="${PLANNING_REPO}"'));
    assert.ok(!envSh.includes('\r'));

    // Skills
    assert.equal(result.skills.synced, 1);
    assert.ok(existsSync(join(tempDir, '.claude', 'skills', 'test-skill', 'SKILL.md')));
    assert.ok(existsSync(join(tempDir, '.claude', 'skills', 'test-skill', 'references', 'ref.md')));

    // Settings
    assert.equal(result.settings.changed, true);
    const settings = JSON.parse(readFileSync(join(tempDir, '.claude', 'settings.json'), 'utf-8'));
    assert.equal(settings.hooks.PreToolUse.length, 1);
  });

  it('is idempotent (second run produces no changes)', () => {
    // First run
    runSync(tempDir);

    // Second run
    const result = runSync(tempDir);

    assert.equal(result.settings.changed, false);

    // Files should be identical
    const cmd = readFileSync(join(tempDir, '.claude', 'commands', 'test-cmd.md'), 'utf-8');
    assert.ok(cmd.includes("literal paths for TestDev's machine"));
    assert.ok(!cmd.includes('${PLANNING_REPO}'));
  });
});
