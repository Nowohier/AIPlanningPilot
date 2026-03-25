using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the code viewer panel.
/// Loads source code files (.sh, .bat, .mjs, .js) and provides
/// the content and syntax highlighting mode for AvalonEdit.
/// </summary>
public partial class CodeViewerViewModel : ObservableObject
{
    private readonly IFileSystemService fileSystemService;

    /// <summary>
    /// Gets or sets the raw text content of the currently loaded file.
    /// </summary>
    [ObservableProperty]
    private string? content;

    /// <summary>
    /// Gets or sets the path of the currently loaded file.
    /// </summary>
    [ObservableProperty]
    private string? currentFilePath;

    /// <summary>
    /// Gets or sets the syntax highlighting mode name for AvalonEdit.
    /// </summary>
    [ObservableProperty]
    private string? syntaxHighlighting;

    /// <summary>
    /// Initializes a new instance of the <see cref="CodeViewerViewModel"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for reading files from disk.</param>
    public CodeViewerViewModel(IFileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Loads the specified code file and determines the appropriate syntax highlighting mode.
    /// </summary>
    /// <param name="filePath">The absolute path to the code file.</param>
    public void LoadFile(string filePath)
    {
        CurrentFilePath = filePath;
        Content = fileSystemService.ReadAllText(filePath);
        SyntaxHighlighting = GetSyntaxHighlightingMode(filePath);
    }

    /// <summary>
    /// Determines the AvalonEdit syntax highlighting mode based on the file extension.
    /// </summary>
    /// <param name="filePath">The file path to inspect.</param>
    /// <returns>The syntax highlighting mode name, or <c>null</c> for plain text.</returns>
    protected static string? GetSyntaxHighlightingMode(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".js" or ".mjs" => "JavaScript",
            ".json" => "Json",
            ".xml" or ".xaml" or ".csproj" or ".slnx" => "XML",
            ".cs" => "C#",
            ".sh" or ".bat" or ".cmd" => null, // No built-in highlighting for shell
            _ => null
        };
    }
}
