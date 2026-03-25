using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="MigrationParser"/>.
/// </summary>
[TestFixture]
public class MigrationParserTests
{
    private Mock<IFileSystemService> _mockFs = null!;
    private MigrationParser _parser = null!;

    /// <summary>
    /// Sets up mocks and the parser instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        _parser = new MigrationParser(_mockFs.Object);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockFs.VerifyAll();
    }

    /// <summary>
    /// Verifies that a valid MIGRATION.md table is parsed into the correct entity list.
    /// </summary>
    [Test]
    public void Parse_WhenValidTable_ShouldReturnEntities()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Customer | Sales | Simple | 5 | Yes | No | Done | 2026-03-20 |",
            "| Order | Sales | Medium | 12 | Yes | Yes | In Progress | 2026-03-21 |",
            "| Product | Catalog | Complex | 25 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Customer");
        result[0].Domain.Should().Be("Sales");
        result[1].Name.Should().Be("Order");
        result[2].Name.Should().Be("Product");
    }

    /// <summary>
    /// Verifies that an empty file returns an empty entity list.
    /// </summary>
    [Test]
    public void Parse_WhenEmptyFile_ShouldReturnEmptyList()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(string.Empty);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that a file with no table returns an empty entity list.
    /// </summary>
    [Test]
    public void Parse_WhenNoTable_ShouldReturnEmptyList()
    {
        // Arrange
        var content = "# MIGRATION.md\n\nSome text without any table.\n";
        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that a row with fewer than 7 columns is skipped.
    /// </summary>
    [Test]
    public void Parse_WhenRowHasLessThan7Columns_ShouldSkipRow()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Customer | Sales | Simple | 5 | Yes | No | Done | 2026-03-20 |",
            "| Incomplete | Sales | Simple |",
            "| Order | Sales | Medium | 12 | Yes | Yes | In Progress | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Customer");
        result[1].Name.Should().Be("Order");
    }

    /// <summary>
    /// Verifies that a "Simple" complexity string maps to <see cref="ComplexityTier.Simple"/>.
    /// </summary>
    [Test]
    public void Parse_WhenComplexityIsSimple_ShouldReturnSimpleTier()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Complexity.Should().Be(ComplexityTier.Simple);
    }

    /// <summary>
    /// Verifies that a "Medium" complexity string maps to <see cref="ComplexityTier.Medium"/>.
    /// </summary>
    [Test]
    public void Parse_WhenComplexityIsMedium_ShouldReturnMediumTier()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Medium | 10 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Complexity.Should().Be(ComplexityTier.Medium);
    }

    /// <summary>
    /// Verifies that a "Complex" complexity string maps to <see cref="ComplexityTier.Complex"/>.
    /// </summary>
    [Test]
    public void Parse_WhenComplexityIsComplex_ShouldReturnComplexTier()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Complex | 25 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Complexity.Should().Be(ComplexityTier.Complex);
    }

    /// <summary>
    /// Verifies that a "Very Complex" complexity string maps to <see cref="ComplexityTier.VeryComplex"/>.
    /// </summary>
    [Test]
    public void Parse_WhenComplexityIsVeryComplex_ShouldReturnVeryComplexTier()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Very Complex | 30 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Complexity.Should().Be(ComplexityTier.VeryComplex);
    }

    /// <summary>
    /// Verifies that a "Done" status string maps to <see cref="MigrationStatus.Done"/>.
    /// </summary>
    [Test]
    public void Parse_WhenStatusIsDone_ShouldReturnDoneStatus()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Done | 2026-03-20 |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(MigrationStatus.Done);
    }

    /// <summary>
    /// Verifies that an "In Progress" status string maps to <see cref="MigrationStatus.InProgress"/>.
    /// </summary>
    [Test]
    public void Parse_WhenStatusIsInProgress_ShouldReturnInProgressStatus()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | In Progress | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(MigrationStatus.InProgress);
    }

    /// <summary>
    /// Verifies that a "Not Started" (or unrecognized) status string maps to <see cref="MigrationStatus.NotStarted"/>.
    /// </summary>
    [Test]
    public void Parse_WhenStatusIsNotStarted_ShouldReturnNotStartedStatus()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(MigrationStatus.NotStarted);
    }

    /// <summary>
    /// Verifies that a "Skipped" status string maps to <see cref="MigrationStatus.Skipped"/>.
    /// </summary>
    [Test]
    public void Parse_WhenStatusIsSkipped_ShouldReturnSkippedStatus()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Skipped | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(MigrationStatus.Skipped);
    }

    /// <summary>
    /// Verifies that HasUI is set to true when the "Has UI" column is "Yes".
    /// </summary>
    [Test]
    public void Parse_WhenHasUIIsYes_ShouldSetHasUITrue()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | Yes | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].HasUi.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasUI is set to false when the "Has UI" column is not "Yes".
    /// </summary>
    [Test]
    public void Parse_WhenHasUIIsNo_ShouldSetHasUIFalse()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].HasUi.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that HasManualCode is set to true when the "Manual" column is "Yes".
    /// </summary>
    [Test]
    public void Parse_WhenHasManualCodeIsYes_ShouldSetHasManualCodeTrue()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | Yes | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].HasManualCode.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasManualCode is set to false when the "Manual" column is not "Yes".
    /// </summary>
    [Test]
    public void Parse_WhenHasManualCodeIsNo_ShouldSetHasManualCodeFalse()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].HasManualCode.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the Date property is populated when the row has 8 columns.
    /// </summary>
    [Test]
    public void Parse_WhenRowHas8Columns_ShouldIncludeDate()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 5 | Yes | No | Done | 2026-03-20 |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Date.Should().Be("2026-03-20");
    }

    /// <summary>
    /// Verifies that the Date property defaults to empty string when the row has only 7 columns.
    /// </summary>
    [Test]
    public void Parse_WhenRowHas7Columns_ShouldSetDateToEmpty()
    {
        // Arrange
        var content = @"## Migration Status

| Entity | Domain | Complexity | Props | Has UI | Manual | Status |
|--------|--------|------------|-------|--------|--------|--------|
| Entity1 | Domain1 | Simple | 5 | No | No | Not Started |
";

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].Date.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that the PropertyCount is correctly parsed from the Props column.
    /// </summary>
    [Test]
    public void Parse_WhenPropsColumnHasValidNumber_ShouldSetPropertyCount()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | 15 | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].PropertyCount.Should().Be(15);
    }

    /// <summary>
    /// Verifies that PropertyCount defaults to 0 when the Props column is not a valid number.
    /// </summary>
    [Test]
    public void Parse_WhenPropsColumnIsInvalid_ShouldDefaultToZero()
    {
        // Arrange
        var content = BuildMigrationContent(
            "| Entity1 | Domain1 | Simple | N/A | No | No | Not Started | |");

        _mockFs.Setup(fs => fs.ReadAllText("migration.md")).Returns(content);

        // Act
        var result = _parser.Parse("migration.md");

        // Assert
        result.Should().HaveCount(1);
        result[0].PropertyCount.Should().Be(0);
    }

    /// <summary>
    /// Verifies that the constructor throws when the file system service is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MigrationParser(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystemService");
    }

    /// <summary>
    /// Builds a MIGRATION.md content string with the "Migration Status" heading and table format.
    /// </summary>
    /// <param name="dataRows">The pipe-delimited data rows to include in the table.</param>
    /// <returns>A complete MIGRATION.md content string.</returns>
    private static string BuildMigrationContent(params string[] dataRows)
    {
        var lines = new List<string>
        {
            "## Migration Status",
            "",
            "| Entity | Domain | Complexity | Props | Has UI | Manual | Status | Date |",
            "|--------|--------|------------|-------|--------|--------|--------|------|"
        };
        lines.AddRange(dataRows);
        return string.Join("\n", lines) + "\n";
    }
}
