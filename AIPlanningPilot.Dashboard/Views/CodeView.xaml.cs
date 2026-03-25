using System.Windows.Controls;

namespace AIPlanningPilot.Dashboard.Views;

/// <summary>
/// Code-behind for the code viewer control.
/// Displays source code with syntax highlighting using AvalonEdit.
/// </summary>
public partial class CodeView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodeView"/> class.
    /// </summary>
    public CodeView()
    {
        InitializeComponent();
    }
}
