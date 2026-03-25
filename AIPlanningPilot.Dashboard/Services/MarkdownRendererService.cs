using System.IO;
using System.Reflection;
using Markdig;

namespace AIPlanningPilot.Dashboard.Services;

/// <summary>
/// Default implementation of <see cref="IMarkdownRenderer"/> using Markdig for HTML generation
/// and embedded CSS themes for styling. Produces complete HTML documents suitable for WebView2.
/// Large JS files are extracted to a temp directory and referenced via script src tags
/// to stay within WebView2's NavigateToString size limit.
/// </summary>
public class MarkdownRendererService : IMarkdownRenderer
{
    /// <summary>
    /// Configuration for a single markdown theme, pairing a CSS stylesheet with
    /// a highlight.js theme and an optional body CSS class.
    /// </summary>
    private sealed record ThemeConfig(string Css, string HighlightCss, string BodyClass);

    private static Dictionary<string, ThemeConfig>? themeMap;
    private static string? assetsDirectory;
    private static bool initialized;
    private static readonly object InitLock = new();

    /// <inheritdoc />
    public string AssetsDirectory => GetAssetsDirectory();

    private readonly MarkdownPipeline pipeline;
    private string selectedThemeName = ParserConstants.DefaultThemeName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownRendererService"/> class
    /// with a Markdig pipeline configured for advanced extensions.
    /// </summary>
    public MarkdownRendererService()
    {
        pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .UseDiagrams()
            .Build();

        EnsureInitialized();
    }

    /// <inheritdoc />
    public string[] AvailableThemes { get; } = [.. GetThemeMap().Keys];

    /// <inheritdoc />
    public string SelectedThemeName
    {
        get => selectedThemeName;
        set
        {
            if (value == selectedThemeName || !GetThemeMap().ContainsKey(value))
            {
                return;
            }

            selectedThemeName = value;
            ThemeChanged?.Invoke();
        }
    }

    /// <inheritdoc />
    public event Action? ThemeChanged;

    /// <inheritdoc />
    public string RenderMarkdown(string markdownText)
    {
        var htmlFragment = Markdown.ToHtml(markdownText, pipeline);
        return WrapHtmlFragment(htmlFragment);
    }

    /// <inheritdoc />
    public string WrapHtmlFragment(string htmlFragment)
    {
        var themeMap = GetThemeMap();
        var theme = themeMap[selectedThemeName];
        var bodyClass = string.IsNullOrEmpty(theme.BodyClass) ? "" : $" class=\"{theme.BodyClass}\"";

        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="color-scheme" content="light">
                <style>{{theme.Css}}</style>
                <style>{{theme.HighlightCss}}</style>
                <style>
                    body {
                        margin: 0;
                        padding: 24px;
                        box-sizing: border-box;
                    }
                </style>
            </head>
            <body{{bodyClass}}>
                {{htmlFragment}}
                <script src="{{WebViewConstants.AssetBaseUrl}}highlight.min.js"></script>
                <script src="{{WebViewConstants.AssetBaseUrl}}highlight-csharp.min.js"></script>
                <script src="{{WebViewConstants.AssetBaseUrl}}highlight-xml.min.js"></script>
                <script>hljs.highlightAll();</script>
                <script src="{{WebViewConstants.AssetBaseUrl}}mermaid.min.js"></script>
                <script>mermaid.default.initialize({ startOnLoad: true, theme: 'default' });</script>
            </body>
            </html>
            """;
    }

    /// <summary>
    /// Ensures embedded resources are extracted and CSS themes are loaded.
    /// Thread-safe and idempotent.
    /// </summary>
    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (initialized)
            {
                return;
            }

            assetsDirectory = Path.Combine(Path.GetTempPath(), "RestructuringDashboard", "assets");
            Directory.CreateDirectory(assetsDirectory);

            ExtractResourceToFile("highlight.min.js");
            ExtractResourceToFile("highlight-csharp.min.js");
            ExtractResourceToFile("highlight-xml.min.js");
            ExtractResourceToFile("mermaid.min.js");
            ExtractResourceToFile("viewer-static.min.js");

            var highlightGithubCss = LoadEmbeddedResource("highlight-github.css");
            var highlightGithubDarkCss = LoadEmbeddedResource("highlight-github-dark.css");

            themeMap = new Dictionary<string, ThemeConfig>
            {
                ["GitHub Light"] = new(LoadEmbeddedResource("github-markdown-light.css"), highlightGithubCss, "markdown-body"),
                ["GitHub Dark"] = new(LoadEmbeddedResource("github-markdown-dark.css"), highlightGithubDarkCss, "markdown-body"),
                ["Water Light"] = new(LoadEmbeddedResource("water-light.css"), highlightGithubCss, ""),
                ["Water Dark"] = new(LoadEmbeddedResource("water-dark.css"), highlightGithubDarkCss, ""),
                ["Pico"] = new(LoadEmbeddedResource("pico.css"), highlightGithubCss, "")
            };

            initialized = true;
        }
    }

    /// <summary>
    /// Gets the theme map, ensuring initialization has occurred.
    /// </summary>
    private static Dictionary<string, ThemeConfig> GetThemeMap()
    {
        EnsureInitialized();
        return themeMap!;
    }

    /// <summary>
    /// Gets the assets directory, ensuring initialization has occurred.
    /// </summary>
    private static string GetAssetsDirectory()
    {
        EnsureInitialized();
        return assetsDirectory!;
    }

    /// <summary>
    /// Extracts an embedded resource to the assets temp directory.
    /// </summary>
    private static void ExtractResourceToFile(string fileName)
    {
        var targetPath = Path.Combine(assetsDirectory!, fileName);
        if (File.Exists(targetPath))
        {
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AIPlanningPilot.Dashboard.Assets.Themes.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var fileStream = File.Create(targetPath);
        stream.CopyTo(fileStream);
    }

    /// <summary>
    /// Loads a text file from the embedded resources in the Assets/Themes directory.
    /// </summary>
    private static string LoadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"AIPlanningPilot.Dashboard.Assets.Themes.{fileName}";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
