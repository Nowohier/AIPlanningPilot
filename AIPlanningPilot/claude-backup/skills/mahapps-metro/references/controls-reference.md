# MahApps.Metro Controls Reference

## Custom Controls (34)

| Control | Description | Key Properties |
|---------|-------------|----------------|
| `MetroWindow` | Main window replacement | GlowBrush, TitleCharacterCasing, SaveWindowPosition, ShowTitleBar |
| `Badged` | Badge overlay | Badge, BadgePlacement |
| `ColorPicker` | Color selection (Canvas, Palette, EyeDropper) | SelectedColor |
| `ContentControlEx` | Enhanced ContentControl | RecognizesAccessKey |
| `CustomValidationPopup` | Validation popup | |
| `DateTimePicker` | Date + time picker | SelectedDateTime, Orientation |
| `DropDownButton` | Button with context menu | Content, Icon, ItemsSource, DisplayMemberPath |
| `FlipView` | Swipe navigation | BannerText, IsBannerEnabled, SelectedIndex |
| `Flyout` | Sliding overlay panel | IsOpen, Position, Theme, Header |
| `FlyoutsControl` | Container for Flyouts | |
| `FontIcon` | Segoe MDL2 Assets icon | Glyph, FontSize |
| `HamburgerMenu` | Navigation drawer | ItemsSource, SelectedItem, DisplayMode |
| `HotKeyBox` | Keyboard shortcut display | HotKey |
| `MetroAnimatedSingleRowTabControl` | Animated single-row tabs | SelectedIndex |
| `MetroAnimatedTabControl` | Animated tabs | SelectedIndex |
| `MetroContentControl` | Animated content transitions | |
| `MetroHeader` | Header + content | Header |
| `MetroNavigationWindow` | Navigation window | |
| `MetroProgressBar` | Progress bar | Value, Maximum, IsIndeterminate |
| `MetroTabControl` | Enhanced tab control | |
| `MetroTabItem` | Enhanced tab item | CloseButtonEnabled, CloseTabCommand |
| `MetroThumbContentControl` | Drag-enabled content | |
| `MultiFrameImage` | Multi-resolution image | |
| `MultiSelectionComboBox` | Multi-select dropdown | SelectedItems |
| `NumericUpDown` | Numeric input | Minimum, Maximum, Interval, StringFormat, Value |
| `ProgressRing` | Circular spinner | IsActive |
| `RangeSlider` | Dual-handle slider | LowerValue, UpperValue, Minimum, Maximum |
| `SplitButton` | Button with selection dropdown | SelectedItem, SelectedIndex, Command |
| `SplitView` | Resizable split panel | IsPaneOpen, DisplayMode, OpenPaneLength |
| `Tile` | Tile element | Title, HorizontalTitleAlignment |
| `TimePicker` | Time selection | SelectedDateTime |
| `ToggleSwitch` | On/off switch | IsOn, Header, OnContent, OffContent |
| `TransitioningContentControl` | Content transitions | Transition |
| `WindowButtonCommands` | Min/Max/Close buttons | |
| `WindowCommands` | Title bar commands area | |

## Auto-Styled Standard WPF Controls (29)

These are styled automatically when `Controls.xaml` is included. No explicit `Style=` needed.

Button, Calendar, CheckBox, ComboBox, DataGrid, DataGridColumns, DatePicker, Expander, GridSplitter, GroupBox, Hyperlink, ListBox, ListView, Menu, ContextMenu, Page, PasswordBox, ProgressBar, RadioButton, ScrollBar, Slider, StatusBar, TabControl, TextBlock, TextBox, ToggleButton, ToolBar, ToolTip, TreeView

## Style Variants

| ResourceDictionary | Look |
|--------------------|------|
| `Controls.xaml` | Default MahApps style |
| `Controls.FlatButton.xaml` | Flat/chromeless buttons |

Style variant keys for specific controls:
- `MahApps.Styles.Button.Circle` -- Circular button
- `MahApps.Styles.Button.Square` -- Square button
- `MahApps.Styles.Button.Square.Accent` -- Accent square button
- `MahApps.Styles.DataGrid.Azure` -- Azure-look DataGrid

## Built-in Themes (46)

Format: `{Base}.{Accent}` (e.g. `Light.Blue`, `Dark.Emerald`)

**Bases:** Light, Dark

**Accents (23):** Red, Green, Blue, Purple, Orange, Lime, Emerald, Teal, Cyan, Cobalt, Indigo, Violet, Pink, Magenta, Crimson, Amber, Yellow, Brown, Olive, Steel, Mauve, Taupe, Sienna

## TextBoxHelper Attached Properties

Works on: TextBox, PasswordBox, ComboBox, NumericUpDown, DatePicker, TimePicker, DateTimePicker, HotKeyBox

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Watermark` | string | "" | Placeholder text |
| `WatermarkAlignment` | TextAlignment | Left | Watermark alignment |
| `WatermarkTrimming` | TextTrimming | None | Overflow behavior |
| `WatermarkWrapping` | TextWrapping | NoWrap | Wrapping behavior |
| `UseFloatingWatermark` | bool | false | Animated floating label |
| `AutoWatermark` | bool | false | From DisplayAttribute |
| `ClearTextButton` | bool | false | Show clear (X) button |
| `SelectAllOnFocus` | bool | false | Select all on focus |
| `ButtonCommand` | ICommand | null | Custom button command |
| `ButtonCommandParameter` | object | null | Button command parameter |
| `ButtonContent` | object | "r" | Button visual content |
| `ButtonContentTemplate` | DataTemplate | null | Button content template |
| `ButtonWidth` | double | 22 | Button width |
| `ButtonsAlignment` | ButtonsAlignment | Right | Button placement |

## ControlsHelper Attached Properties

| Property | Description |
|----------|-------------|
| `ControlsHelper.ContentCharacterCasing` | Override text casing on headers |
| `ControlsHelper.CornerRadius` | Corner radius for controls |
| `ControlsHelper.MouseOverBorderBrush` | Border brush on mouse over |
| `ControlsHelper.FocusBorderBrush` | Border brush on focus |
| `ControlsHelper.DisabledVisualElementVisibility` | Visibility when disabled |

## Key Resource Key Naming Convention

All MahApps resource keys follow: `MahApps.{Category}.{Specific}`

| Pattern | Example |
|---------|---------|
| `MahApps.Brushes.*` | `MahApps.Brushes.Accent`, `MahApps.Brushes.Gray3` |
| `MahApps.Colors.*` | `MahApps.Colors.AccentBase`, `MahApps.Colors.ThemeBackground` |
| `MahApps.Styles.*` | `MahApps.Styles.Button.Circle` |
| `MahApps.Sizes.*` | `MahApps.Sizes.Font.Header`, `MahApps.Sizes.Font.Content` |
| `MahApps.Font.*` | `MahApps.Font.Family.Header`, `MahApps.Font.Family.Button` |

## PhosphorIcons (v6.x) -- Common Enum Values

**Navigation/Actions:** ArrowRight, ArrowLeft, ArrowsClockwise, CaretDown, CaretRight, Check, X, Plus, Minus, MagnifyingGlass, Gear, House

**Files/Folders:** File, FileText, FileJs, FileCode, FileDoc, Files, FolderSimple, FolderOpen, Folders

**UI Elements:** ChartBar, Graph, Terminal, Code, BracketsCurly, Scales, Warning, Info, Question

**People/Communication:** User, UserCircle, Users, Envelope, ChatCircle

**Status:** Check, CheckCircle, XCircle, CircleDashed, Clock, ArrowRight

**Not available in v6.0 (use alternatives):**
- ~~FolderSimpleOpen~~ -- use `FolderOpen` instead
- ~~Brackets~~ -- use `BracketsCurly`, `BracketsSquare`, or `BracketsAngle`
