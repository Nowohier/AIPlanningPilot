using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the inline search. Provides debounced full-text search across all files
/// in the restructuring directory with an overlay for results display.
/// </summary>
public partial class SearchViewModel : ObservableObject, IDisposable
{
    private readonly ISearchService searchService;
    private readonly IConfigurationService configurationService;
    private readonly INavigationService navigationService;
    private readonly SynchronizationContext? syncContext;
    private Timer? debounceTimer;
    private CancellationTokenSource? searchCts;
    private bool disposed;

    private const int DebounceDelayMs = 300;

    /// <summary>Gets or sets the search query text.</summary>
    [ObservableProperty]
    private string query = string.Empty;

    /// <summary>Gets or sets whether the search results overlay is visible.</summary>
    [ObservableProperty]
    private bool isOverlayVisible;

    /// <summary>Gets the search results.</summary>
    public ObservableCollection<SearchResult> Results { get; } = [];

    /// <summary>Gets or sets the count of search results.</summary>
    [ObservableProperty]
    private int resultCount;

    /// <summary>Gets or sets whether a search has been performed.</summary>
    [ObservableProperty]
    private bool hasSearched;

    /// <summary>Gets or sets whether a search is currently running.</summary>
    [ObservableProperty]
    private bool isSearching;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchViewModel"/> class.
    /// </summary>
    /// <param name="searchService">Service for performing file searches.</param>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="navigationService">Service for navigating to files.</param>
    public SearchViewModel(
        ISearchService searchService,
        IConfigurationService configurationService,
        INavigationService navigationService)
    {
        this.searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        syncContext = SynchronizationContext.Current;
    }

    /// <summary>
    /// Called when <see cref="Query"/> changes. Starts/resets the debounce timer.
    /// </summary>
    partial void OnQueryChanged(string value)
    {
        debounceTimer?.Dispose();

        if (string.IsNullOrEmpty(value))
        {
            Results.Clear();
            ResultCount = 0;
            HasSearched = false;
            IsOverlayVisible = false;
            return;
        }

        debounceTimer = new Timer(
            _ => PostToUiThread(ExecuteSearchAsync),
            null, DebounceDelayMs, Timeout.Infinite);
    }

    /// <summary>
    /// Shows the results overlay if there is a query with previous results.
    /// Called when the search field gains focus.
    /// </summary>
    public void OnSearchFieldGotFocus()
    {
        if (!string.IsNullOrEmpty(Query) && HasSearched)
        {
            IsOverlayVisible = true;
        }
    }

    /// <summary>
    /// Executes the search asynchronously with the current query text.
    /// </summary>
    private async void ExecuteSearchAsync()
    {
        var query = Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            return;
        }

        searchCts?.Cancel();
        searchCts?.Dispose();
        searchCts = new CancellationTokenSource();
        var token = searchCts.Token;

        IsSearching = true;

        try
        {
            var rootPath = configurationService.RestructuringRootPath;
            var results = await Task.Run(() => searchService.Search(query, rootPath), token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            Results.Clear();
            foreach (var result in results)
            {
                Results.Add(result);
            }

            ResultCount = results.Count;
            HasSearched = true;
            IsOverlayVisible = true;
        }
        catch (OperationCanceledException)
        {
            // Search was superseded by a newer query
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search failed: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    /// <summary>
    /// Navigates to the file containing the selected search result
    /// and hides the results overlay.
    /// </summary>
    /// <param name="result">The search result to navigate to.</param>
    [RelayCommand]
    public void NavigateToResult(SearchResult? result)
    {
        if (result is not null)
        {
            IsOverlayVisible = false;
            navigationService.NavigateToFile(result.FilePath);
        }
    }

    /// <summary>
    /// Hides the search results overlay.
    /// </summary>
    [RelayCommand]
    public void HideOverlay()
    {
        IsOverlayVisible = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        debounceTimer?.Dispose();
        searchCts?.Dispose();
        disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Posts an action to the UI thread via the captured synchronization context.
    /// </summary>
    private void PostToUiThread(Action action)
    {
        if (syncContext is not null)
        {
            syncContext.Post(_ => action(), null);
        }
        else
        {
            action();
        }
    }
}
