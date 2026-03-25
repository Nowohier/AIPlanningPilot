using Mammoth;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IDocxRenderer"/> using Mammoth to convert
/// .docx documents to HTML. Images are embedded as base64 data URIs.
/// </summary>
public class DocxRendererService : IDocxRenderer
{
    private readonly IMarkdownRenderer markdownRenderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocxRendererService"/> class.
    /// </summary>
    /// <param name="markdownRenderer">The markdown renderer used for CSS theme wrapping.</param>
    public DocxRendererService(IMarkdownRenderer markdownRenderer)
    {
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
    }

    /// <inheritdoc />
    public string RenderDocx(string filePath)
    {
        var converter = new DocumentConverter();
        var result = converter.ConvertToHtml(filePath);
        return markdownRenderer.WrapHtmlFragment(result.Value);
    }
}
