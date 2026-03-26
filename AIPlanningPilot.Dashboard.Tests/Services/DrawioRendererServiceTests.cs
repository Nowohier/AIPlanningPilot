using FluentAssertions;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.Tests.Services;

/// <summary>
/// Unit tests for <see cref="DrawioRendererService"/>.
/// Verifies that the generated HTML document contains the expected structure,
/// scripts, styles, and properly escaped XML content.
/// </summary>
[TestFixture]
public class DrawioRendererServiceTests
{
    private DrawioRendererService sut = null!;

    /// <summary>
    /// Creates a fresh service instance before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        sut = new DrawioRendererService();
    }

    /// <summary>
    /// Verifies that the rendered output is a proper HTML document with a DOCTYPE declaration.
    /// </summary>
    [Test]
    public void RenderDrawio_WhenCalledWithXml_ShouldReturnHtmlDocument()
    {
        // Arrange
        var xml = "<mxfile><diagram>test</diagram></mxfile>";

        // Act
        var result = sut.RenderDrawio(xml);

        // Assert
        result.Should().Contain("<!DOCTYPE html>");
    }

    /// <summary>
    /// Verifies that the rendered output contains a div with the mxgraph CSS class
    /// required by the draw.io viewer.
    /// </summary>
    [Test]
    public void RenderDrawio_WhenCalledWithXml_ShouldContainMxgraphDiv()
    {
        // Arrange
        var xml = "<mxfile><diagram>test</diagram></mxfile>";

        // Act
        var result = sut.RenderDrawio(xml);

        // Assert
        result.Should().Contain("class=\"mxgraph\"");
    }

    /// <summary>
    /// Verifies that the rendered output includes a script reference to the
    /// draw.io viewer-static.min.js library.
    /// </summary>
    [Test]
    public void RenderDrawio_WhenCalledWithXml_ShouldContainViewerScript()
    {
        // Arrange
        var xml = "<mxfile><diagram>test</diagram></mxfile>";

        // Act
        var result = sut.RenderDrawio(xml);

        // Assert
        result.Should().Contain("viewer-static.min.js");
    }

    /// <summary>
    /// Verifies that XML content containing quotes and special characters is properly
    /// escaped for safe embedding in the data-mxgraph JSON attribute.
    /// </summary>
    [Test]
    public void RenderDrawio_WhenCalledWithXml_ShouldEscapeXmlContent()
    {
        // Arrange
        var xml = "<mxfile><diagram name=\"Page-1\" id=\"abc\">content with \"quotes\" & <tags></diagram></mxfile>";

        // Act
        var result = sut.RenderDrawio(xml);

        // Assert
        result.Should().Contain("data-mxgraph=");
        result.Should().NotContain("\"quotes\"", "XML quotes should be escaped in the JSON attribute");
        result.Should().Contain("\\u0026", "ampersands should be JSON-escaped");
    }

    /// <summary>
    /// Verifies that the rendered output contains the configured draw.io highlight color.
    /// </summary>
    [Test]
    public void RenderDrawio_WhenCalledWithXml_ShouldContainHighlightColor()
    {
        // Arrange
        var xml = "<mxfile><diagram>test</diagram></mxfile>";

        // Act
        var result = sut.RenderDrawio(xml);

        // Assert
        result.Should().Contain("#0066CC");
    }
}
