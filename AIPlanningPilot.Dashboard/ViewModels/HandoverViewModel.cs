using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the handover notes view. Displays per-developer handover files
/// with their session notes, next-session items, and open threads.
/// </summary>
public partial class HandoverViewModel : ObservableObject
{
    private readonly IConfigurationService configurationService;
    private readonly IHandoverParser handoverParser;
    private readonly IFileSystemService fileSystemService;

    /// <summary>Gets all parsed handover notes.</summary>
    public ObservableCollection<HandoverNotes> Handovers { get; } = [];

    /// <summary>Gets or sets the currently selected handover.</summary>
    [ObservableProperty]
    private HandoverNotes? selectedHandover;

    /// <summary>Gets or sets whether data has been loaded.</summary>
    [ObservableProperty]
    private bool isLoaded;

    /// <summary>Gets or sets whether no handovers are available after loading.</summary>
    [ObservableProperty]
    private bool hasNoHandovers;

    /// <summary>
    /// Initializes a new instance of the <see cref="HandoverViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="handoverParser">Parser for handover files.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    public HandoverViewModel(
        IConfigurationService configurationService,
        IHandoverParser handoverParser,
        IFileSystemService fileSystemService)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.handoverParser = handoverParser ?? throw new ArgumentNullException(nameof(handoverParser));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Loads all handover notes from the handovers directory.
    /// </summary>
    [RelayCommand]
    public void LoadData()
    {
        var handoversDir = Path.Combine(configurationService.RestructuringRootPath, ParserConstants.PathHandovers);
        if (!fileSystemService.DirectoryExists(handoversDir))
        {
            IsLoaded = true;
            HasNoHandovers = true;
            return;
        }

        var handovers = handoverParser.ParseAll(handoversDir);
        Handovers.Clear();
        foreach (var handover in handovers)
        {
            Handovers.Add(handover);
        }

        IsLoaded = true;
        HasNoHandovers = Handovers.Count == 0;

        if (Handovers.Count > 0)
        {
            SelectedHandover = Handovers[0];
        }
    }
}
