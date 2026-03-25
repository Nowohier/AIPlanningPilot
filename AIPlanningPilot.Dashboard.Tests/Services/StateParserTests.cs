using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="StateParser"/>.
/// </summary>
[TestFixture]
public class StateParserTests
{
    private Mock<IFileSystemService> _mockFs = null!;
    private Mock<IConfigurationService> _mockConfig = null!;
    private StateParser _parser = null!;
    private string _sampleContent = null!;

    /// <summary>
    /// Sets up mocks and the parser instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        _mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _parser = new StateParser(_mockFs.Object, _mockConfig.Object);
        _sampleContent = File.ReadAllText(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "sample-state.md"));
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
    /// Verifies that the current phase is correctly extracted from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenValidStateFile_ShouldExtractCurrentPhase()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.CurrentPhase.Should().Contain("Phase 1");
        result.CurrentPhase.Should().Contain("Deep Analysis");
    }

    /// <summary>
    /// Verifies that the day number is correctly parsed from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenValidStateFile_ShouldExtractDayNumber()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.Day.Should().Be(1);
    }

    /// <summary>
    /// Verifies that the last updated date is correctly extracted from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenValidStateFile_ShouldExtractLastUpdated()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.LastUpdated.Should().Contain("2026-03-23");
    }

    /// <summary>
    /// Verifies that the branch name is correctly extracted from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenValidStateFile_ShouldExtractBranch()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.Branch.Should().Contain("features/restructure");
    }

    /// <summary>
    /// Verifies that all next action rows are parsed from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenNextActionsPresent_ShouldParseAllRows()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.NextActions.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that next action statuses are correctly parsed from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenNextActionsPresent_ShouldParseStatusCorrectly()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.NextActions[0].Status.Should().Be(ActionStatus.Next);
        result.NextActions[1].Status.Should().Be(ActionStatus.Next);
        result.NextActions[2].Status.Should().Be(ActionStatus.Pending);
        result.NextActions[3].Status.Should().Be(ActionStatus.Done);
    }

    /// <summary>
    /// Verifies that all phase progress rows are parsed from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenPhaseProgressPresent_ShouldParseAllPhases()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress.Should().HaveCount(3);
        result.PhaseProgress[0].PhaseName.Should().Contain("Pre-Phase");
        result.PhaseProgress[1].PhaseName.Should().Contain("Phase 1");
    }

    /// <summary>
    /// Verifies that open decisions are parsed from a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenOpenDecisionsPresent_ShouldParseAll()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.OpenDecisions.Should().HaveCount(3);
        result.OpenDecisions[0].Impact.Should().Be(ImpactLevel.High);
        result.OpenDecisions[2].Impact.Should().Be(ImpactLevel.VeryHigh);
    }

    /// <summary>
    /// Verifies that team members are extracted from the team section of a valid state file.
    /// </summary>
    [Test]
    public void Parse_WhenTeamSectionPresent_ShouldExtractMembers()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.TeamMembers.Should().Contain("Chris");
        result.TeamMembers.Should().Contain("Dev B");
    }

    /// <summary>
    /// Verifies that the constructor throws when the file system service is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullFileSystem_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new StateParser(null!, _mockConfig.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("fileSystemService");
    }

    /// <summary>
    /// Verifies that the constructor throws when the configuration service is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new StateParser(_mockFs.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("configurationService");
    }

    /// <summary>
    /// Verifies that a 3-column Phase Progress table (without PlanFile column) is parsed
    /// with backward compatibility, leaving PlanFilePath null and PlanHeadings empty.
    /// </summary>
    [Test]
    public void Parse_WhenThreeColumnPhaseProgressTable_ShouldParseWithoutPlanFile()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress.Should().HaveCount(3);
        result.PhaseProgress.Should().AllSatisfy(p =>
        {
            p.PlanFilePath.Should().BeNull();
            p.PlanHeadings.Should().BeEmpty();
        });
    }

    /// <summary>
    /// Verifies that 1-based Index values are assigned to each phase progress item.
    /// </summary>
    [Test]
    public void Parse_WhenPhaseProgressPresent_ShouldAssignOneBasedIndex()
    {
        // Arrange
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(_sampleContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress[0].Index.Should().Be(1);
        result.PhaseProgress[1].Index.Should().Be(2);
        result.PhaseProgress[2].Index.Should().Be(3);
    }

    /// <summary>
    /// Verifies that the PlanFile column is parsed and the resolved plan file is read
    /// when the file exists, populating PlanFilePath and PlanHeadings.
    /// </summary>
    [Test]
    public void Parse_WhenPlanFileColumnExistsAndFileExists_ShouldPopulatePlanHeadings()
    {
        // Arrange
        var stateContent = @"# STATE.md -- Morning Briefing

> **Last Updated**: 2026-03-24 (EOD)
> **Branch**: features/restructure
> **Phase**: Phase 1 -- Deep Analysis
> **Day**: 2 (started 2026-03-05)

## Phase Progress

| Phase | Status | Summary | PlanFile |
|-------|--------|---------|----------|
| **Pre-Phase** | Done | Setup complete | plans/pre-phase.md |
| **Phase 1** (Days 1-4) | In Progress | Analysis | plans/phase1.md |
";

        var planContent = @"# Phase 1 Plan

## Goals
Some goals text.

### Sub-goal A
Details about sub-goal A.

## Timeline
Timeline details.

### Milestone 1
First milestone.
";

        var prePhaseExpectedPath = Path.Combine(@"C:\root", "plans/pre-phase.md");
        var phase1ExpectedPath = Path.Combine(@"C:\root", "plans/phase1.md");

        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(stateContent);
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");

        _mockFs.Setup(fs => fs.FileExists(prePhaseExpectedPath)).Returns(true);
        _mockFs.Setup(fs => fs.ReadAllText(prePhaseExpectedPath)).Returns(planContent);
        _mockFs.Setup(fs => fs.FileExists(phase1ExpectedPath)).Returns(true);
        _mockFs.Setup(fs => fs.ReadAllText(phase1ExpectedPath)).Returns(planContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress.Should().HaveCount(2);

        var prePhase = result.PhaseProgress[0];
        prePhase.PlanFilePath.Should().Be(prePhaseExpectedPath);
        prePhase.PlanHeadings.Should().HaveCount(4);
        prePhase.PlanHeadings[0].Level.Should().Be(2);
        prePhase.PlanHeadings[0].Text.Should().Be("Goals");
        prePhase.PlanHeadings[1].Level.Should().Be(3);
        prePhase.PlanHeadings[1].Text.Should().Be("Sub-goal A");
        prePhase.PlanHeadings[2].Level.Should().Be(2);
        prePhase.PlanHeadings[2].Text.Should().Be("Timeline");
        prePhase.PlanHeadings[3].Level.Should().Be(3);
        prePhase.PlanHeadings[3].Text.Should().Be("Milestone 1");

        var phase1 = result.PhaseProgress[1];
        phase1.PlanFilePath.Should().Be(phase1ExpectedPath);
        phase1.PlanHeadings.Should().HaveCount(4);
    }

    /// <summary>
    /// Verifies that when a PlanFile column references a non-existent file,
    /// PlanFilePath remains null and PlanHeadings stays empty.
    /// </summary>
    [Test]
    public void Parse_WhenPlanFileColumnExistsButFileDoesNotExist_ShouldLeavePlanFilePathNull()
    {
        // Arrange
        var stateContent = @"# STATE.md -- Morning Briefing

> **Last Updated**: 2026-03-24 (EOD)
> **Branch**: features/restructure
> **Phase**: Phase 1 -- Deep Analysis
> **Day**: 2 (started 2026-03-05)

## Phase Progress

| Phase | Status | Summary | PlanFile |
|-------|--------|---------|----------|
| **Phase 1** (Days 1-4) | In Progress | Analysis | plans/missing.md |
";

        var expectedPath = Path.Combine(@"C:\root", "plans/missing.md");
        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(stateContent);
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _mockFs.Setup(fs => fs.FileExists(expectedPath)).Returns(false);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress.Should().HaveCount(1);
        var phase = result.PhaseProgress[0];
        phase.PlanFilePath.Should().BeNull();
        phase.PlanHeadings.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that when the PlanFile column value is empty or whitespace,
    /// no file resolution is attempted and PlanFilePath remains null.
    /// </summary>
    [Test]
    public void Parse_WhenPlanFileColumnIsEmpty_ShouldNotAttemptFileResolution()
    {
        // Arrange
        var stateContent = @"# STATE.md -- Morning Briefing

> **Last Updated**: 2026-03-24 (EOD)
> **Branch**: features/restructure
> **Phase**: Phase 1 -- Deep Analysis
> **Day**: 2 (started 2026-03-05)

## Phase Progress

| Phase | Status | Summary | PlanFile |
|-------|--------|---------|----------|
| **Phase 1** (Days 1-4) | In Progress | Analysis |  |
";

        _mockFs.Setup(fs => fs.ReadAllText("state.md")).Returns(stateContent);

        // Act
        var result = _parser.Parse("state.md");

        // Assert
        result.PhaseProgress.Should().HaveCount(1);
        var phase = result.PhaseProgress[0];
        phase.PlanFilePath.Should().BeNull();
        phase.PlanHeadings.Should().BeEmpty();
    }
}
