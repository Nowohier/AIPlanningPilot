using FluentAssertions;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="FileTreeNodeViewModel"/>.
/// </summary>
[TestFixture]
public class FileTreeNodeViewModelTests
{
    [Test]
    public void Constructor_WhenGivenFileNode_ShouldExposeModelProperties()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "STATE.md",
            FullPath = @"C:\restructuring\main\STATE.md",
            IsDirectory = false,
            LastModified = new DateTime(2026, 3, 23, 12, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.Name.Should().Be("STATE.md");
        viewModel.FullPath.Should().Be(@"C:\restructuring\main\STATE.md");
        viewModel.IsDirectory.Should().BeFalse();
        viewModel.Extension.Should().Be(".md");
        viewModel.Children.Should().BeEmpty();
    }

    [Test]
    public void Constructor_WhenGivenDirectoryNode_ShouldBuildChildViewModels()
    {
        // Arrange
        var childFile = new FileTreeNode
        {
            Name = "CONFIG.md",
            FullPath = @"C:\restructuring\main\CONFIG.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };
        var model = new FileTreeNode
        {
            Name = "main",
            FullPath = @"C:\restructuring\main",
            IsDirectory = true,
            LastModified = DateTime.UtcNow,
            Children = [childFile]
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.IsDirectory.Should().BeTrue();
        viewModel.Extension.Should().BeEmpty();
        viewModel.Children.Should().HaveCount(1);
        viewModel.Children[0].Name.Should().Be("CONFIG.md");
    }

    [Test]
    public void IsSelected_WhenSetToTrue_ShouldInvokeSelectionCallback()
    {
        // Arrange
        FileTreeNodeViewModel? selectedNode = null;
        var model = new FileTreeNode
        {
            Name = "overview.md",
            FullPath = @"C:\restructuring\plan\overview.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };
        var viewModel = new FileTreeNodeViewModel(model, node => selectedNode = node);

        // Act
        viewModel.IsSelected = true;

        // Assert
        selectedNode.Should().BeSameAs(viewModel);
    }

    [Test]
    public void IsSelected_WhenSetToFalse_ShouldNotInvokeSelectionCallback()
    {
        // Arrange
        FileTreeNodeViewModel? selectedNode = null;
        var model = new FileTreeNode
        {
            Name = "overview.md",
            FullPath = @"C:\restructuring\plan\overview.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };
        var viewModel = new FileTreeNodeViewModel(model, node => selectedNode = node);

        // Act
        viewModel.IsSelected = false;

        // Assert
        selectedNode.Should().BeNull();
    }

    [Test]
    public void IconKind_WhenMarkdownFile_ShouldReturnFileText()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "README.md",
            FullPath = @"C:\README.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.IconKind.Should().Be("FileText");
    }

    [Test]
    public void IconKind_WhenDirectory_ShouldReturnFolderSimple()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "main",
            FullPath = @"C:\restructuring\main",
            IsDirectory = true,
            LastModified = DateTime.UtcNow
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.IconKind.Should().Be("FolderSimple");
    }

    [Test]
    public void IconKind_WhenShellScript_ShouldReturnTerminal()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "run-tests.sh",
            FullPath = @"C:\tests\run-tests.sh",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.IconKind.Should().Be("Terminal");
    }

    [Test]
    public void IconKind_WhenJavaScriptFile_ShouldReturnFileJs()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "sync-claude.mjs",
            FullPath = @"C:\scripts\sync-claude.mjs",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(model);

        // Assert
        viewModel.IconKind.Should().Be("FileJs");
    }

    [Test]
    public void Constructor_WhenGivenNestedDirectoryStructure_ShouldBuildRecursively()
    {
        // Arrange
        var grandchild = new FileTreeNode
        {
            Name = "test.md",
            FullPath = @"C:\a\b\test.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };
        var child = new FileTreeNode
        {
            Name = "b",
            FullPath = @"C:\a\b",
            IsDirectory = true,
            LastModified = DateTime.UtcNow,
            Children = [grandchild]
        };
        var root = new FileTreeNode
        {
            Name = "a",
            FullPath = @"C:\a",
            IsDirectory = true,
            LastModified = DateTime.UtcNow,
            Children = [child]
        };

        // Act
        var viewModel = new FileTreeNodeViewModel(root);

        // Assert
        viewModel.Children.Should().HaveCount(1);
        viewModel.Children[0].Children.Should().HaveCount(1);
        viewModel.Children[0].Children[0].Name.Should().Be("test.md");
    }

    [Test]
    public void Constructor_WhenNoCallbackProvided_ShouldNotThrowOnSelection()
    {
        // Arrange
        var model = new FileTreeNode
        {
            Name = "file.md",
            FullPath = @"C:\file.md",
            IsDirectory = false,
            LastModified = DateTime.UtcNow
        };
        var viewModel = new FileTreeNodeViewModel(model);

        // Act
        var act = () => viewModel.IsSelected = true;

        // Assert
        act.Should().NotThrow();
    }
}
