using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Converts a <see cref="ViewModels.FileTreeNodeViewModel"/> to a <see cref="PackIconPhosphorIconsKind"/>
/// based on whether it is a directory and its file extension.
/// </summary>
public class FileExtensionToIconConverter : IValueConverter
{
    /// <summary>
    /// Converts a file tree node ViewModel to the appropriate PhosphorIcons icon kind.
    /// </summary>
    /// <param name="value">The <see cref="ViewModels.FileTreeNodeViewModel"/> to convert.</param>
    /// <param name="targetType">The target type (unused).</param>
    /// <param name="parameter">An optional parameter (unused).</param>
    /// <param name="culture">The culture (unused).</param>
    /// <returns>A <see cref="PackIconPhosphorIconsKind"/> value.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not ViewModels.FileTreeNodeViewModel node)
        {
            return PackIconPhosphorIconsKind.File;
        }

        if (node.IsDirectory)
        {
            return node.IsExpanded
                ? PackIconPhosphorIconsKind.FolderOpen
                : PackIconPhosphorIconsKind.FolderSimple;
        }

        return node.Extension switch
        {
            ".md" => PackIconPhosphorIconsKind.FileText,
            ".json" => PackIconPhosphorIconsKind.BracketsCurly,
            ".sh" or ".bat" or ".cmd" => PackIconPhosphorIconsKind.Terminal,
            ".js" or ".mjs" => PackIconPhosphorIconsKind.FileJs,
            ".cs" => PackIconPhosphorIconsKind.FileCode,
            ".xml" or ".xaml" or ".csproj" or ".slnx" => PackIconPhosphorIconsKind.Code,
            ".drawio" => PackIconPhosphorIconsKind.Graph,
            ".docx" => PackIconPhosphorIconsKind.FileDoc,
            _ => PackIconPhosphorIconsKind.File
        };
    }

    /// <summary>
    /// Not supported. This is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
