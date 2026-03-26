using CommunityToolkit.Mvvm.ComponentModel;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the markdown viewer panel.
/// Loads a markdown file and renders it as an HTML string for display in a WebView2 control.
/// </summary>
public partial class MarkdownViewerViewModel : ObservableObject
{
    private readonly IFileSystemService fileSystemService;
    private readonly IMarkdownRenderer markdownRenderer;

    /// <summary>
    /// Gets or sets the rendered HTML string for the currently loaded markdown file.
    /// </summary>
    [ObservableProperty]
    private string? renderedHtml;

    /// <summary>
    /// Gets or sets the path of the currently loaded file.
    /// </summary>
    [ObservableProperty]
    private string? currentFilePath;

    /// <summary>
    /// Gets the directory path where WebView2 assets are extracted.
    /// </summary>
    public string AssetsDirectory => markdownRenderer.AssetsDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownViewerViewModel"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files from disk.</param>
    /// <param name="markdownRenderer">Service for converting markdown to HTML.</param>
    public MarkdownViewerViewModel(IFileSystemService fileSystemService, IMarkdownRenderer markdownRenderer)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
    }

    /// <summary>
    /// Loads and renders the specified markdown file.
    /// </summary>
    /// <param name="filePath">The absolute path to the markdown file.</param>
    public void LoadFile(string filePath)
    {
        CurrentFilePath = filePath;
        var content = fileSystemService.ReadAllText(filePath);
        RenderedHtml = markdownRenderer.RenderMarkdown(content);
    }

    /// <summary>
    /// Displays pre-rendered HTML content for non-markdown files (e.g., .docx, .drawio).
    /// </summary>
    /// <param name="html">The complete HTML document string to display.</param>
    /// <param name="filePath">The path of the source file.</param>
    public void LoadHtml(string html, string filePath)
    {
        CurrentFilePath = filePath;
        RenderedHtml = html;
    }

    /// <summary>
    /// Re-renders the currently loaded file, if any.
    /// Used when the markdown theme changes.
    /// </summary>
    public void ReloadCurrentFile()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath))
        {
            LoadFile(CurrentFilePath);
        }
    }
}
