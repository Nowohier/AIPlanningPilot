using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="MarkdownViewerViewModel"/>.
/// </summary>
[TestFixture]
public class MarkdownViewerViewModelTests
{
    private Mock<IFileSystemService> _mockFileSystemService = null!;
    private Mock<IMarkdownRenderer> _mockMarkdownRenderer = null!;

    [SetUp]
    public void SetUp()
    {
        _mockFileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
        _mockMarkdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
    }

    [TearDown]
    public void TearDown()
    {
        _mockFileSystemService.VerifyAll();
        _mockMarkdownRenderer.VerifyAll();
    }

    [Test]
    public void LoadFile_WhenValidMarkdown_ShouldSetDocumentAndPath()
    {
        // Arrange
        var filePath = @"C:\restructuring\main\STATE.md";
        var markdownContent = "# STATE.md\n\nSome content here.";
        var expectedHtml = "<html><body><h1>STATE.md</h1></body></html>";

        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns(markdownContent);
        _mockMarkdownRenderer.Setup(r => r.RenderMarkdown(markdownContent)).Returns(expectedHtml);

        var viewModel = new MarkdownViewerViewModel(_mockFileSystemService.Object, _mockMarkdownRenderer.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.RenderedHtml.Should().BeSameAs(expectedHtml);
        viewModel.CurrentFilePath.Should().Be(filePath);
    }

    [Test]
    public void LoadFile_WhenCalledMultipleTimes_ShouldUpdateDocument()
    {
        // Arrange
        var filePath1 = @"C:\file1.md";
        var filePath2 = @"C:\file2.md";
        var html1 = "<html><body><h1>File 1</h1></body></html>";
        var html2 = "<html><body><h1>File 2</h1></body></html>";

        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath1)).Returns("# File 1");
        _mockFileSystemService.Setup(fs => fs.ReadAllText(filePath2)).Returns("# File 2");
        _mockMarkdownRenderer.Setup(r => r.RenderMarkdown("# File 1")).Returns(html1);
        _mockMarkdownRenderer.Setup(r => r.RenderMarkdown("# File 2")).Returns(html2);

        var viewModel = new MarkdownViewerViewModel(_mockFileSystemService.Object, _mockMarkdownRenderer.Object);

        // Act
        viewModel.LoadFile(filePath1);
        viewModel.LoadFile(filePath2);

        // Assert
        viewModel.RenderedHtml.Should().BeSameAs(html2);
        viewModel.CurrentFilePath.Should().Be(filePath2);
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MarkdownViewerViewModel(null!, _mockMarkdownRenderer.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }

    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new MarkdownViewerViewModel(_mockFileSystemService.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("markdownRenderer");
    }
}
