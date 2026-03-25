using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="HandoverViewModel"/>.
/// </summary>
[TestFixture]
public class HandoverViewModelTests
{
    private Mock<IConfigurationService> _mockConfig = null!;
    private Mock<IHandoverParser> _mockParser = null!;
    private Mock<IFileSystemService> _mockFs = null!;

    [SetUp]
    public void SetUp()
    {
        _mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        _mockParser = new Mock<IHandoverParser>(MockBehavior.Strict);
        _mockFs = new Mock<IFileSystemService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all strict mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _mockConfig.VerifyAll();
        _mockParser.VerifyAll();
        _mockFs.VerifyAll();
    }

    [Test]
    public void LoadData_WhenHandoversExist_ShouldPopulateAndSelectFirst()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(true);
        _mockParser.Setup(p => p.ParseAll(@"C:\root\handovers")).Returns(
        [
            new HandoverNotes
            {
                DeveloperName = "chris",
                LastUpdated = "2026-03-23",
                ForNextSession = ["Update CLAUDE.md", "Implement hooks"],
                OpenThreads = ["AG Grid decision"]
            }
        ]);

        var vm = new HandoverViewModel(_mockConfig.Object, _mockParser.Object, _mockFs.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.Handovers.Should().HaveCount(1);
        vm.SelectedHandover.Should().NotBeNull();
        vm.SelectedHandover!.DeveloperName.Should().Be("chris");
        vm.IsLoaded.Should().BeTrue();
    }

    [Test]
    public void LoadData_WhenDirectoryMissing_ShouldNotThrow()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(false);

        var vm = new HandoverViewModel(_mockConfig.Object, _mockParser.Object, _mockFs.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.Handovers.Should().BeEmpty();
        vm.IsLoaded.Should().BeTrue();
        vm.HasNoHandovers.Should().BeTrue();
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverViewModel(null!, _mockParser.Object, _mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullHandoverParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverViewModel(_mockConfig.Object, null!, _mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("handoverParser");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverViewModel(_mockConfig.Object, _mockParser.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void LoadData_WhenMultipleHandovers_ShouldLoadAllAndSelectFirst()
    {
        // Arrange
        _mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        _mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(true);
        _mockParser.Setup(p => p.ParseAll(@"C:\root\handovers")).Returns(
        [
            new HandoverNotes
            {
                DeveloperName = "alice",
                LastUpdated = "2026-03-22",
                ForNextSession = ["Task A"],
                OpenThreads = ["Thread 1"]
            },
            new HandoverNotes
            {
                DeveloperName = "bob",
                LastUpdated = "2026-03-23",
                ForNextSession = ["Task B", "Task C"],
                OpenThreads = ["Thread 2"]
            }
        ]);

        var vm = new HandoverViewModel(_mockConfig.Object, _mockParser.Object, _mockFs.Object);

        // Act
        vm.LoadData();

        // Assert
        vm.Handovers.Should().HaveCount(2);
        vm.SelectedHandover.Should().NotBeNull();
        vm.SelectedHandover!.DeveloperName.Should().Be("alice");
        vm.IsLoaded.Should().BeTrue();
        vm.HasNoHandovers.Should().BeFalse();
    }
}
