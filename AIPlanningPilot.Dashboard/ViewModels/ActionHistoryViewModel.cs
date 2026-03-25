using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the action history view. Displays completed actions
/// from archive/completed-actions.md in a chronological timeline.
/// </summary>
public partial class ActionHistoryViewModel : ObservableObject
{
    private readonly IConfigurationService configurationService;
    private readonly IActionHistoryParser actionHistoryParser;
    private readonly IFileSystemService fileSystemService;

    /// <summary>Gets the list of completed actions.</summary>
    public ObservableCollection<CompletedAction> CompletedActions { get; } = [];

    /// <summary>Gets or sets whether data has been loaded.</summary>
    [ObservableProperty]
    private bool isLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionHistoryViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="actionHistoryParser">Parser for completed actions.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    public ActionHistoryViewModel(
        IConfigurationService configurationService,
        IActionHistoryParser actionHistoryParser,
        IFileSystemService fileSystemService)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.actionHistoryParser = actionHistoryParser ?? throw new ArgumentNullException(nameof(actionHistoryParser));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Loads completed actions from archive/completed-actions.md.
    /// </summary>
    [RelayCommand]
    public void LoadData()
    {
        var actionsPath = Path.Combine(configurationService.RestructuringRootPath, ParserConstants.PathArchive, ParserConstants.FileCompletedActions);
        if (!fileSystemService.FileExists(actionsPath))
        {
            IsLoaded = true;
            return;
        }

        var actions = actionHistoryParser.Parse(actionsPath);
        CompletedActions.Clear();
        foreach (var action in actions)
        {
            CompletedActions.Add(action);
        }

        IsLoaded = true;
    }
}
