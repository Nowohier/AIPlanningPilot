#!/usr/bin/env node
/**
 * sync-claude.mjs -- Sync .claude/ from claude-backup/ templates.
 *
 * Replaces the manual /moin Step 0b (30-50 tool calls) with a single script.
 * Reads developer config from .claude/CLAUDE.md, then syncs commands, hooks,
 * skills, and hook registrations from the backup template.
 *
 * Usage:  node restructuring/scripts/sync-claude.mjs
 * Run from project root. Exit 0 = success, 1 = error.
 */

import { readFileSync, writeFileSync, readdirSync, mkdirSync, existsSync, statSync, copyFileSync } from 'node:fs';
import { join, resolve, basename } from 'node:path';
import { fileURLToPath } from 'node:url';

// ---------------------------------------------------------------------------
// Config parsing
// ---------------------------------------------------------------------------

/**
 * Converts a Windows path to a bash-compatible path.
 * E.g. M:\Projects\MyApp -> /m/Projects/MyApp
 * Unix paths are returned unchanged.
 * @param {string} winPath
 * @returns {string}
 */
export function winToBashPath(winPath) {
    // Already a Unix path
    if (winPath.startsWith('/')) return winPath;

    const match = winPath.match(/^([A-Za-z]):[/\\](.*)/);
    if (!match) return winPath;

    const drive = match[1].toLowerCase();
    const rest = match[2].replace(/\\/g, '/');
    return `/${drive}/${rest}`;
}

/**
 * Parses .claude/CLAUDE.md to extract developer name and path variables.
 * @param {string} claudeMdPath - Absolute path to .claude/CLAUDE.md
 * @returns {{ developer: string, projectRepo: string, planningRepo: string, bashProjectPath: string, bashPlanningPath: string }}
 */
export function parseConfig(claudeMdPath) {
    let content;
    try {
        content = readFileSync(claudeMdPath, 'utf-8');
    } catch {
        throw new Error(`Cannot parse ${claudeMdPath}. Run /onboard first.`);
    }

    // Extract developer name: **Developer**: {name}
    const devMatch = content.match(/\*\*Developer\*\*:\s*(.+)/);
    if (!devMatch) {
        throw new Error(`Developer field not found in ${claudeMdPath}. Run /onboard first.`);
    }
    const developer = devMatch[1].trim();
    if (developer === 'UNCONFIGURED') {
        throw new Error('Developer not configured. Run /onboard first.');
    }

    // Extract path variables from the markdown table.
    // Table rows look like: | `${PROJECT_REPO}` | `M:\CODE_COPY\MyOrg\MyProject` |
    const projectRepoMatch = content.match(/\|\s*`\$\{PROJECT_REPO\}`\s*\|\s*`([^`]+)`/);
    const planningRepoMatch = content.match(/\|\s*`\$\{PLANNING_REPO\}`\s*\|\s*`([^`]+)`/);

    if (!projectRepoMatch || !planningRepoMatch) {
        throw new Error(`Path variables not found in ${claudeMdPath}`);
    }

    const projectRepo = projectRepoMatch[1].trim();
    const planningRepo = planningRepoMatch[1].trim();

    return {
        developer,
        projectRepo,
        planningRepo,
        bashProjectPath: winToBashPath(projectRepo),
        bashPlanningPath: winToBashPath(planningRepo),
    };
}

// ---------------------------------------------------------------------------
// Command sync
// ---------------------------------------------------------------------------

// Regex for the two-line template header block (both variants).
// Variant A: > **Paths**: This template uses ${PROJECT_REPO} and ${PLANNING_REPO} variables.
// Variant B: > **Paths**: This template uses <PROJECT_REPO> and <PLANNING_REPO> variables.
// Both followed by: > Run `/onboard` to configure literal paths for your machine. See `main/CONFIG.md`.
// Anchored to line start to avoid matching indented documentation references.
const HEADER_REGEX = /^> \*\*Paths\*\*: This template uses .*(?:PROJECT_REPO|PLANNING_REPO).*\n^> Run [`']\/onboard[`'] to configure literal paths for your machine\. See [`']main\/CONFIG\.md[`']\./m;

/**
 * Builds the deployed header for command files.
 * @param {string} developer
 * @returns {string}
 */
function buildDeployedHeader(developer) {
    return `> **Paths**: This command uses literal paths for ${developer}'s machine.\n> If paths differ, update them here or run \`/onboard\`. Canonical docs: \`main/CONFIG.md\`.`;
}

/**
 * Syncs command files from backup to target, applying header and path substitutions.
 * @param {{ developer: string, projectRepo: string, planningRepo: string }} config
 * @param {string} backupDir - Path to claude-backup/commands/
 * @param {string} targetDir - Path to .claude/commands/
 * @returns {{ synced: number, localPreserved: number }}
 */
export function syncCommands(config, backupDir, targetDir) {
    mkdirSync(targetDir, { recursive: true });

    const backupFiles = readdirSync(backupDir).filter(f => f.endsWith('.md'));
    const targetFiles = new Set(readdirSync(targetDir).filter(f => f.endsWith('.md')));
    const backupFileSet = new Set(backupFiles);

    const deployedHeader = buildDeployedHeader(config.developer);

    for (const file of backupFiles) {
        let content = readFileSync(join(backupDir, file), 'utf-8');

        // Normalize CRLF to LF before processing (templates may have Windows line endings)
        content = content.replace(/\r\n/g, '\n').replace(/\r/g, '\n');

        // Replace template header with deployed header
        content = content.replace(HEADER_REGEX, deployedHeader);

        // Protect escaped references: \${VAR} → sentinel (these are instructional examples, not paths)
        content = content.split('\\${PROJECT_REPO}').join('__ESCAPED_PROJECT_REPO__');
        content = content.split('\\${PLANNING_REPO}').join('__ESCAPED_PLANNING_REPO__');

        // Substitute path variables in body
        // Use split/join for literal replacement (no regex escaping needed)
        content = content.split('${PLANNING_REPO}').join(config.planningRepo);
        content = content.split('${PROJECT_REPO}').join(config.projectRepo);

        // Restore escaped references back to literal ${VAR} text
        content = content.split('__ESCAPED_PROJECT_REPO__').join('${PROJECT_REPO}');
        content = content.split('__ESCAPED_PLANNING_REPO__').join('${PLANNING_REPO}');

        writeFileSync(join(targetDir, file), content, 'utf-8');
    }

    // Count local-only files (in target but not in backup)
    let localPreserved = 0;
    for (const file of targetFiles) {
        if (!backupFileSet.has(file)) localPreserved++;
    }

    return { synced: backupFiles.length, localPreserved };
}

// ---------------------------------------------------------------------------
// Hook sync
// ---------------------------------------------------------------------------

/**
 * Syncs hook files from backup to target, resolving env.sh variables and fixing line endings.
 * @param {{ developer: string, bashProjectPath: string, bashPlanningPath: string }} config
 * @param {string} backupDir - Path to claude-backup/hooks/
 * @param {string} targetDir - Path to .claude/hooks/
 * @returns {{ synced: number }}
 */
export function syncHooks(config, backupDir, targetDir) {
    mkdirSync(targetDir, { recursive: true });

    const backupFiles = readdirSync(backupDir).filter(f => f.endsWith('.sh'));

    for (const file of backupFiles) {
        let content = readFileSync(join(backupDir, file), 'utf-8');

        // Special handling for env.sh: resolve variable placeholders
        if (file === 'env.sh') {
            // Replace only the top-level variable assignments, not derived ones like PLANNING_DIR
            content = content.replace(
                /^PROJECT_REPO=.*$/m,
                `PROJECT_REPO="${config.bashProjectPath}"`
            );
            content = content.replace(
                /^PLANNING_REPO=.*$/m,
                `PLANNING_REPO="${config.bashPlanningPath}"`
            );
            content = content.replace(
                /^DEVELOPER=.*$/m,
                `DEVELOPER="${config.developer}"`
            );
        }

        // Fix line endings: CRLF -> LF
        content = content.replace(/\r\n/g, '\n');
        // Also handle stray \r
        content = content.replace(/\r/g, '\n');

        writeFileSync(join(targetDir, file), content, 'utf-8');
    }

    return { synced: backupFiles.length };
}

// ---------------------------------------------------------------------------
// Skills sync
// ---------------------------------------------------------------------------

/**
 * Recursively copies a directory, resolving symlinks to actual file content.
 * @param {string} src
 * @param {string} dest
 */
export function copyDirRecursive(src, dest) {
    mkdirSync(dest, { recursive: true });

    const entries = readdirSync(src, { withFileTypes: true });
    for (const entry of entries) {
        const srcPath = join(src, entry.name);
        const destPath = join(dest, entry.name);

        // Resolve symlinks: check the real stat, not the link stat
        const realStat = statSync(srcPath);
        if (realStat.isDirectory()) {
            copyDirRecursive(srcPath, destPath);
        } else {
            // copyFileSync follows symlinks by default (copies actual content)
            copyFileSync(srcPath, destPath);
        }
    }
}

/**
 * Syncs skill directories from backup to target, preserving local-only skills.
 * @param {string} backupDir - Path to claude-backup/skills/
 * @param {string} targetDir - Path to .claude/skills/
 * @returns {{ synced: number, localPreserved: number }}
 */
export function syncSkills(backupDir, targetDir) {
    mkdirSync(targetDir, { recursive: true });

    const backupEntries = readdirSync(backupDir, { withFileTypes: true })
        .filter(e => e.isDirectory());
    const targetEntries = existsSync(targetDir)
        ? readdirSync(targetDir, { withFileTypes: true }).filter(e => e.isDirectory())
        : [];

    const backupNames = new Set(backupEntries.map(e => e.name));

    for (const entry of backupEntries) {
        copyDirRecursive(join(backupDir, entry.name), join(targetDir, entry.name));
    }

    let localPreserved = 0;
    for (const entry of targetEntries) {
        if (!backupNames.has(entry.name)) localPreserved++;
    }

    return { synced: backupEntries.length, localPreserved };
}

// ---------------------------------------------------------------------------
// Hook registration merge
// ---------------------------------------------------------------------------

/**
 * Checks if a hook entry is a "planning hook" (references a script in the backup hooks dir).
 * @param {object} hookEntry - A hook entry with { type, command }
 * @param {Set<string>} hookFileNames - Set of filenames from claude-backup/hooks/
 * @returns {boolean}
 */
function isPlanningHook(hookEntry, hookFileNames) {
    if (!hookEntry.command) return false;
    for (const name of hookFileNames) {
        if (hookEntry.command.includes(name)) return true;
    }
    return false;
}

/**
 * Merges hook registrations from backup settings.json into target settings.json.
 * @param {string} backupSettingsPath - Path to claude-backup/settings.json
 * @param {string} targetSettingsPath - Path to .claude/settings.json
 * @param {Set<string>} hookFileNames - Set of hook script filenames from claude-backup/hooks/
 * @returns {{ changed: boolean, added: number, removed: number }}
 */
export function mergeHookRegistrations(backupSettingsPath, targetSettingsPath, hookFileNames) {
    const backupSettings = JSON.parse(readFileSync(backupSettingsPath, 'utf-8'));
    const backupHooks = backupSettings.hooks || {};

    let targetSettings;
    if (existsSync(targetSettingsPath)) {
        targetSettings = JSON.parse(readFileSync(targetSettingsPath, 'utf-8'));
    } else {
        // First-time setup: start with empty object
        targetSettings = {};
    }

    const targetBefore = JSON.stringify(targetSettings);

    const targetHooks = targetSettings.hooks || {};
    const hookPoints = ['PreToolUse', 'PostToolUse', 'UserPromptSubmit'];

    for (const point of hookPoints) {
        const backupEntries = backupHooks[point] || [];
        const targetEntries = targetHooks[point] || [];

        // Filter out all planning hooks from target
        const nonPlanningEntries = targetEntries.filter(entry => {
            const hooks = entry.hooks || [];
            // An entry is a planning entry if ANY of its hooks is a planning hook
            return !hooks.some(h => isPlanningHook(h, hookFileNames));
        });

        // Append all planning entries from backup
        const planningEntries = backupEntries.filter(entry => {
            const hooks = entry.hooks || [];
            return hooks.some(h => isPlanningHook(h, hookFileNames));
        });

        const merged = [...nonPlanningEntries, ...planningEntries];

        if (merged.length > 0) {
            targetHooks[point] = merged;
        } else {
            delete targetHooks[point];
        }
    }

    targetSettings.hooks = targetHooks;

    // Clean up empty hooks object
    if (Object.keys(targetSettings.hooks).length === 0) {
        delete targetSettings.hooks;
    }

    const targetAfter = JSON.stringify(targetSettings);
    const changed = targetBefore !== targetAfter;

    writeFileSync(targetSettingsPath, JSON.stringify(targetSettings, null, 2) + '\n', 'utf-8');

    // Count changes for reporting
    let added = 0;
    let removed = 0;
    if (changed) {
        const beforeHooks = JSON.parse(targetBefore).hooks || {};
        const afterHooks = targetSettings.hooks || {};
        for (const point of hookPoints) {
            const beforeCount = (beforeHooks[point] || []).length;
            const afterCount = (afterHooks[point] || []).length;
            if (afterCount > beforeCount) added += afterCount - beforeCount;
            if (beforeCount > afterCount) removed += beforeCount - afterCount;
        }
    }

    return { changed, added, removed };
}

// ---------------------------------------------------------------------------
// Project config
// ---------------------------------------------------------------------------

/**
 * Writes main/project.json into the planning repo with the project name
 * derived from the PROJECT_REPO path. The Dashboard reads this file at startup
 * to display the orchestrated project's name in the title bar.
 * @param {string} planningRepo - Absolute path to the planning repo
 * @param {string} projectRepo - Absolute path to the project repo
 * @returns {{ written: boolean }}
 */
export function writeProjectConfig(planningRepo, projectRepo) {
    const projectName = basename(projectRepo.replace(/[\\/]+$/, ''));
    const configDir = join(planningRepo, 'main');
    const configPath = join(configDir, 'project.json');

    mkdirSync(configDir, { recursive: true });

    const content = JSON.stringify({ projectName }, null, 2) + '\n';

    // Only write if changed (avoid unnecessary git diffs)
    if (existsSync(configPath)) {
        const existing = readFileSync(configPath, 'utf-8');
        if (existing === content) {
            return { written: false };
        }
    }

    writeFileSync(configPath, content, 'utf-8');
    return { written: true };
}

// ---------------------------------------------------------------------------
// Main
// ---------------------------------------------------------------------------

/**
 * Runs the full sync. Designed to be called from main() or tests.
 * @param {string} projectRoot - Absolute path to the project root
 * @returns {{ commands: object, hooks: object, skills: object, settings: object }}
 */
export function runSync(projectRoot) {
    const claudeMdPath = join(projectRoot, '.claude', 'CLAUDE.md');
    const config = parseConfig(claudeMdPath);

    const backupBase = join(config.planningRepo, 'claude-backup');
    const targetBase = join(projectRoot, '.claude');

    if (!existsSync(backupBase)) {
        throw new Error(`Backup directory not found: ${backupBase}`);
    }

    const commands = syncCommands(
        config,
        join(backupBase, 'commands'),
        join(targetBase, 'commands')
    );

    const hooks = syncHooks(
        config,
        join(backupBase, 'hooks'),
        join(targetBase, 'hooks')
    );

    const skills = syncSkills(
        join(backupBase, 'skills'),
        join(targetBase, 'skills')
    );

    // Build hook filename set from backup hooks directory
    const hookFileNames = new Set(
        readdirSync(join(backupBase, 'hooks')).filter(f => f.endsWith('.sh'))
    );

    const settings = mergeHookRegistrations(
        join(backupBase, 'settings.json'),
        join(targetBase, 'settings.json'),
        hookFileNames
    );

    const projectConfig = writeProjectConfig(config.planningRepo, config.projectRepo);

    return { commands, hooks, skills, settings, projectConfig };
}

/**
 * Entry point. Only runs when the script is executed directly.
 */
function main() {
    const projectRoot = resolve(process.cwd());

    try {
        const result = runSync(projectRoot);

        console.log(`Commands: synced ${result.commands.synced} files (${result.commands.localPreserved} local preserved)`);
        console.log(`Hooks:    synced ${result.hooks.synced} files (LF line endings applied)`);
        console.log(`Skills:   synced ${result.skills.synced} directories (${result.skills.localPreserved} local preserved)`);

        if (result.settings.changed) {
            console.log(`Settings: ${result.settings.added} hook(s) added, ${result.settings.removed} hook(s) removed -- RESTART SESSION for changes to take effect`);
        } else {
            console.log('Settings: no registration changes');
        }

        if (result.projectConfig.written) {
            console.log('Project:  main/project.json updated');
        } else {
            console.log('Project:  main/project.json unchanged');
        }
    } catch (err) {
        console.error(`Error: ${err.message}`);
        process.exit(1);
    }
}

// Run main() only when executed directly (not imported as a module)
const __filename = fileURLToPath(import.meta.url);
if (process.argv[1] && resolve(process.argv[1]) === resolve(__filename)) {
    main();
}
