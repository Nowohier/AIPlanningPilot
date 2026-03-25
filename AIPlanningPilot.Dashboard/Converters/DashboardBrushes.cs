using System.Windows.Media;

namespace AIPlanningPilot.Dashboard.Converters;

/// <summary>
/// Centralized color palette for the dashboard UI. All converters reference
/// these shared frozen brushes to ensure consistency and avoid per-call allocations.
/// </summary>
internal static class DashboardBrushes
{
    // -- Status colors (solid) --

    /// <summary>Green brush for "Done" / "Complete" status.</summary>
    public static readonly SolidColorBrush GreenBrush = Freeze(new SolidColorBrush(Color.FromRgb(76, 175, 80)));

    /// <summary>Blue brush for "In Progress" / "Next" status.</summary>
    public static readonly SolidColorBrush BlueBrush = Freeze(new SolidColorBrush(Color.FromRgb(33, 150, 243)));

    /// <summary>Amber brush for warnings and medium-priority items.</summary>
    public static readonly SolidColorBrush AmberBrush = Freeze(new SolidColorBrush(Color.FromRgb(255, 193, 7)));

    /// <summary>Orange brush for complex-tier items.</summary>
    public static readonly SolidColorBrush OrangeBrush = Freeze(new SolidColorBrush(Color.FromRgb(255, 152, 0)));

    /// <summary>Red brush for critical / very complex items.</summary>
    public static readonly SolidColorBrush RedBrush = Freeze(new SolidColorBrush(Color.FromRgb(244, 67, 54)));

    /// <summary>Gray brush for "Not Started" / "Pending" / default status.</summary>
    public static readonly SolidColorBrush GrayBrush = Freeze(new SolidColorBrush(Color.FromRgb(158, 158, 158)));

    // -- Phase background (pastel) --

    /// <summary>Light green pastel background for completed phases.</summary>
    public static readonly SolidColorBrush DonePastelBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE8, 0xF5, 0xE9)));

    /// <summary>Light blue pastel background for in-progress phases.</summary>
    public static readonly SolidColorBrush InProgressPastelBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE3, 0xF2, 0xFD)));

    /// <summary>Light gray background for not-started phases.</summary>
    public static readonly SolidColorBrush NotStartedPastelBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xEB, 0xEB, 0xEB)));

    // -- Phase foreground/text colors --

    /// <summary>Light gray brush for not-started circle indicators.</summary>
    public static readonly SolidColorBrush LightGrayBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xCC, 0xCC, 0xCC)));

    /// <summary>Gray text brush for not-started labels.</summary>
    public static readonly SolidColorBrush GrayTextBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E)));

    /// <summary>White brush for foreground text on colored badges.</summary>
    public static readonly SolidColorBrush WhiteBrush = Freeze(new SolidColorBrush(Colors.White));

    /// <summary>Transparent brush.</summary>
    public static readonly SolidColorBrush TransparentBrush = Freeze(new SolidColorBrush(Colors.Transparent));

    // -- Impact colors --

    /// <summary>Very high impact background brush.</summary>
    public static readonly SolidColorBrush VeryHighImpactBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE5, 0x39, 0x35)));

    /// <summary>High impact background brush.</summary>
    public static readonly SolidColorBrush HighImpactBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFB, 0x8C, 0x00)));

    /// <summary>Medium impact background brush.</summary>
    public static readonly SolidColorBrush MediumImpactBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xFD, 0xD8, 0x35)));

    /// <summary>Low impact background brush.</summary>
    public static readonly SolidColorBrush LowImpactBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x43, 0xA0, 0x47)));

    /// <summary>Dark foreground text brush for medium impact badges.</summary>
    public static readonly SolidColorBrush DarkForegroundBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33)));

    /// <summary>
    /// Freezes a brush to make it immutable and improve rendering performance.
    /// </summary>
    private static SolidColorBrush Freeze(SolidColorBrush brush)
    {
        brush.Freeze();
        return brush;
    }
}
