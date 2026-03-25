namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Constants for markdown section headings and metadata keys used by parsers.
/// Centralizes all hard-coded strings to avoid duplication and enable refactoring.
/// </summary>
internal static class ParserConstants
{
    // -- STATE.md blockquote keys --

    /// <summary>Phase metadata key in STATE.md blockquote.</summary>
    public const string KeyPhase = "Phase";

    /// <summary>Day metadata key in STATE.md blockquote.</summary>
    public const string KeyDay = "Day";

    /// <summary>Last Updated metadata key in STATE.md blockquote.</summary>
    public const string KeyLastUpdated = "Last Updated";

    /// <summary>Branch metadata key in STATE.md blockquote.</summary>
    public const string KeyBranch = "Branch";

    // -- STATE.md section headings --

    /// <summary>Next Actions section heading in STATE.md.</summary>
    public const string SectionNextActions = "Next Actions";

    /// <summary>Phase Progress section heading in STATE.md.</summary>
    public const string SectionPhaseProgress = "Phase Progress";

    /// <summary>Open Decisions section heading in STATE.md.</summary>
    public const string SectionOpenDecisions = "Open Decisions";

    /// <summary>Team &amp; Branches section heading in STATE.md.</summary>
    public const string SectionTeamBranches = "Team & Branches";

    // -- ADR metadata keys --

    /// <summary>Date metadata key in ADR files.</summary>
    public const string KeyDate = "Date";

    /// <summary>Participants metadata key in ADR files.</summary>
    public const string KeyParticipants = "Participants";

    // -- ADR section headings --

    /// <summary>Context section heading in ADR files.</summary>
    public const string SectionContext = "Context";

    /// <summary>Decision section heading in ADR files.</summary>
    public const string SectionDecision = "Decision";

    // -- Handover section headings --

    /// <summary>For Next Session section heading in handover files.</summary>
    public const string SectionForNextSession = "For Next Session";

    /// <summary>Decisions &amp; Findings section heading in handover files.</summary>
    public const string SectionDecisionsAndFindings = "Decisions & Findings";

    /// <summary>Open Threads section heading in handover files.</summary>
    public const string SectionOpenThreads = "Open Threads";

    /// <summary>From Last Session section heading in handover files.</summary>
    public const string SectionFromLastSession = "From Last Session";

    // -- Overview section headings --

    /// <summary>Risk Register section heading in plan/overview.md.</summary>
    public const string SectionRiskRegister = "Risk Register";

    // -- Analysis section headings --

    /// <summary>Entity Counts section heading in analysis/quick-ref.md.</summary>
    public const string SectionEntityCounts = "Entity Counts";

    /// <summary>Key Numbers section heading in analysis/quick-ref.md.</summary>
    public const string SectionKeyNumbers = "Key Numbers";

    /// <summary>Migration Order section heading in analysis/quick-ref.md.</summary>
    public const string SectionMigrationOrder = "Migration Order";

    // -- File paths relative to restructuring root --

    /// <summary>Relative path to STATE.md.</summary>
    public const string PathStateMd = "main";

    /// <summary>STATE.md file name.</summary>
    public const string FileStateMd = "STATE.md";

    /// <summary>Relative path to decisions directory.</summary>
    public const string PathDecisions = "decisions";

    /// <summary>INDEX.md file name.</summary>
    public const string FileIndexMd = "INDEX.md";

    /// <summary>Relative path to handovers directory.</summary>
    public const string PathHandovers = "handovers";

    /// <summary>Relative path to completed actions.</summary>
    public const string PathArchive = "archive";

    /// <summary>Completed actions file name.</summary>
    public const string FileCompletedActions = "completed-actions.md";

    // -- MIGRATION.md section headings --

    /// <summary>Migration Status section heading in MIGRATION.md.</summary>
    public const string SectionMigrationStatus = "Migration Status";

    // -- MIGRATION.md file path --

    /// <summary>MIGRATION.md file name.</summary>
    public const string FileMigrationMd = "MIGRATION.md";

    // -- Default values --

    /// <summary>Default markdown renderer theme name.</summary>
    public const string DefaultThemeName = "GitHub Light";

    // -- Tab indices --

    /// <summary>Index of the Files tab in the main tab control.</summary>
    public const int TabIndexFiles = 1;
}
