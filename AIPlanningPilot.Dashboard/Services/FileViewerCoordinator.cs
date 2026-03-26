using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IFileViewerCoordinator"/>.
/// Dispatches files to the markdown viewer, code viewer, docx renderer,
/// or drawio renderer based on the file extension.
/// </summary>
public class FileViewerCoordinator : IFileViewerCoordinator
{
    private readonly IFileSystemService fileSystemService;
    private readonly IMarkdownRenderer markdownRenderer;
    private readonly IDocxRenderer docxRenderer;
    private readonly IDrawioRenderer drawioRenderer;
    private readonly MarkdownViewerViewModel markdownViewer;
    private readonly CodeViewerViewModel codeViewer;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileViewerCoordinator"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for file system access.</param>
    /// <param name="markdownRenderer">Service for rendering markdown to HTML.</param>
    /// <param name="docxRenderer">Service for rendering .docx files to HTML.</param>
    /// <param name="drawioRenderer">Service for rendering .drawio files to HTML.</param>
    /// <param name="markdownViewer">The markdown viewer ViewModel.</param>
    /// <param name="codeViewer">The code viewer ViewModel.</param>
    public FileViewerCoordinator(
        IFileSystemService fileSystemService,
        IMarkdownRenderer markdownRenderer,
        IDocxRenderer docxRenderer,
        IDrawioRenderer drawioRenderer,
        MarkdownViewerViewModel markdownViewer,
        CodeViewerViewModel codeViewer)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
        this.docxRenderer = docxRenderer ?? throw new ArgumentNullException(nameof(docxRenderer));
        this.drawioRenderer = drawioRenderer ?? throw new ArgumentNullException(nameof(drawioRenderer));
        this.markdownViewer = markdownViewer ?? throw new ArgumentNullException(nameof(markdownViewer));
        this.codeViewer = codeViewer ?? throw new ArgumentNullException(nameof(codeViewer));
    }

    /// <inheritdoc />
    public ObservableObject? OpenFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !fileSystemService.FileExists(filePath))
        {
            return null;
        }

        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        switch (extension)
        {
            case ".md":
                markdownViewer.LoadFile(filePath);
                return markdownViewer;
            case ".docx":
                markdownViewer.LoadHtml(docxRenderer.RenderDocx(filePath), filePath);
                return markdownViewer;
            case ".drawio":
                var drawioXml = fileSystemService.ReadAllText(filePath);
                markdownViewer.LoadHtml(drawioRenderer.RenderDrawio(drawioXml), filePath);
                return markdownViewer;
            default:
                codeViewer.LoadFile(filePath);
                return codeViewer;
        }
    }
}
