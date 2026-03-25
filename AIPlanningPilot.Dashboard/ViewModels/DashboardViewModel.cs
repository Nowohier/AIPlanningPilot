using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the dashboard view. Orchestrates parsers to present
/// a consolidated view of the restructuring project state.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IConfigurationService configurationService;
    private readonly IStateParser stateParser;
    private readonly IHandoverParser handoverParser;
    private readonly IFileSystemService fileSystemService;
    private readonly INavigationService navigationService;

    // -- State data --

    /// <summary>Gets or sets the current project phase.</summary>
    [ObservableProperty]
    private string currentPhase = string.Empty;

    /// <summary>Gets or sets the current day number.</summary>
    [ObservableProperty]
    private int currentDay;

    /// <summary>Gets or sets the last updated date.</summary>
    [ObservableProperty]
    private string lastUpdated = string.Empty;

    /// <summary>Gets or sets the active branch name.</summary>
    [ObservableProperty]
    private string branch = string.Empty;

    /// <summary>Gets the next actions list.</summary>
    public ObservableCollection<ActionItem> NextActions { get; } = [];

    /// <summary>Gets the phase progress list.</summary>
    public ObservableCollection<PhaseProgressItem> PhaseProgress { get; } = [];

    /// <summary>Gets the open decisions list.</summary>
    public ObservableCollection<OpenDecision> OpenDecisions { get; } = [];

    /// <summary>Gets or sets the count of open decisions.</summary>
    [ObservableProperty]
    private int openDecisionCount;

    /// <summary>Gets or sets the count of pending/next actions (not Done).</summary>
    [ObservableProperty]
    private int pendingActionCount;

    /// <summary>Gets or sets the currently selected phase in the phase list.</summary>
    [ObservableProperty]
    private PhaseProgressItem? selectedPhase;

    // -- Handover data --

    /// <summary>Gets the handover "For Next Session" items.</summary>
    public ObservableCollection<string> HandoverNextItems { get; } = [];

    /// <summary>Gets or sets the handover developer name.</summary>
    [ObservableProperty]
    private string handoverDeveloper = string.Empty;

    // -- Team --

    /// <summary>Gets the team members.</summary>
    public ObservableCollection<string> TeamMembers { get; } = [];

    /// <summary>Gets or sets whether data has been loaded successfully.</summary>
    [ObservableProperty]
    private bool isLoaded;

    /// <summary>Gets or sets the error message if loading fails.</summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="stateParser">Parser for STATE.md.</param>
    /// <param name="handoverParser">Parser for handover files.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    /// <param name="navigationService">Service for navigating to files.</param>
    public DashboardViewModel(
        IConfigurationService configurationService,
        IStateParser stateParser,
        IHandoverParser handoverParser,
        IFileSystemService fileSystemService,
        INavigationService navigationService)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.stateParser = stateParser ?? throw new ArgumentNullException(nameof(stateParser));
        this.handoverParser = handoverParser ?? throw new ArgumentNullException(nameof(handoverParser));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// Loads all dashboard data from the restructuring files.
    /// </summary>
    [RelayCommand]
    public void LoadData()
    {
        try
        {
            ErrorMessage = null;
            var root = configurationService.RestructuringRootPath;

            LoadState(root);
            LoadHandovers(root);

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load dashboard data: {ex.Message}";
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Opens the plan document for the given phase (or the selected phase) in the file viewer.
    /// </summary>
    /// <param name="phase">The phase to open. If null, uses <see cref="SelectedPhase"/>.</param>
    [RelayCommand]
    public void OpenPhaseDocument(PhaseProgressItem? phase)
    {
        var target = phase ?? SelectedPhase;
        if (target?.PlanFilePath is not null)
        {
            navigationService.NavigateToFile(target.PlanFilePath);
        }
    }

    /// <summary>
    /// Loads and parses STATE.md data.
    /// </summary>
    private void LoadState(string root)
    {
        var statePath = Path.Combine(root, ParserConstants.PathStateMd, ParserConstants.FileStateMd);
        if (!fileSystemService.FileExists(statePath))
        {
            return;
        }

        var state = stateParser.Parse(statePath);
        CurrentPhase = state.CurrentPhase;
        CurrentDay = state.Day;
        LastUpdated = state.LastUpdated;
        Branch = state.Branch;

        ReplaceAll(NextActions, state.NextActions);
        PendingActionCount = state.NextActions.Count(a => a.Status != ActionStatus.Done);

        ReplaceAll(PhaseProgress, state.PhaseProgress);

        ReplaceAll(OpenDecisions, state.OpenDecisions);
        OpenDecisionCount = state.OpenDecisions.Count;

        ReplaceAll(TeamMembers, state.TeamMembers);

        // Auto-select first phase
        SelectedPhase = PhaseProgress.FirstOrDefault();
    }

    /// <summary>
    /// Loads handover notes, showing the first developer's "For Next Session" items.
    /// </summary>
    private void LoadHandovers(string root)
    {
        var handoversDir = Path.Combine(root, ParserConstants.PathHandovers);
        if (!fileSystemService.DirectoryExists(handoversDir))
        {
            return;
        }

        var handovers = handoverParser.ParseAll(handoversDir);
        HandoverNextItems.Clear();

        if (handovers.Count > 0)
        {
            var primary = handovers[0];
            HandoverDeveloper = primary.DeveloperName.ToUpperInvariant();
            ReplaceAll(HandoverNextItems, primary.ForNextSession);
        }
    }

    /// <summary>
    /// Replaces all items in the target collection with items from the source.
    /// </summary>
    private static void ReplaceAll<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
