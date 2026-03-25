namespace AIPlanningPilot.Dashboard.Models;

/// <summary>
/// Groups migration entities by domain for display in the migration tracker view.
/// </summary>
public class DomainMigrationGroup
{
    /// <summary>Gets or sets the domain name.</summary>
    public string DomainName { get; set; } = string.Empty;

    /// <summary>Gets or sets the total number of entities in this domain.</summary>
    public int EntityCount { get; set; }

    /// <summary>Gets or sets the number of entities with status Done.</summary>
    public int DoneCount { get; set; }

    /// <summary>Gets or sets the number of entities with status InProgress.</summary>
    public int InProgressCount { get; set; }

    /// <summary>Gets or sets the migration progress percentage (0-100).</summary>
    public double ProgressPercent { get; set; }

    /// <summary>Gets or sets the entities belonging to this domain.</summary>
    public List<MigrationEntity> Entities { get; set; } = [];
}
