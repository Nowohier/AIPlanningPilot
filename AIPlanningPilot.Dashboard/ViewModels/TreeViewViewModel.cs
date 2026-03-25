using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Services;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel for the file tree panel. Loads the directory structure of the
/// restructuring project and handles file selection.
/// </summary>
public partial class TreeViewViewModel : ObservableObject
{
    private readonly IFileSystemService fileSystemService;
    private readonly IConfigurationService configurationService;
    private readonly INavigationService navigationService;

    private bool suppressSelection;

    /// <summary>
    /// Gets the root-level nodes of the file tree.
    /// </summary>
    public ObservableCollection<FileTreeNodeViewModel> RootNodes { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TreeViewViewModel"/> class.
    /// </summary>
    /// <param name="fileSystemService">Service for accessing the file system.</param>
    /// <param name="configurationService">Service providing the restructuring root path.</param>
    /// <param name="navigationService">Service for coordinating navigation to files.</param>
    public TreeViewViewModel(
        IFileSystemService fileSystemService,
        IConfigurationService configurationService,
        INavigationService navigationService)
    {
        this.fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        this.configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    /// <summary>
    /// Loads the directory tree from the restructuring root path.
    /// </summary>
    [RelayCommand]
    public void LoadTree()
    {
        RootNodes.Clear();

        var rootPath = configurationService.RestructuringRootPath;
        var nodes = fileSystemService.GetDirectoryTree(rootPath, applyWhitelist: true);

        foreach (var node in nodes)
        {
            RootNodes.Add(new FileTreeNodeViewModel(node, OnNodeSelected));
        }
    }

    /// <summary>
    /// Refreshes the file tree by reloading from disk.
    /// </summary>
    [RelayCommand]
    public void Refresh()
    {
        LoadTree();
    }

    /// <summary>
    /// Collapses all nodes, then expands the path to the specified file and selects it.
    /// </summary>
    /// <param name="filePath">The absolute path to the file to reveal.</param>
    public void RevealAndSelect(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return;
        }

        suppressSelection = true;
        try
        {
            CollapseAll(RootNodes);

            var node = FindAndExpand(RootNodes, filePath);
            if (node is not null)
            {
                node.IsSelected = true;
            }
        }
        finally
        {
            suppressSelection = false;
        }
    }

    /// <summary>
    /// Recursively collapses all nodes and deselects them.
    /// </summary>
    private static void CollapseAll(IEnumerable<FileTreeNodeViewModel> nodes)
    {
        foreach (var node in nodes)
        {
            node.IsExpanded = false;
            node.IsSelected = false;
            if (node.IsDirectory)
            {
                CollapseAll(node.Children);
            }
        }
    }

    /// <summary>
    /// Recursively searches for the node matching the file path.
    /// Expands parent directories along the way.
    /// </summary>
    /// <returns>The matching node, or null if not found.</returns>
    private static FileTreeNodeViewModel? FindAndExpand(
        IEnumerable<FileTreeNodeViewModel> nodes, string filePath)
    {
        foreach (var node in nodes)
        {
            if (string.Equals(node.FullPath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }

            if (node.IsDirectory &&
                filePath.StartsWith(node.FullPath, StringComparison.OrdinalIgnoreCase))
            {
                var found = FindAndExpand(node.Children, filePath);
                if (found is not null)
                {
                    node.IsExpanded = true;
                    return found;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Handles selection of a tree node by navigating to the file.
    /// </summary>
    /// <param name="node">The selected tree node.</param>
    private void OnNodeSelected(FileTreeNodeViewModel node)
    {
        if (!suppressSelection && !node.IsDirectory)
        {
            navigationService.NavigateToFile(node.FullPath);
        }
    }
}
