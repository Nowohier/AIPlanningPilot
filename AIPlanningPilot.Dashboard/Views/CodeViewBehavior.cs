using System.Windows;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Attached behavior for AvalonEdit <see cref="TextEditor"/> that enables data binding
/// for the document text and syntax highlighting mode, since AvalonEdit properties
/// are not dependency properties by default.
/// </summary>
public static class CodeViewBehavior
{
    /// <summary>
    /// Identifies the DocumentText attached property that binds to the editor's text content.
    /// </summary>
    public static readonly DependencyProperty DocumentTextProperty =
        DependencyProperty.RegisterAttached(
            "DocumentText",
            typeof(string),
            typeof(CodeViewBehavior),
            new FrameworkPropertyMetadata(null, OnDocumentTextChanged));

    /// <summary>
    /// Identifies the HighlightingName attached property that binds to the syntax highlighting mode.
    /// </summary>
    public static readonly DependencyProperty HighlightingNameProperty =
        DependencyProperty.RegisterAttached(
            "HighlightingName",
            typeof(string),
            typeof(CodeViewBehavior),
            new FrameworkPropertyMetadata(null, OnHighlightingNameChanged));

    /// <summary>
    /// Gets the DocumentText attached property value.
    /// </summary>
    public static string? GetDocumentText(DependencyObject obj) => (string?)obj.GetValue(DocumentTextProperty);

    /// <summary>
    /// Sets the DocumentText attached property value.
    /// </summary>
    public static void SetDocumentText(DependencyObject obj, string? value) => obj.SetValue(DocumentTextProperty, value);

    /// <summary>
    /// Gets the HighlightingName attached property value.
    /// </summary>
    public static string? GetHighlightingName(DependencyObject obj) => (string?)obj.GetValue(HighlightingNameProperty);

    /// <summary>
    /// Sets the HighlightingName attached property value.
    /// </summary>
    public static void SetHighlightingName(DependencyObject obj, string? value) => obj.SetValue(HighlightingNameProperty, value);

    /// <summary>
    /// Handles changes to the DocumentText attached property by updating the editor text.
    /// </summary>
    private static void OnDocumentTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextEditor editor && e.NewValue is string text)
        {
            editor.Text = text;
            editor.ScrollToHome();
        }
    }

    /// <summary>
    /// Handles changes to the HighlightingName attached property by updating the syntax highlighting.
    /// </summary>
    private static void OnHighlightingNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextEditor editor)
        {
            var name = e.NewValue as string;
            editor.SyntaxHighlighting = string.IsNullOrEmpty(name)
                ? null
                : HighlightingManager.Instance.GetDefinition(name);
        }
    }
}
