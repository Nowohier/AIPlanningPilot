using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the main application window. Coordinates the file tree,
/// content viewer, and navigation between Dashboard, Decisions, Handover, Migration, and File views.
/// </summary>
public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private const string DefaultTitlePrefix = "AI Planning Pilot";
    private const string TimeFormat = "HH:mm";

    private readonly string defaultTitle;

    private readonly INavigationService navigationService;
    private readonly IFileSystemService fileSystemService;
    private readonly IMarkdownRenderer markdownRenderer;
    private readonly IConfigurationService configurationService;
    private readonly IFileWatcherService fileWatcherService;
    private readonly IFileViewerCoordinator fileViewerCoordinator;
    private readonly IDialogService dialogService;

    /// <summary>
    /// Gets the tree view ViewModel for the left panel.
    /// </summary>
    public TreeViewViewModel TreeView { get; }

    /// <summary>
    /// Gets the markdown viewer ViewModel.
    /// </summary>
    public MarkdownViewerViewModel MarkdownViewer { get; }

    /// <summary>
    /// Gets the code viewer ViewModel.
    /// </summary>
    public CodeViewerViewModel CodeViewer { get; }

    /// <summary>
    /// Gets the dashboard ViewModel.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// Gets the decision tracker ViewModel.
    /// </summary>
    public DecisionTrackerViewModel DecisionTracker { get; }

    /// <summary>
    /// Gets the handover ViewModel.
    /// </summary>
    public HandoverViewModel Handover { get; }

    /// <summary>
    /// Gets the action history ViewModel.
    /// </summary>
    public ActionHistoryViewModel ActionHistory { get; }

    /// <summary>
    /// Gets the search ViewModel.
    /// </summary>
    public SearchViewModel Search { get; }

    /// <summary>
    /// Gets the migration tracker ViewModel.
    /// </summary>
    public MigrationTrackerViewModel MigrationTracker { get; }

    /// <summary>
    /// Gets or sets the currently active content ViewModel (markdown or code viewer).
    /// </summary>
    [ObservableProperty]
    private ObservableObject? activeContentViewModel;

    /// <summary>
    /// Gets or sets the currently selected view in the main content area.
    /// </summary>
    [ObservableProperty]
    private ActiveView selectedView = ActiveView.Dashboard;

    /// <summary>
    /// Gets or sets the path of the currently displayed file.
    /// </summary>
    [ObservableProperty]
    private string currentFilePath = string.Empty;

    /// <summary>
    /// Gets or sets the title displayed in the window title bar.
    /// </summary>
    [ObservableProperty]
    private string title = DefaultTitlePrefix;

    /// <summary>
    /// Gets the total number of files tracked in the restructuring directory.
    /// </summary>
    [ObservableProperty]
    private int fileCount;

    /// <summary>
    /// Gets or sets the time of the last data refresh for display in the status bar.
    /// </summary>
    [ObservableProperty]
    private string lastRefreshedTime = string.Empty;

    /// <summary>
    /// Gets or sets whether the Migration tab button is visible (MIGRATION.md exists).
    /// </summary>
    [ObservableProperty]
    private bool isMigrationAvailable;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="navigationService">Service for coordinating file navigation.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    /// <param name="markdownRenderer">Service for rendering markdown.</param>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="dashboardViewModel">The dashboard ViewModel.</param>
    /// <param name="decisionTrackerViewModel">The decision tracker ViewModel.</param>
    /// <param name="handoverViewModel">The handover ViewModel.</param>
    /// <param name="actionHistoryViewModel">The action history ViewModel.</param>
    /// <param name="searchViewModel">The search ViewModel.</param>
    /// <param name="migrationTrackerViewModel">The migration tracker ViewModel.</param>
    /// <param name="fileWatcherService">Service for watching file changes.</param>
    /// <param name="fileViewerCoordinator">Service for dispatching files to the appropriate viewer.</param>
    /// <param name="dialogService">Service for showing application dialogs.</param>
    /// <param name="treeViewViewModel">The tree view ViewModel for the left panel.</param>
    /// <param name="markdownViewerViewModel">The markdown viewer ViewModel.</param>
    /// <param name="codeViewerViewModel">The code viewer ViewModel.</param>
    public MainWindowViewModel(
        INavigationService navigationService,
        IFileSystemService fileSystemService,
        IMarkdownRenderer markdownRenderer,
        IConfigurationService configurationService,
        DashboardViewModel dashboardViewModel,
        DecisionTrackerViewModel decisionTrackerViewModel,
        HandoverViewModel handoverViewModel,
        ActionHistoryViewModel actionHistoryViewModel,
        SearchViewModel searchViewModel,
        MigrationTrackerViewModel migrationTrackerViewModel,
        IFileWatcherService fileWatcherService,
        IFileViewerCoordinator fileViewerCoordinator,
        IDialogService dialogService,
        TreeViewViewModel treeViewViewModel,
        MarkdownViewerViewModel markdownViewerViewModel,
        CodeViewerViewModel codeViewerViewModel)
    {
        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.markdownRenderer = markdownRenderer ?? throw new ArgumentNullException(nameof(markdownRenderer));
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
        this.fileViewerCoordinator = fileViewerCoordinator ?? throw new ArgumentNullException(nameof(fileViewerCoordinator));
        this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        Dashboard = dashboardViewModel ?? throw new ArgumentNullException(nameof(dashboardViewModel));
        DecisionTracker = decisionTrackerViewModel ?? throw new ArgumentNullException(nameof(decisionTrackerViewModel));
        Handover = handoverViewModel ?? throw new ArgumentNullException(nameof(handoverViewModel));
        ActionHistory = actionHistoryViewModel ?? throw new ArgumentNullException(nameof(actionHistoryViewModel));
        Search = searchViewModel ?? throw new ArgumentNullException(nameof(searchViewModel));
        MigrationTracker = migrationTrackerViewModel ?? throw new ArgumentNullException(nameof(migrationTrackerViewModel));

        TreeView = treeViewViewModel ?? throw new ArgumentNullException(nameof(treeViewViewModel));
        MarkdownViewer = markdownViewerViewModel ?? throw new ArgumentNullException(nameof(markdownViewerViewModel));
        CodeViewer = codeViewerViewModel ?? throw new ArgumentNullException(nameof(codeViewerViewModel));

        this.navigationService.FileSelected += OnFileSelected;
        this.markdownRenderer.ThemeChanged += OnMarkdownThemeChanged;
        this.fileWatcherService.FileChanged += OnFileWatcherChanged;

        defaultTitle = string.IsNullOrEmpty(this.configurationService.ProjectName)
            ? DefaultTitlePrefix
            : $"{DefaultTitlePrefix} - {this.configurationService.ProjectName}";
        Title = defaultTitle;
    }

    /// <summary>
    /// Initializes the application by loading the file tree, all child VM data,
    /// and starting the file watcher.
    /// </summary>
    [RelayCommand]
    public void Initialize()
    {
        TreeView.LoadTree();
        FileCount = CountFiles(TreeView.RootNodes);

        Dashboard.LoadData();
        DecisionTracker.LoadData();
        Handover.LoadData();

        var migrationPath = Path.Combine(
            configurationService.RestructuringRootPath,
            ParserConstants.PathStateMd,
            ParserConstants.FileMigrationMd);
        IsMigrationAvailable = fileSystemService.FileExists(migrationPath);
        if (IsMigrationAvailable)
        {
            MigrationTracker.LoadData();
        }

        fileWatcherService.Start(configurationService.RestructuringRootPath);

        LastRefreshedTime = DateTime.Now.ToString(TimeFormat);
    }

    /// <summary>
    /// Opens the settings window as a modal dialog.
    /// </summary>
    [RelayCommand]
    public void OpenSettings()
    {
        dialogService.ShowSettingsDialog();
    }

    /// <summary>
    /// Switches to the Dashboard view.
    /// </summary>
    [RelayCommand]
    public void NavigateToDashboard()
    {
        SelectedView = ActiveView.Dashboard;
        Search.HideOverlay();
    }

    /// <summary>
    /// Switches to the Decisions view.
    /// </summary>
    [RelayCommand]
    public void NavigateToDecisions()
    {
        SelectedView = ActiveView.Decisions;
        Search.HideOverlay();
    }

    /// <summary>
    /// Switches to the Handover view.
    /// </summary>
    [RelayCommand]
    public void NavigateToHandover()
    {
        SelectedView = ActiveView.Handover;
        Search.HideOverlay();
    }

    /// <summary>
    /// Switches to the Migration view.
    /// </summary>
    [RelayCommand]
    public void NavigateToMigration()
    {
        SelectedView = ActiveView.Migration;
        Search.HideOverlay();
    }

    /// <summary>
    /// Navigates to and displays the specified file in the appropriate viewer.
    /// </summary>
    /// <param name="filePath">The absolute path to the file to display.</param>
    public void NavigateToFile(string filePath)
    {
        var viewModel = fileViewerCoordinator.OpenFile(filePath);
        if (viewModel is null)
        {
            return;
        }

        CurrentFilePath = filePath;
        ActiveContentViewModel = viewModel;
        SelectedView = ActiveView.File;
        Search.HideOverlay();
        TreeView.RevealAndSelect(filePath);

        var relativePath = Path.GetRelativePath(configurationService.RestructuringRootPath, filePath);
        Title = $"{defaultTitle} - {relativePath}";
    }

    /// <summary>Handles the <see cref="INavigationService.FileSelected"/> event by navigating to the specified file.</summary>
    private void OnFileSelected(object? sender, string filePath)
    {
        NavigateToFile(filePath);
    }

    /// <summary>
    /// Handles the <see cref="IMarkdownRenderer.ThemeChanged"/> event
    /// by re-rendering all active markdown documents.
    /// </summary>
    private void OnMarkdownThemeChanged()
    {
        MarkdownViewer.ReloadCurrentFile();
        DecisionTracker.ReRenderSelectedDecision();
    }

    /// <summary>
    /// Handles file watcher changes by refreshing the tree and reloading the current file if it changed.
    /// </summary>
    private void OnFileWatcherChanged(string changedFilePath)
    {
        TreeView.Refresh();
        FileCount = CountFiles(TreeView.RootNodes);

        Dashboard.LoadData();
        DecisionTracker.LoadData();
        Handover.LoadData();
        MigrationTracker.LoadData();

        LastRefreshedTime = DateTime.Now.ToString(TimeFormat);

        if (!string.IsNullOrEmpty(CurrentFilePath) &&
            string.Equals(CurrentFilePath, changedFilePath, StringComparison.OrdinalIgnoreCase))
        {
            NavigateToFile(CurrentFilePath);
        }
    }

    /// <summary>
    /// Recursively counts all file nodes in the tree.
    /// </summary>
    private static int CountFiles(IEnumerable<FileTreeNodeViewModel> nodes)
    {
        var count = 0;
        foreach (var node in nodes)
        {
            if (node.IsDirectory)
            {
                count += CountFiles(node.Children);
            }
            else
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Unsubscribes from all event handlers to prevent memory leaks.
    /// </summary>
    public void Dispose()
    {
        navigationService.FileSelected -= OnFileSelected;
        markdownRenderer.ThemeChanged -= OnMarkdownThemeChanged;
        fileWatcherService.FileChanged -= OnFileWatcherChanged;
        GC.SuppressFinalize(this);
    }
}
