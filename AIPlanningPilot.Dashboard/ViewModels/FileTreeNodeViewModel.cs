using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.ViewModels;

/// <summary>
/// ViewModel representing a single node (file or directory) in the file tree.
/// Eagerly populates directory children at construction time and supports selection callbacks.
/// </summary>
public partial class FileTreeNodeViewModel : ObservableObject
{
    private readonly Action<FileTreeNodeViewModel>? onSelected;

    /// <summary>
    /// Gets the underlying file tree node model.
    /// </summary>
    public FileTreeNode Model { get; }

    /// <summary>
    /// Gets the display name of the node.
    /// </summary>
    public string Name => Model.Name;

    /// <summary>
    /// Gets the full path to the file or directory.
    /// </summary>
    public string FullPath => Model.FullPath;

    /// <summary>
    /// Gets a value indicating whether this node is a directory.
    /// </summary>
    public bool IsDirectory => Model.IsDirectory;

    /// <summary>
    /// Gets the file extension in lowercase, or empty for directories.
    /// </summary>
    public string Extension => Model.Extension;

    /// <summary>
    /// Gets or sets a value indicating whether this node is expanded in the tree.
    /// </summary>
    [ObservableProperty]
    private bool isExpanded;

    /// <summary>
    /// Gets or sets a value indicating whether this node is selected in the tree.
    /// </summary>
    [ObservableProperty]
    private bool isSelected;

    /// <summary>
    /// Gets the child nodes of this directory node.
    /// </summary>
    public ObservableCollection<FileTreeNodeViewModel> Children { get; } = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="FileTreeNodeViewModel"/> class.
    /// </summary>
    /// <param name="model">The underlying file tree node model.</param>
    /// <param name="onSelected">Callback invoked when this node is selected.</param>
    public FileTreeNodeViewModel(FileTreeNode model, Action<FileTreeNodeViewModel>? onSelected = null)
    {
        Model = model ?? throw new ArgumentNullException(nameof(model));
        this.onSelected = onSelected;

        if (model.IsDirectory)
        {
            foreach (var child in model.Children)
            {
                Children.Add(new FileTreeNodeViewModel(child, onSelected));
            }
        }
    }

    /// <summary>
    /// Invokes the selection callback when this node becomes selected.
    /// </summary>
    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            onSelected?.Invoke(this);
        }
    }

    /// <summary>
    /// Gets the icon kind name for the file type, used to select the appropriate PhosphorIcon.
    /// </summary>
    public string IconKind => Model.IsDirectory
        ? "FolderSimple"
        : Model.Extension switch
        {
            ".md" => "FileText",
            ".json" => "FileCss",
            ".sh" => "Terminal",
            ".bat" or ".cmd" => "Terminal",
            ".js" or ".mjs" => "FileJs",
            ".cs" => "FileCode",
            ".xml" or ".xaml" => "FileXml",
            ".drawio" => "Graph",
            _ => "File"
        };
}
