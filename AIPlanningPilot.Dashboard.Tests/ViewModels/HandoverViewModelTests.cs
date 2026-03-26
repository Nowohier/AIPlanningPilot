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
    private Mock<IConfigurationService> mockConfig = null!;
    private Mock<IHandoverParser> mockParser = null!;
    private Mock<IFileSystemService> mockFs = null!;

    [SetUp]
    public void SetUp()
    {
        mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockParser = new Mock<IHandoverParser>(MockBehavior.Strict);
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
    public void LoadData_WhenHandoversExist_ShouldPopulateAndSelectFirst()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(true);
        mockParser.Setup(p => p.ParseAll(@"C:\root\handovers")).Returns(
        [
            new HandoverNotes
            {
                DeveloperName = "chris",
                LastUpdated = "2026-03-23",
                ForNextSession = ["Update CLAUDE.md", "Implement hooks"],
                OpenThreads = ["AG Grid decision"]
            }
        ]);

        var vm = new HandoverViewModel(mockConfig.Object, mockParser.Object, mockFs.Object);

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
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(false);

        var vm = new HandoverViewModel(mockConfig.Object, mockParser.Object, mockFs.Object);

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
        var act = () => new HandoverViewModel(null!, mockParser.Object, mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullHandoverParser_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverViewModel(mockConfig.Object, null!, mockFs.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("handoverParser");
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new HandoverViewModel(mockConfig.Object, mockParser.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void LoadData_WhenMultipleHandovers_ShouldLoadAllAndSelectFirst()
    {
        // Arrange
        mockConfig.Setup(c => c.RestructuringRootPath).Returns(@"C:\root");
        mockFs.Setup(fs => fs.DirectoryExists(@"C:\root\handovers")).Returns(true);
        mockParser.Setup(p => p.ParseAll(@"C:\root\handovers")).Returns(
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

        var vm = new HandoverViewModel(mockConfig.Object, mockParser.Object, mockFs.Object);

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
