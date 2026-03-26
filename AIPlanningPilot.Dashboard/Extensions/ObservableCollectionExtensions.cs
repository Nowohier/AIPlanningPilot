using System.Collections.ObjectModel;

namespace AIPlanningPilot.Dashboard.Extensions;

/// <summary>
/// Extension methods for <see cref="ObservableCollection{T}"/>.
/// </summary>
internal static class ObservableCollectionExtensions
{
    /// <summary>
    /// Replaces all items in the collection with items from the source sequence.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="collection">The target collection to replace items in.</param>
    /// <param name="source">The source items to populate the collection with.</param>
    public static void ReplaceWith<T>(this ObservableCollection<T> collection, IEnumerable<T> source)
    {
        collection.Clear();
        foreach (var item in source)
        {
            collection.Add(item);
        }
    }
}
