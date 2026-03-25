using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the decision tracker view. Displays a list of all ADRs
/// with a detail panel showing the full rendered markdown for the selected decision.
/// </summary>
public partial class DecisionTrackerViewModel : ObservableObject
{
    private readonly IConfigurationService configurationService;
    private readonly IDecisionParser decisionParser;
    private readonly IFileSystemService fileSystemService;
    private readonly IMarkdownRenderer markdownRenderer;

    /// <summary>Gets the list of all parsed decisions.</summary>
    public ObservableCollection<Decision> Decisions { get; } = [];

    /// <summary>Gets or sets the currently selected decision.</summary>
    [ObservableProperty]
    private Decision? selectedDecision;

    /// <summary>Gets or sets the rendered HTML for the selected decision.</summary>
    [ObservableProperty]
    private string? selectedDecisionHtml;

    /// <summary>Gets or sets whether data has been loaded.</summary>
    [ObservableProperty]
    private bool isLoaded;

    /// <summary>Gets or sets whether the decisions list is empty after loading.</summary>
    [ObservableProperty]
    private bool hasNoDecisions;

    /// <summary>Gets or sets the error message if loading fails.</summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="DecisionTrackerViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="decisionParser">Parser for ADR files.</param>
    /// <param name="fileSystemService">Service for reading files.</param>
    /// <param name="markdownRenderer">Service for rendering markdown.</param>
    public DecisionTrackerViewModel(
        IConfigurationService configurationService,
        IDecisionParser decisionParser,
        IFileSystemService fileSystemService,
        IMarkdownRenderer markdownRenderer)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.decisionParser = decisionParser ?? throw new ArgumentNullException(nameof(decisionParser));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
    }

    /// <summary>
    /// Loads all decisions from the decisions directory.
    /// </summary>
    [RelayCommand]
    public void LoadData()
    {
        try
        {
            ErrorMessage = null;

            var decisionsDir = Path.Combine(configurationService.RestructuringRootPath, ParserConstants.PathDecisions);
            if (!fileSystemService.DirectoryExists(decisionsDir))
            {
                IsLoaded = true;
                HasNoDecisions = true;
                return;
            }

            var decisions = decisionParser.ParseAll(decisionsDir);
            Decisions.Clear();
            foreach (var decision in decisions)
            {
                Decisions.Add(decision);
            }

            IsLoaded = true;
            HasNoDecisions = Decisions.Count == 0;

            if (Decisions.Count > 0)
            {
                SelectedDecision = Decisions[0];
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load decisions: {ex.Message}";
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Renders the newly selected decision when the selection changes.
    /// </summary>
    partial void OnSelectedDecisionChanged(Decision? value)
    {
        RenderDecision(value);
    }

    /// <summary>
    /// Re-renders the currently selected decision, if any.
    /// Used when the markdown theme changes.
    /// </summary>
    public void ReRenderSelectedDecision()
    {
        RenderDecision(SelectedDecision);
    }

    /// <summary>
    /// Renders the specified decision's markdown file into an HTML string.
    /// </summary>
    /// <param name="decision">The decision to render, or <c>null</c> to clear the content.</param>
    private void RenderDecision(Decision? decision)
    {
        if (decision is null || string.IsNullOrEmpty(decision.FilePath))
        {
            SelectedDecisionHtml = null;
            return;
        }

        if (fileSystemService.FileExists(decision.FilePath))
        {
            var content = fileSystemService.ReadAllText(decision.FilePath);
            SelectedDecisionHtml = markdownRenderer.RenderMarkdown(content);
        }
    }
}
