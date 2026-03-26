using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="TreeViewViewModel"/>.
/// </summary>
[TestFixture]
public class TreeViewViewModelTests
{
    private Mock<IFileSystemService> mockFileSystemService = null!;
    private Mock<IConfigurationService> mockConfigService = null!;
    private Mock<INavigationService> mockNavigationService = null!;

    [SetUp]
    public void SetUp()
    {
        mockFileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        mockConfigService = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockNavigationService = new Mock<INavigationService>(MockBehavior.Strict);
    }

    [TearDown]
    public void TearDown()
    {
        mockFileSystemService.VerifyAll();
        mockConfigService.VerifyAll();
        mockNavigationService.VerifyAll();
    }

    [Test]
    public void LoadTree_WhenDirectoryHasFiles_ShouldPopulateRootNodes()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        mockFileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true))
            .Returns(
            [
                new FileTreeNode { Name = "main", FullPath = $@"{rootPath}\main", IsDirectory = true, LastModified = DateTime.UtcNow },
                new FileTreeNode { Name = "Readme.md", FullPath = $@"{rootPath}\Readme.md", IsDirectory = false, LastModified = DateTime.UtcNow }
            ]);

        var viewModel = new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, mockNavigationService.Object);

        // Act
        viewModel.LoadTree();

        // Assert
        viewModel.RootNodes.Should().HaveCount(2);
        viewModel.RootNodes[0].Name.Should().Be("main");
        viewModel.RootNodes[0].IsDirectory.Should().BeTrue();
        viewModel.RootNodes[1].Name.Should().Be("Readme.md");
        viewModel.RootNodes[1].IsDirectory.Should().BeFalse();
    }

    [Test]
    public void LoadTree_WhenDirectoryIsEmpty_ShouldHaveEmptyRootNodes()
    {
        // Arrange
        var rootPath = @"C:\empty";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        mockFileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true))
            .Returns([]);

        var viewModel = new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, mockNavigationService.Object);

        // Act
        viewModel.LoadTree();

        // Assert
        viewModel.RootNodes.Should().BeEmpty();
    }

    [Test]
    public void Refresh_WhenCalled_ShouldReloadTree()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);

        // First load: 1 file
        mockFileSystemService.SetupSequence(fs => fs.GetDirectoryTree(rootPath, true))
            .Returns([new FileTreeNode { Name = "old.md", FullPath = $@"{rootPath}\old.md", IsDirectory = false, LastModified = DateTime.UtcNow }])
            .Returns([new FileTreeNode { Name = "new.md", FullPath = $@"{rootPath}\new.md", IsDirectory = false, LastModified = DateTime.UtcNow }]);

        var viewModel = new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, mockNavigationService.Object);
        viewModel.LoadTree();
        viewModel.RootNodes.Should().HaveCount(1);
        viewModel.RootNodes[0].Name.Should().Be("old.md");

        // Act
        viewModel.Refresh();

        // Assert
        viewModel.RootNodes.Should().HaveCount(1);
        viewModel.RootNodes[0].Name.Should().Be("new.md");
    }

    [Test]
    public void LoadTree_WhenFileNodeSelected_ShouldNavigateToFile()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        var filePath = $@"{rootPath}\STATE.md";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        mockFileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true))
            .Returns([new FileTreeNode { Name = "STATE.md", FullPath = filePath, IsDirectory = false, LastModified = DateTime.UtcNow }]);
        mockNavigationService.Setup(n => n.NavigateToFile(filePath));

        var viewModel = new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, mockNavigationService.Object);
        viewModel.LoadTree();

        // Act
        viewModel.RootNodes[0].IsSelected = true;

        // Assert
        mockNavigationService.Verify(n => n.NavigateToFile(filePath), Times.Once);
    }

    [Test]
    public void LoadTree_WhenDirectoryNodeSelected_ShouldNotNavigate()
    {
        // Arrange
        var rootPath = @"C:\restructuring";
        mockConfigService.Setup(c => c.RestructuringRootPath).Returns(rootPath);
        mockFileSystemService.Setup(fs => fs.GetDirectoryTree(rootPath, true))
            .Returns([new FileTreeNode { Name = "main", FullPath = $@"{rootPath}\main", IsDirectory = true, LastModified = DateTime.UtcNow }]);

        var viewModel = new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, mockNavigationService.Object);
        viewModel.LoadTree();

        // Act
        viewModel.RootNodes[0].IsSelected = true;

        // Assert - NavigateToFile should NOT be called for directories
        mockNavigationService.Verify(n => n.NavigateToFile(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new TreeViewViewModel(null!, mockConfigService.Object, mockNavigationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new TreeViewViewModel(mockFileSystemService.Object, null!, mockNavigationService.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    [Test]
    public void Constructor_WhenNullNavigationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new TreeViewViewModel(mockFileSystemService.Object, mockConfigService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("navigationService");
    }
}
