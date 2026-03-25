namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Represents a single entity's migration tracking record, parsed from MIGRATION.md.
/// </summary>
public class MigrationEntity
{
    /// <summary>Gets or sets the entity class name (PascalCase).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the domain this entity belongs to.</summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>Gets or sets the complexity tier.</summary>
    public ComplexityTier Complexity { get; set; }

    /// <summary>Gets or sets the property count.</summary>
    public int PropertyCount { get; set; }

    /// <summary>Gets or sets whether this entity has frontend UI layouts.</summary>
    public bool HasUi { get; set; }

    /// <summary>Gets or sets whether this entity has manual code overrides (MANUAL CHANGE markers).</summary>
    public bool HasManualCode { get; set; }

    /// <summary>Gets or sets the current migration status.</summary>
    public MigrationStatus Status { get; set; }

    /// <summary>Gets or sets the date of last status change (ISO format, empty if NotStarted).</summary>
    public string Date { get; set; } = string.Empty;
}
