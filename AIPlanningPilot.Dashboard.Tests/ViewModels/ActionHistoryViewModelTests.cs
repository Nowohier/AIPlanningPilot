using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="ActionHistoryViewModel"/>.
/// </summary>
[TestFixture]
public class ActionHistoryViewModelTests
{
    private Mock<IConfigurationService> mockConfig = null!;
    private Mock<IActionHistoryParser> mockParser = null!;
    private Mock<IFileSystemService> mockFs = null!;

    [SetUp]
    public void SetUp()
    {
        mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockParser = new Mock<IActionHistoryParser>(MockBehavior.Strict);
        mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
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
    }

    [Test]
    public void LoadData_WhenActionsExist_ShouldPopulateList()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.FileExists(@"C:\root\archive\completed-actions.md")).Returns(true);
        mockParser.Setup(p => p.Parse(@"C:\root\archive\completed-actions.md")).Returns(
        [
            new CompletedAction { Number = 1, Description = "Action 1", Owner = "Chris", CompletedDate = "2026-03-04" },
            new CompletedAction { Number = 2, Description = "Action 2", Owner = "Claude", CompletedDate = "2026-03-04" }
        ]);

        var vm = new ActionHistoryViewModel(mockConfig.Object, mockParser.Object, mockFs.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.CompletedActions.Should().HaveCount(2);
        vm.IsLoaded.Should().BeTrue();
    }

    [Test]
    public void LoadData_WhenFileMissing_ShouldNotThrow()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.FileExists(@"C:\root\archive\completed-actions.md")).Returns(false);

        var vm = new ActionHistoryViewModel(mockConfig.Object, mockParser.Object, mockFs.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.CompletedActions.Should().BeEmpty();
        vm.IsLoaded.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ActionHistoryViewModel(null!, mockParser.Object, mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullActionHistoryParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ActionHistoryViewModel(mockConfig.Object, null!, mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("actionHistoryParser");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new ActionHistoryViewModel(mockConfig.Object, mockParser.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }
}
