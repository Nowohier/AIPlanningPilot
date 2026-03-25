---
name: mahapps-metro
description: "Build WPF applications with MahApps.Metro v2.4.x, MahApps.Metro.IconPacks, CommunityToolkit.Mvvm. Covers MetroWindow, theming, controls, dialogs (including MVVM DialogCoordinator), styles, and icon packs. Scoped to the AIPlanningPilot.Dashboard project."
user-invocable: true
origin: "Manually curated from https://mahapps.com/docs/ and MahApps.Metro source"
installed: 2026-03-23
scope: "AIPlanningPilot.Dashboard/**"
---

# MahApps.Metro -- WPF UI Skill

## Purpose

Guide the implementation of WPF UI using MahApps.Metro v2.4.x with modern MVVM patterns (CommunityToolkit.Mvvm). This skill covers setup, theming, controls, dialogs, styles, and icon packs.

## When to Use

- Creating or modifying WPF views in `AIPlanningPilot.Dashboard`
- Adding MahApps controls (Flyouts, ToggleSwitch, NumericUpDown, etc.)
- Theming or styling work (light/dark, accent colors, custom brushes)
- Implementing async dialogs (message, input, progress)
- Using icon packs (PhosphorIcons)
- Troubleshooting XAML binding or style issues with MahApps

## When NOT to Use

- Angular frontend code (use angular-* skills)
- Backend .NET code without UI
- Non-MahApps WPF (vanilla WPF or WPF-UI/Fluent)

## Stack

| Package | Version | Purpose |
|---------|---------|---------|
| `MahApps.Metro` | 2.4.10 | MetroWindow, controls, dialogs, theming |
| `MahApps.Metro.IconPacks.PhosphorIcons` | 6.0.0 | Icons |
| `CommunityToolkit.Mvvm` | 8.4.0 | ObservableObject, [ObservableProperty], [RelayCommand] |
| `Markdig.Wpf` | 0.5.0.1 | Markdown rendering |
| `AvalonEdit` | 6.3.0.90 | Code syntax highlighting |

---

## 1. Setup

### App.xaml Resource Dictionaries (required, order matters)

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Controls.xaml" />
            <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Fonts.xaml" />
            <ResourceDictionary Source="pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Blue.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

For flat button styles, add: `pack://application:,,,/MahApps.Metro;component/Styles/Controls.FlatButton.xaml`

**Gotcha:** Resource file names are **case sensitive**.

### XAML Namespace Declarations

```xml
xmlns:mah="http://metro.mahapps.com/winfx/xaml/controls"
xmlns:iconPacks="http://metro.mahapps.com/winfx/xaml/iconpacks"
xmlns:Dialog="clr-namespace:MahApps.Metro.Controls.Dialogs;assembly=MahApps.Metro"
```

---

## 2. MetroWindow

### Basic Window

```xml
<mah:MetroWindow x:Class="MyApp.Views.MainWindow"
                 xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                 xmlns:mah="http://metro.mahapps.com/winfx/xaml/controls"
                 Title="My App" Height="800" Width="1280"
                 TitleCharacterCasing="Normal"
                 WindowStartupLocation="CenterScreen"
                 GlowBrush="{DynamicResource MahApps.Brushes.Accent}"
                 BorderThickness="1">
```

Code-behind must inherit `MetroWindow`:

```csharp
using MahApps.Metro.Controls;
public partial class MainWindow : MetroWindow { ... }
```

### Key MetroWindow Properties

| Property | Type | Description |
|----------|------|-------------|
| `GlowBrush` | Brush | Glow effect around window edges |
| `NonActiveGlowBrush` | Brush | Glow when window is inactive |
| `BorderThickness` | Thickness | Border width (use with BorderBrush) |
| `SaveWindowPosition` | bool | Persist position/size across restarts |
| `ShowTitleBar` | bool | Show/hide title bar |
| `TitleBarHeight` | int | Title bar height in pixels |
| `TitleCharacterCasing` | CharacterCasing | `Normal`, `Upper`, `Lower` |
| `TitleAlignment` | HorizontalAlignment | Title text alignment |
| `ShowIconOnTitleBar` | bool | Show icon in title bar |
| `WindowTransitionsEnabled` | bool | Enable/disable animations |

### Window Commands (title bar buttons)

```xml
<mah:MetroWindow.LeftWindowCommands>
    <mah:WindowCommands>
        <Button ToolTip="Settings">
            <iconPacks:PackIconPhosphorIcons Kind="Gear" Width="18" Height="18" />
        </Button>
    </mah:WindowCommands>
</mah:MetroWindow.LeftWindowCommands>

<mah:MetroWindow.RightWindowCommands>
    <mah:WindowCommands>
        <Button Command="{Binding RefreshCommand}" ToolTip="Refresh">
            <StackPanel Orientation="Horizontal">
                <iconPacks:PackIconPhosphorIcons Kind="ArrowsClockwise" Width="16" Height="16"
                                                 VerticalAlignment="Center" Margin="0,0,6,0" />
                <TextBlock Text="Refresh" VerticalAlignment="Center" />
            </StackPanel>
        </Button>
    </mah:WindowCommands>
</mah:MetroWindow.RightWindowCommands>
```

Supported control types in WindowCommands: `Button`, `ToggleButton`, `SplitButton`, `DropDownButton`.

---

## 3. Theming

Theme API lives in `ControlzEx.Theming`. Theme name format: `{Base}.{Accent}`.

**Bases:** `Light`, `Dark`
**Accents:** Red, Green, Blue, Purple, Orange, Lime, Emerald, Teal, Cyan, Cobalt, Indigo, Violet, Pink, Magenta, Crimson, Amber, Yellow, Brown, Olive, Steel, Mauve, Taupe, Sienna

### Change theme at runtime

```csharp
using ControlzEx.Theming;

// Application-wide
ThemeManager.Current.ChangeTheme(Application.Current, "Dark.Blue");

// Single window
ThemeManager.Current.ChangeTheme(this, "Light.Cobalt");
```

### Sync with Windows OS theme

```csharp
ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
ThemeManager.Current.SyncTheme();
```

### Generate theme from any color

```csharp
ThemeManager.Current.AddTheme(
    RuntimeThemeGenerator.Current.GenerateRuntimeTheme("Dark", Colors.Red));
```

### Key Brush Resource Keys (use with DynamicResource)

| Key | Usage |
|-----|-------|
| `MahApps.Brushes.Accent` | Primary accent color |
| `MahApps.Brushes.Accent2` | Lighter accent |
| `MahApps.Brushes.Accent3` | Even lighter accent |
| `MahApps.Brushes.Accent4` | Lightest accent |
| `MahApps.Brushes.ThemeBackground` | Window/page background |
| `MahApps.Brushes.ThemeForeground` | Primary text color |
| `MahApps.Brushes.IdealForeground` | Text on accent backgrounds |
| `MahApps.Brushes.Gray1` through `Gray10` | Gray scale |
| `MahApps.Brushes.Text` | Standard text |
| `MahApps.Brushes.TextBox.Border` | TextBox border |
| `MahApps.Brushes.TextBox.Border.Focus` | TextBox focused border |
| `MahApps.Brushes.Button.Border` | Button border |
| `MahApps.Brushes.Validation5` | Validation error (#FFDC000C) |

---

## 4. Controls

### Flyouts (sliding overlay panels)

```xml
<mah:MetroWindow.Flyouts>
    <mah:FlyoutsControl>
        <mah:Flyout Header="Settings"
                    IsOpen="{Binding IsSettingsFlyoutOpen}"
                    Position="Right" Width="300"
                    Theme="Adapt">
            <!-- Content here -->
        </mah:Flyout>
    </mah:FlyoutsControl>
</mah:MetroWindow.Flyouts>
```

| Property | Values |
|----------|--------|
| `Position` | `Left`, `Right`, `Top`, `Bottom` |
| `Theme` | `Adapt`, `Inverse`, `Dark`, `Light`, `Accent` |

**Gotcha:** LeftWindowCommands, RightWindowCommands, and Icon are **never** shown over an opened Flyout.

### ToggleSwitch

```xml
<mah:ToggleSwitch Header="Enable Feature"
                  IsOn="{Binding IsFeatureEnabled}"
                  OnContent="Active"
                  OffContent="Inactive" />
```

### NumericUpDown

```xml
<mah:NumericUpDown Minimum="0" Maximum="10000"
                   Interval="100" StringFormat="N0"
                   Value="{Binding Quantity}" />
```

| Property | Description |
|----------|-------------|
| `Minimum` / `Maximum` | Value range |
| `Interval` | Step per click |
| `StringFormat` | Display format (`"C2"`, `"N0"`, `"P1"`, `"{0:N2} pcs"`) |
| `HideUpDownButtons` | Hide +/- buttons |
| `SpeedUp` | Acceleration when holding button (default: true) |

### ProgressRing

```xml
<mah:ProgressRing IsActive="{Binding IsLoading}" />
```

### MetroAnimatedSingleRowTabControl (single-row animated tabs)

```xml
<mah:MetroAnimatedSingleRowTabControl SelectedIndex="{Binding SelectedTabIndex}">
    <TabItem Header="Dashboard">
        <!-- content -->
    </TabItem>
    <TabItem>
        <TabItem.Header>
            <StackPanel Orientation="Horizontal">
                <iconPacks:PackIconPhosphorIcons Kind="Files" Width="16" Height="16"
                                                 VerticalAlignment="Center" Margin="0,0,6,0" />
                <TextBlock Text="Files" VerticalAlignment="Center" />
            </StackPanel>
        </TabItem.Header>
        <!-- content -->
    </TabItem>
</mah:MetroAnimatedSingleRowTabControl>
```

### MetroProgressBar

```xml
<mah:MetroProgressBar Value="{Binding Progress, Mode=OneWay}" Maximum="100"
                       Foreground="{DynamicResource MahApps.Brushes.Accent}" />
```

**Gotcha:** `MetroProgressBar.Value` binding defaults to TwoWay. Use `Mode=OneWay` when binding to read-only sources like `KeyValuePair.Value`.

### DropDownButton

```xml
<mah:DropDownButton Content="Options"
                    ItemsSource="{Binding MenuItems}"
                    DisplayMemberPath="Name">
    <mah:DropDownButton.Icon>
        <iconPacks:PackIconPhosphorIcons Kind="List" Margin="6" />
    </mah:DropDownButton.Icon>
    <mah:DropDownButton.ItemContainerStyle>
        <Style BasedOn="{StaticResource {x:Type MenuItem}}" TargetType="{x:Type MenuItem}">
            <Setter Property="Command" Value="{Binding RelativeSource={RelativeSource FindAncestor,
                    AncestorType={x:Type mah:DropDownButton}}, Path=DataContext.ItemCommand}" />
            <Setter Property="CommandParameter" Value="{Binding}" />
        </Style>
    </mah:DropDownButton.ItemContainerStyle>
</mah:DropDownButton>
```

Uses `ContextMenu` internally. No `SelectedItem`.

### SplitButton (button + dropdown with selection)

```xml
<mah:SplitButton SelectedIndex="0"
                 ItemsSource="{Binding Options}"
                 DisplayMemberPath="Name"
                 Command="{Binding ExecuteCommand}" />
```

Has `SelectedItem`, `SelectedIndex`, `SelectionChanged`. Uses `ListBox` internally.

### Badged (badge overlay)

```xml
<mah:Badged Badge="{Binding UnreadCount}" BadgePlacement="TopRight">
    <Button Content="Notifications" />
</mah:Badged>
```

### Tile

```xml
<mah:Tile Title="Mail" Background="Teal" HorizontalTitleAlignment="Right">
    <iconPacks:PackIconPhosphorIcons Kind="Envelope" Width="40" Height="40" />
</mah:Tile>
```

---

## 5. Dialogs (Async API)

All dialog methods are async extension methods on `MetroWindow`. Namespace: `MahApps.Metro.Controls.Dialogs`.

### ShowMessageAsync

```csharp
var result = await this.ShowMessageAsync(
    "Confirm Delete",
    "Are you sure?",
    MessageDialogStyle.AffirmativeAndNegative,
    new MetroDialogSettings
    {
        AffirmativeButtonText = "Delete",
        NegativeButtonText = "Cancel",
        DefaultButtonFocus = MessageDialogResult.Negative
    });

if (result == MessageDialogResult.Affirmative) { /* delete */ }
```

**MessageDialogStyle:** `Affirmative` (OK only), `AffirmativeAndNegative` (OK + Cancel), `AffirmativeAndNegativeAndSingleAuxiliary` (+ 1 extra), `AffirmativeAndNegativeAndDoubleAuxiliary` (+ 2 extra)

**MessageDialogResult:** `Canceled` (-1), `Negative` (0), `Affirmative` (1), `FirstAuxiliary` (2), `SecondAuxiliary` (3)

### ShowInputAsync

```csharp
var input = await this.ShowInputAsync("Rename", "Enter new name:",
    new MetroDialogSettings { DefaultText = "Current Name" });
if (input != null) { /* use input */ }
```

Returns `null` if cancelled.

### ShowProgressAsync

```csharp
var controller = await this.ShowProgressAsync("Working", "Please wait...", isCancelable: true);
controller.SetIndeterminate();

// Do work...
controller.SetProgress(0.5);
controller.SetMessage("Almost done...");

await controller.CloseAsync();
```

**ProgressDialogController API:** `SetProgress(double)`, `SetIndeterminate()`, `SetMessage(string)`, `SetTitle(object)`, `SetCancelable(bool)`, `CloseAsync()`, `IsCanceled`, `IsOpen`

### ShowLoginAsync

```csharp
var data = await this.ShowLoginAsync("Login", "Enter credentials:");
if (data != null) { /* data.Username, data.Password */ }
```

### MetroDialogSettings (key properties)

| Property | Default | Description |
|----------|---------|-------------|
| `AffirmativeButtonText` | "OK" | OK button text |
| `NegativeButtonText` | "Cancel" | Cancel button text |
| `FirstAuxiliaryButtonText` | null | Extra button 1 text |
| `DefaultButtonFocus` | Negative | Which button gets focus |
| `DefaultText` | "" | Input dialog default text |
| `AnimateShow` / `AnimateHide` | true | Dialog animations |
| `ColorScheme` | Theme | `Theme` or `Accented` |
| `CancellationToken` | None | Cancellation token |

### MVVM Dialogs (DialogCoordinator) -- no MetroWindow reference needed

XAML -- register ViewModel as dialog context:
```xml
<mah:MetroWindow xmlns:Dialog="clr-namespace:MahApps.Metro.Controls.Dialogs;assembly=MahApps.Metro"
                 Dialog:DialogParticipation.Register="{Binding}">
```

ViewModel:
```csharp
using MahApps.Metro.Controls.Dialogs;

public class MyViewModel
{
    private readonly IDialogCoordinator _dialogCoordinator;

    public MyViewModel(IDialogCoordinator dialogCoordinator)
    {
        _dialogCoordinator = dialogCoordinator;
    }

    private async Task ConfirmAsync()
    {
        var result = await _dialogCoordinator.ShowMessageAsync(
            this, "Title", "Message",
            MessageDialogStyle.AffirmativeAndNegative);
    }

    private async Task ShowProgressAsync()
    {
        var ctrl = await _dialogCoordinator.ShowProgressAsync(this, "Working", "...");
        ctrl.SetIndeterminate();
        // ...
        await ctrl.CloseAsync();
    }
}
```

DI registration: `services.AddSingleton<IDialogCoordinator>(DialogCoordinator.Instance);`

---

## 6. Styles

### Button Styles

| Style Key | Look |
|-----------|------|
| (default) | Standard metro button |
| `MahApps.Styles.Button.Circle` | Circular |
| `MahApps.Styles.Button.Square` | Square |
| `MahApps.Styles.Button.Square.Accent` | Square with accent background |

```xml
<Button Style="{DynamicResource MahApps.Styles.Button.Circle}">
    <iconPacks:PackIconPhosphorIcons Kind="Plus" />
</Button>
```

### TextBox Attached Properties (TextBoxHelper)

```xml
<TextBox mah:TextBoxHelper.Watermark="Search..."
         mah:TextBoxHelper.ClearTextButton="True"
         mah:TextBoxHelper.UseFloatingWatermark="True" />

<PasswordBox mah:TextBoxHelper.Watermark="Password"
             mah:TextBoxHelper.ClearTextButton="True" />
```

| Attached Property | Description |
|-------------------|-------------|
| `TextBoxHelper.Watermark` | Placeholder text |
| `TextBoxHelper.ClearTextButton` | Show X button to clear |
| `TextBoxHelper.UseFloatingWatermark` | Animated floating label |
| `TextBoxHelper.SelectAllOnFocus` | Select all on focus |
| `TextBoxHelper.AutoWatermark` | Watermark from DisplayAttribute |
| `TextBoxHelper.ButtonCommand` | Custom button command |
| `TextBoxHelper.ButtonContent` | Custom button content |

Works on: `TextBox`, `PasswordBox`, `ComboBox`, `NumericUpDown`, `DatePicker`, `TimePicker`.

### DataGrid Styles

```xml
<!-- Default (auto-applied) -->
<DataGrid ItemsSource="{Binding Items}" />

<!-- Azure style -->
<DataGrid Style="{StaticResource MahApps.Styles.DataGrid.Azure}" />

<!-- NumericUpDown column -->
<mah:DataGridNumericUpDownColumn Header="Price" Binding="{Binding Price}"
                                 StringFormat="C" Minimum="0" />

<!-- Styled CheckBox column -->
<DataGridCheckBoxColumn Header="Select"
    ElementStyle="{DynamicResource MetroDataGridCheckBox}"
    EditingElementStyle="{DynamicResource MetroDataGridCheckBox}" />
```

### GroupBox, StatusBar, TabControl

All standard WPF controls are auto-styled by the `Controls.xaml` resource dictionary. No explicit `Style=` needed.

---

## 7. Icon Packs (PhosphorIcons)

```xml
xmlns:iconPacks="http://metro.mahapps.com/winfx/xaml/iconpacks"
```

### Basic Usage

```xml
<iconPacks:PackIconPhosphorIcons Kind="House" Width="24" Height="24" />
```

### Properties

| Property | Description |
|----------|-------------|
| `Kind` | Icon enum value (e.g. `File`, `FolderSimple`, `MagnifyingGlass`) |
| `Foreground` | Icon color brush |
| `Width` / `Height` | Size |
| `Flip` | `Normal`, `Horizontal`, `Vertical`, `Both` |
| `RotationAngle` | Degrees |
| `Spin` | Enable spin animation |
| `SpinDuration` | Seconds per rotation |

### Common Icons in This Project

| Icon Kind | Used For |
|-----------|----------|
| `ChartBar` | Dashboard tab |
| `Files` | Files tab, file count |
| `Scales` | Decisions tab |
| `Warning` | Risks tab |
| `MagnifyingGlass` | Search tab |
| `UserCircle` | Handover tab |
| `ArrowsClockwise` | Refresh button |
| `FolderSimple` / `FolderOpen` | Directory nodes |
| `FileText` | Markdown files |
| `Terminal` | Shell scripts |
| `FileJs` | JavaScript files |
| `FileCode` | C# files |
| `BracketsCurly` | JSON files |
| `ArrowRight` | List item bullets |
| `Check` | Completed items |
| `User` | Team members |

---

## 8. Common Patterns in This Project

### Tab header with icon

```xml
<TabItem>
    <TabItem.Header>
        <StackPanel Orientation="Horizontal">
            <iconPacks:PackIconPhosphorIcons Kind="ChartBar" Width="16" Height="16"
                                             VerticalAlignment="Center" Margin="0,0,6,0" />
            <TextBlock Text="Dashboard" VerticalAlignment="Center" />
        </StackPanel>
    </TabItem.Header>
    <views:DashboardView DataContext="{Binding Dashboard}" />
</TabItem>
```

### Status badge (colored pill)

```xml
<Border CornerRadius="3" Padding="8,2"
        Background="{Binding Status, Converter={StaticResource ActionStatusToColorConverter}}">
    <TextBlock Text="{Binding Status}" FontSize="11" Foreground="White" FontWeight="SemiBold" />
</Border>
```

### KPI card (number + label)

```xml
<Border Background="White" CornerRadius="4" Padding="12,6">
    <StackPanel>
        <TextBlock Text="{Binding Count}" FontSize="22" FontWeight="Bold"
                   Foreground="{DynamicResource MahApps.Brushes.Accent}" HorizontalAlignment="Center" />
        <TextBlock Text="Open Decisions" FontSize="10"
                   Foreground="{DynamicResource MahApps.Brushes.Accent}" />
    </StackPanel>
</Border>
```

### Accent header banner

```xml
<Border Background="{DynamicResource MahApps.Brushes.Accent}" CornerRadius="6" Padding="16">
    <TextBlock Text="{Binding Title}" FontSize="20" FontWeight="Bold" Foreground="White" />
</Border>
```

### GroupBox with icon header

```xml
<GroupBox>
    <GroupBox.Header>
        <StackPanel Orientation="Horizontal">
            <iconPacks:PackIconPhosphorIcons Kind="UserCircle" Width="16" Height="16"
                                              VerticalAlignment="Center" Margin="0,0,6,0" />
            <TextBlock Text="Section Title" FontWeight="SemiBold" />
        </StackPanel>
    </GroupBox.Header>
    <!-- content -->
</GroupBox>
```

### StatusBar with accent background

```xml
<StatusBar Background="{DynamicResource MahApps.Brushes.Accent}">
    <StatusBarItem>
        <StackPanel Orientation="Horizontal">
            <iconPacks:PackIconPhosphorIcons Kind="Files" Width="14" Height="14"
                                             Foreground="White" VerticalAlignment="Center" Margin="0,0,4,0" />
            <TextBlock Text="{Binding FileCount, StringFormat='{}{0} files'}" Foreground="White" />
        </StackPanel>
    </StatusBarItem>
</StatusBar>
```

---

## 9. Gotchas and Anti-Patterns

1. **MetroProgressBar.Value** defaults to TwoWay binding. Always use `Mode=OneWay` when binding to read-only properties (e.g. `KeyValuePair.Value`).
2. **Resource dictionary filenames** are case-sensitive: `Light.Blue.xaml` not `light.blue.xaml`.
3. **Flyouts cover WindowCommands** -- LeftWindowCommands, RightWindowCommands, and Icon are never shown over an open Flyout.
4. **SaveWindowPosition** can cause off-screen launch if a monitor is disconnected. Provide a reset option.
5. **ThemeManager** is in `ControlzEx.Theming`, not `MahApps.Metro`.
6. **DynamicResource vs StaticResource** -- Use `DynamicResource` for MahApps brushes/colors so they respond to runtime theme changes.
7. **DialogCoordinator context** -- Must register the ViewModel via `Dialog:DialogParticipation.Register="{Binding}"` in the MetroWindow XAML.
8. **Code-behind inheritance** -- The Window class must inherit `MetroWindow`, not `Window`.

## References

See `references/controls-reference.md` for the full controls inventory and property tables.
