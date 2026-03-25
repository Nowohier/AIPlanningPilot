using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the migration tracker view. Parses MIGRATION.md and presents
/// entity migration progress with KPIs, domain groups, and filtering.
/// </summary>
public partial class MigrationTrackerViewModel : ObservableObject
{
    private const string FilterAll = "All";
    private const string FilterNotStarted = "Not Started";
    private const string FilterInProgress = "In Progress";
    private const string FilterDone = "Done";
    private const string FilterSkipped = "Skipped";
    private const string FilterSimple = "Simple";
    private const string FilterMedium = "Medium";
    private const string FilterComplex = "Complex";
    private const string FilterVeryComplex = "Very Complex";

    private readonly IConfigurationService configurationService;
    private readonly IMigrationParser migrationParser;
    private readonly IFileSystemService fileSystemService;

    private List<MigrationEntity> allEntities = [];

    // -- KPI properties --

    /// <summary>Gets or sets the total number of entities with frontend UI.</summary>
    [ObservableProperty]
    private int totalEntitiesWithUi;

    /// <summary>Gets or sets the count of migrated entities.</summary>
    [ObservableProperty]
    private int migratedCount;

    /// <summary>Gets or sets the count of entities currently being migrated.</summary>
    [ObservableProperty]
    private int inProgressCount;

    /// <summary>Gets or sets the count of entities not yet started.</summary>
    [ObservableProperty]
    private int notStartedCount;

    /// <summary>Gets or sets the count of entities with manual code overrides.</summary>
    [ObservableProperty]
    private int manualOverrideCount;

    /// <summary>Gets or sets the overall migration progress percentage.</summary>
    [ObservableProperty]
    private double overallProgressPercent;

    // -- Complexity tier counts --

    /// <summary>Gets or sets the count of simple-tier entities.</summary>
    [ObservableProperty]
    private int simpleTierCount;

    /// <summary>Gets or sets the count of medium-tier entities.</summary>
    [ObservableProperty]
    private int mediumTierCount;

    /// <summary>Gets or sets the count of complex-tier entities.</summary>
    [ObservableProperty]
    private int complexTierCount;

    /// <summary>Gets or sets the count of very-complex-tier entities.</summary>
    [ObservableProperty]
    private int veryComplexTierCount;

    // -- Progress bar segments --

    /// <summary>Gets or sets the done percentage for the segmented progress bar.</summary>
    [ObservableProperty]
    private double donePercent;

    /// <summary>Gets or sets the in-progress percentage for the segmented progress bar.</summary>
    [ObservableProperty]
    private double inProgressPercent;

    /// <summary>Gets or sets the remaining percentage for the segmented progress bar.</summary>
    [ObservableProperty]
    private double remainingPercent = 100;

    // -- Collections --

    /// <summary>Gets the domain groups for the domain card view.</summary>
    public ObservableCollection<DomainMigrationGroup> DomainGroups { get; } = [];

    // -- Filter state --

    /// <summary>Gets or sets the search text for filtering entities.</summary>
    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>Gets or sets the selected status filter.</summary>
    [ObservableProperty]
    private string selectedStatusFilter = FilterAll;

    /// <summary>Gets or sets the selected complexity filter.</summary>
    [ObservableProperty]
    private string selectedComplexityFilter = FilterAll;

    /// <summary>Gets the available status filter options.</summary>
    public static string[] StatusFilters { get; } = [FilterAll, FilterNotStarted, FilterInProgress, FilterDone, FilterSkipped];

    /// <summary>Gets the available complexity filter options.</summary>
    public static string[] ComplexityFilters { get; } = [FilterAll, FilterSimple, FilterMedium, FilterComplex, FilterVeryComplex];

    // -- Loading state --

    /// <summary>Gets or sets whether data has been loaded successfully.</summary>
    [ObservableProperty]
    private bool isLoaded;

    /// <summary>Gets or sets whether the MIGRATION.md file exists.</summary>
    [ObservableProperty]
    private bool hasMigrationFile;

    /// <summary>Gets or sets the error message if loading fails.</summary>
    [ObservableProperty]
    private string? errorMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationTrackerViewModel"/> class.
    /// </summary>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="migrationParser">Parser for MIGRATION.md.</param>
    /// <param name="fileSystemService">Service for file system access.</param>
    public MigrationTrackerViewModel(
        IConfigurationService configurationService,
        IMigrationParser migrationParser,
        IFileSystemService fileSystemService)
    {
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.migrationParser = migrationParser ?? throw new ArgumentNullException(nameof(migrationParser));
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
    }

    /// <summary>
    /// Loads all migration data from MIGRATION.md.
    /// </summary>
    [RelayCommand]
    public void LoadData()
    {
        try
        {
            ErrorMessage = null;
            var root = configurationService.RestructuringRootPath;
            var migrationPath = Path.Combine(root, ParserConstants.PathStateMd, ParserConstants.FileMigrationMd);

            if (!fileSystemService.FileExists(migrationPath))
            {
                HasMigrationFile = false;
                IsLoaded = true;
                return;
            }

            HasMigrationFile = true;
            allEntities = migrationParser.Parse(migrationPath);

            ComputeKpis();
            ApplyFilters();

            IsLoaded = true;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load migration data: {ex.Message}";
            IsLoaded = false;
        }
    }

    /// <summary>
    /// Reapplies filters when the search text changes.
    /// </summary>
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    /// <summary>
    /// Reapplies filters when the status filter changes.
    /// </summary>
    partial void OnSelectedStatusFilterChanged(string value) => ApplyFilters();

    /// <summary>
    /// Reapplies filters when the complexity filter changes.
    /// </summary>
    partial void OnSelectedComplexityFilterChanged(string value) => ApplyFilters();

    /// <summary>
    /// Computes all KPI values from the full entity list.
    /// </summary>
    private void ComputeKpis()
    {
        var total = allEntities.Count;
        var statusCounts = allEntities.GroupBy(e => e.Status)
            .ToDictionary(g => g.Key, g => g.Count());
        var complexityCounts = allEntities.GroupBy(e => e.Complexity)
            .ToDictionary(g => g.Key, g => g.Count());

        var done = statusCounts.GetValueOrDefault(MigrationStatus.Done);
        var inProgress = statusCounts.GetValueOrDefault(MigrationStatus.InProgress);

        TotalEntitiesWithUi = total;
        MigratedCount = done;
        InProgressCount = inProgress;
        NotStartedCount = statusCounts.GetValueOrDefault(MigrationStatus.NotStarted);
        ManualOverrideCount = allEntities.Count(e => e.HasManualCode);
        OverallProgressPercent = total > 0 ? Math.Round(done * 100.0 / total, 1) : 0;

        SimpleTierCount = complexityCounts.GetValueOrDefault(ComplexityTier.Simple);
        MediumTierCount = complexityCounts.GetValueOrDefault(ComplexityTier.Medium);
        ComplexTierCount = complexityCounts.GetValueOrDefault(ComplexityTier.Complex);
        VeryComplexTierCount = complexityCounts.GetValueOrDefault(ComplexityTier.VeryComplex);

        if (total > 0)
        {
            DonePercent = done * 100.0 / total;
            InProgressPercent = inProgress * 100.0 / total;
            RemainingPercent = (total - done - inProgress) * 100.0 / total;
        }
        else
        {
            DonePercent = 0;
            InProgressPercent = 0;
            RemainingPercent = 100;
        }
    }

    /// <summary>
    /// Filters entities by search text, status, and complexity, then rebuilds domain groups.
    /// </summary>
    private void ApplyFilters()
    {
        var filtered = allEntities.AsEnumerable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim();
            filtered = filtered.Where(e =>
                e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Domain.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        // Status filter
        if (SelectedStatusFilter != FilterAll)
        {
            var status = SelectedStatusFilter switch
            {
                FilterNotStarted => MigrationStatus.NotStarted,
                FilterInProgress => MigrationStatus.InProgress,
                FilterDone => MigrationStatus.Done,
                FilterSkipped => MigrationStatus.Skipped,
                _ => (MigrationStatus?)null
            };
            if (status.HasValue)
            {
                filtered = filtered.Where(e => e.Status == status.Value);
            }
        }

        // Complexity filter
        if (SelectedComplexityFilter != FilterAll)
        {
            var tier = SelectedComplexityFilter switch
            {
                FilterSimple => ComplexityTier.Simple,
                FilterMedium => ComplexityTier.Medium,
                FilterComplex => ComplexityTier.Complex,
                FilterVeryComplex => ComplexityTier.VeryComplex,
                _ => (ComplexityTier?)null
            };
            if (tier.HasValue)
            {
                filtered = filtered.Where(e => e.Complexity == tier.Value);
            }
        }

        BuildDomainGroups(filtered.ToList());
    }

    /// <summary>
    /// Groups filtered entities by domain and populates the DomainGroups collection.
    /// </summary>
    private void BuildDomainGroups(List<MigrationEntity> entities)
    {
        DomainGroups.Clear();

        var groups = entities
            .GroupBy(e => e.Domain)
            .OrderBy(g => g.Key);

        foreach (var group in groups)
        {
            var entityList = group.ToList();
            var doneCount = entityList.Count(e => e.Status == MigrationStatus.Done);
            var inProgressCount = entityList.Count(e => e.Status == MigrationStatus.InProgress);

            DomainGroups.Add(new DomainMigrationGroup
            {
                DomainName = group.Key,
                EntityCount = entityList.Count,
                DoneCount = doneCount,
                InProgressCount = inProgressCount,
                ProgressPercent = entityList.Count > 0
                    ? Math.Round(doneCount * 100.0 / entityList.Count, 1)
                    : 0,
                Entities = entityList
            });
        }
    }
}
