using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="DecisionTrackerViewModel"/>.
/// </summary>
[TestFixture]
public class DecisionTrackerViewModelTests
{
    private Mock<IConfigurationService> mockConfig = null!;
    private Mock<IDecisionParser> mockParser = null!;
    private Mock<IFileSystemService> mockFs = null!;
    private Mock<IMarkdownRenderer> mockRenderer = null!;

    [SetUp]
    public void SetUp()
    {
        mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockParser = new Mock<IDecisionParser>(MockBehavior.Strict);
        mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
        mockRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all strict mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        mockConfig.VerifyAll();
        mockParser.VerifyAll();
        mockFs.VerifyAll();
        mockRenderer.VerifyAll();
    }

    [Test]
    public void LoadData_WhenDecisionsExist_ShouldPopulateList()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\decisions")).Returns(true);
        mockParser.Setup(p => p.ParseAll(@"C:\root\decisions")).Returns(
        [
            new Decision { Number = 0, Title = "Plan structure", FilePath = @"C:\root\decisions\000.md" },
            new Decision { Number = 1, Title = "Tech eval", FilePath = @"C:\root\decisions\001.md" }
        ]);
        mockFs.Setup(fs => fs.FileExists(@"C:\root\decisions\000.md")).Returns(true);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\root\decisions\000.md")).Returns("# Decision 000");
        mockRenderer.Setup(r => r.RenderMarkdown("# Decision 000")).Returns("<html><body>rendered</body></html>");

        var vm = new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.Decisions.Should().HaveCount(2);
        vm.IsLoaded.Should().BeTrue();
        vm.SelectedDecision.Should().NotBeNull();
        vm.SelectedDecision!.Number.Should().Be(0);
    }

    [Test]
    public void LoadData_WhenDirectoryMissing_ShouldNotThrow()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\decisions")).Returns(false);

        var vm = new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.Decisions.Should().BeEmpty();
    }

    [Test]
    public void SelectedDecision_WhenChanged_ShouldRenderMarkdown()
    {
        // Arrange
        var decision = new Decision { Number = 4, Title = "Quality", FilePath = @"C:\004.md" };
        mockFs.Setup(fs => fs.FileExists(@"C:\004.md")).Returns(true);
        mockFs.Setup(fs => fs.ReadAllText(@"C:\004.md")).Returns("# Decision 004");
        mockRenderer.Setup(r => r.RenderMarkdown("# Decision 004")).Returns("<html><body>rendered</body></html>");

        var vm = new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Act
        vm.SelectedDecision = decision;

        // Assert
        vm.SelectedDecisionHtml.Should().NotBeNull();
    }

    [Test]
    public void SelectedDecision_WhenSetToNull_ShouldClearDocument()
    {
        // Arrange
        var vm = new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Act
        vm.SelectedDecision = null;

        // Assert
        vm.SelectedDecisionHtml.Should().BeNull();
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DecisionTrackerViewModel(null!, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullDecisionParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DecisionTrackerViewModel(mockConfig.Object, null!, mockFs.Object, mockRenderer.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("decisionParser");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, null!, mockRenderer.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownRenderer");
    }

    [Test]
    public void LoadData_WhenParserThrows_ShouldNotCrash()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\decisions")).Returns(true);
        mockParser.Setup(p => p.ParseAll(@"C:\root\decisions")).Throws(new InvalidOperationException("Corrupt file"));

        var vm = new DecisionTrackerViewModel(mockConfig.Object, mockParser.Object, mockFs.Object, mockRenderer.Object);

        // Act
        var act = () => vm.LoadData();

        // Assert
        act.Should().NotThrow();
        vm.Decisions.Should().BeEmpty();
    }
}
