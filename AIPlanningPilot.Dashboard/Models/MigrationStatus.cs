namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents the migration status of an entity in the model editor removal process.
/// </summary>
public enum MigrationStatus
{
    /// <summary>Entity has not been migrated yet.</summary>
    NotStarted,

    /// <summary>Entity migration is currently in progress.</summary>
    InProgress,

    /// <summary>Entity migration is complete.</summary>
    Done,

    /// <summary>Entity migration has been skipped.</summary>
    Skipped
}
