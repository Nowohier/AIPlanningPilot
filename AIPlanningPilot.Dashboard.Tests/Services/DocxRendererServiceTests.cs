using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DocxRendererService"/>.
/// Note: Mammoth's <c>DocumentConverter</c> cannot be mocked and requires a real .docx file,
/// so these tests focus on constructor validation and guard clauses.
/// </summary>
[TestFixture]
public class DocxRendererServiceTests
{
    private Mock<IMarkdownRenderer> mockMarkdownRenderer = null!;

    /// <summary>
    /// Initializes mocks before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        mockMarkdownRenderer = new Mock<IMarkdownRenderer>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all mock expectations after each test.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        mockMarkdownRenderer.VerifyAll();
    }

    /// <summary>
    /// Verifies that the constructor throws <see cref="ArgumentNullException"/>
    /// when a null markdown renderer is provided.
    /// </summary>
    [Test]
    public void Constructor_WhenNullMarkdownRenderer_ShouldThrow()
    {
        // Arrange
        IMarkdownRenderer nullRenderer = null!;

        // Act
        var act = () => new DocxRendererService(nullRenderer);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("markdownRenderer");
    }

    /// <summary>
    /// Verifies that the constructor succeeds when a valid markdown renderer is provided.
    /// </summary>
    [Test]
    public void Constructor_WhenValidMarkdownRenderer_ShouldNotThrow()
    {
        // Arrange & Act
        var act = () => new DocxRendererService(mockMarkdownRenderer.Object);

        // Assert
        act.Should().NotThrow();
    }
}
