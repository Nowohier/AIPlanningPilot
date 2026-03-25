using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IMigrationParser"/>.
/// Parses MIGRATION.md tables into <see cref="MigrationEntity"/> records.
/// </summary>
public class MigrationParser : IMigrationParser
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationParser"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files.</param>
    public MigrationParser(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <inheritdoc />
    public List<MigrationEntity> Parse(string filePath)
    {
        var content = fileSystemService.ReadAllText(filePath);
        var entities = new List<MigrationEntity>();

        var rows = MarkdownTableParser.ExtractTableUnderHeading(content, ParserConstants.SectionMigrationStatus);
        foreach (var row in rows)
        {
            if (row.Length >= 7)
            {
                entities.Add(new MigrationEntity
                {
                    Name = row[0],
                    Domain = row[1],
                    Complexity = ParseComplexityTier(row[2]),
                    PropertyCount = int.TryParse(row[3], out var props) ? props : 0,
                    HasUi = row[4].Equals("Yes", StringComparison.OrdinalIgnoreCase),
                    HasManualCode = row[5].Equals("Yes", StringComparison.OrdinalIgnoreCase),
                    Status = ParseMigrationStatus(row[6]),
                    Date = row.Length > 7 ? row[7] : string.Empty
                });
            }
        }

        return entities;
    }

    /// <summary>
    /// Parses a complexity tier string into a <see cref="ComplexityTier"/> enum value.
    /// </summary>
    private static ComplexityTier ParseComplexityTier(string text)
    {
        var lower = text.ToLowerInvariant().Trim();
        return lower switch
        {
            "simple" => ComplexityTier.Simple,
            "medium" => ComplexityTier.Medium,
            "complex" => ComplexityTier.Complex,
            "verycomplex" or "very complex" => ComplexityTier.VeryComplex,
            _ => ComplexityTier.Simple
        };
    }

    /// <summary>
    /// Parses a migration status string into a <see cref="MigrationStatus"/> enum value.
    /// </summary>
    private static MigrationStatus ParseMigrationStatus(string text)
    {
        var lower = text.ToLowerInvariant().Trim();
        return lower switch
        {
            "done" => MigrationStatus.Done,
            "inprogress" or "in progress" => MigrationStatus.InProgress,
            "skipped" => MigrationStatus.Skipped,
            _ => MigrationStatus.NotStarted
        };
    }
}
