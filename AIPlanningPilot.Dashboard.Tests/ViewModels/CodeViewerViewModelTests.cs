using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="CodeViewerViewModel"/>.
/// </summary>
[TestFixture]
public class CodeViewerViewModelTests
{
    private Mock<IFileSystemService> mockFileSystemService = null!;

    [SetUp]
    public void SetUp()
    {
        mockFileSystemService = new Mock<IFileSystemService>(MockBehavior.Strict);
    }

    [TearDown]
    public void TearDown()
    {
        mockFileSystemService.VerifyAll();
    }

    [Test]
    public void LoadFile_WhenJavaScriptFile_ShouldSetJavaScriptHighlighting()
    {
        // Arrange
        var filePath = @"C:\scripts\sync-claude.mjs";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("export function sync() {}");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.Content.Should().Be("export function sync() {}");
        viewModel.SyntaxHighlighting.Should().Be("JavaScript");
        viewModel.CurrentFilePath.Should().Be(filePath);
    }

    [Test]
    public void LoadFile_WhenJsonFile_ShouldSetJsonHighlighting()
    {
        // Arrange
        var filePath = @"C:\settings.json";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("{}");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.SyntaxHighlighting.Should().Be("Json");
    }

    [Test]
    public void LoadFile_WhenShellScript_ShouldSetNullHighlighting()
    {
        // Arrange
        var filePath = @"C:\tests\run-tests.sh";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("#!/bin/bash");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.Content.Should().Be("#!/bin/bash");
        viewModel.SyntaxHighlighting.Should().BeNull();
    }

    [Test]
    public void LoadFile_WhenXmlFile_ShouldSetXmlHighlighting()
    {
        // Arrange
        var filePath = @"C:\project.csproj";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("<Project />");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.SyntaxHighlighting.Should().Be("XML");
    }

    [Test]
    public void LoadFile_WhenCSharpFile_ShouldSetCSharpHighlighting()
    {
        // Arrange
        var filePath = @"C:\Program.cs";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("class Foo {}");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.SyntaxHighlighting.Should().Be("C#");
    }

    [Test]
    public void LoadFile_WhenUnknownExtension_ShouldSetNullHighlighting()
    {
        // Arrange
        var filePath = @"C:\file.txt";
        mockFileSystemService.Setup(fs => fs.ReadAllText(filePath)).Returns("plain text");

        var viewModel = new CodeViewerViewModel(mockFileSystemService.Object);

        // Act
        viewModel.LoadFile(filePath);

        // Assert
        viewModel.SyntaxHighlighting.Should().BeNull();
    }

    [Test]
    public void Constructor_WhenNullFileSystemService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new CodeViewerViewModel(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("fileSystemService");
    }
}
