using System.Globalization;
using System.Windows;
using System.Windows.Media;
using FluentAssertions;
using AIPlanningPilot.Dashboard.Converters;
using AIPlanningPilot.Dashboard.Models;

namespace AIPlanningPilot.Dashboard.Tests.Converters;

/// <summary>
/// Unit tests for WPF value converters:
/// <see cref="BoolToVisibilityConverter"/>,
/// <see cref="InverseBoolToVisibilityConverter"/>,
/// <see cref="NullToVisibilityConverter"/>,
/// <see cref="ActionStatusToColorConverter"/>,
/// <see cref="ActiveViewToBrushConverter"/>,
/// <see cref="ActiveViewToVisibilityConverter"/>,
/// <see cref="ComplexityTierToColorConverter"/>,
/// <see cref="DoubleToGridLengthConverter"/>,
/// <see cref="ImpactToBrushConverter"/>,
/// <see cref="MigrationStatusToColorConverter"/>,
/// <see cref="PhaseStatusClassifier"/>,
/// <see cref="PhaseStatusDisplayConverter"/>,
/// <see cref="PhaseStatusToBackgroundConverter"/>,
/// <see cref="PhaseStatusToColorConverter"/>,
/// <see cref="PlanHeadingLevelToMarginConverter"/>, and
/// <see cref="StringToUpperConverter"/>.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class ConverterTests
{
    // ----- BoolToVisibilityConverter -----

    [Test]
    public void BoolToVisibility_Convert_WhenTrue_ShouldReturnVisible()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Test]
    public void BoolToVisibility_Convert_WhenFalse_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new BoolToVisibilityConverter();

        // Act
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    // ----- InverseBoolToVisibilityConverter -----

    [Test]
    public void InverseBoolToVisibility_Convert_WhenTrue_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Test]
    public void InverseBoolToVisibility_Convert_WhenFalse_ShouldReturnVisible()
    {
        // Arrange
        var converter = new InverseBoolToVisibilityConverter();

        // Act
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    // ----- NullToVisibilityConverter -----

    [Test]
    public void NullToVisibility_Convert_WhenNotNull_ShouldReturnVisible()
    {
        // Arrange
        var converter = new NullToVisibilityConverter();

        // Act
        var result = converter.Convert("some value", typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Test]
    public void NullToVisibility_Convert_WhenNull_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new NullToVisibilityConverter();

        // Act
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    // ----- ActionStatusToColorConverter -----

    [Test]
    public void ActionStatusToColor_Convert_WhenDone_ShouldReturnGreenBrush()
    {
        // Arrange
        var converter = new ActionStatusToColorConverter();

        // Act
        var result = converter.Convert(ActionStatus.Done, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result;
        brush.Color.Should().Be(Color.FromRgb(76, 175, 80));
    }

    [Test]
    public void ActionStatusToColor_Convert_WhenNext_ShouldReturnBlueBrush()
    {
        // Arrange
        var converter = new ActionStatusToColorConverter();

        // Act
        var result = converter.Convert(ActionStatus.Next, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result;
        brush.Color.Should().Be(Color.FromRgb(33, 150, 243));
    }

    [Test]
    public void ActionStatusToColor_Convert_WhenPending_ShouldReturnGrayBrush()
    {
        // Arrange
        var converter = new ActionStatusToColorConverter();

        // Act
        var result = converter.Convert(ActionStatus.Pending, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result;
        brush.Color.Should().Be(Color.FromRgb(158, 158, 158));
    }

    // ----- ActiveViewToBrushConverter -----

    [Test]
    public void ActiveViewToBrush_Convert_WhenViewMatchesParameter_ShouldReturnNotStartedPastelBrush()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var result = converter.Convert(ActiveView.Dashboard, typeof(Brush), "Dashboard", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        var brush = (SolidColorBrush)result;
        brush.Color.Should().Be(DashboardBrushes.NotStartedPastelBrush.Color);
    }

    [Test]
    public void ActiveViewToBrush_Convert_WhenViewDoesNotMatchParameter_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var result = converter.Convert(ActiveView.Dashboard, typeof(Brush), "Decisions", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Test]
    public void ActiveViewToBrush_Convert_WhenParameterIsInvalidString_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var result = converter.Convert(ActiveView.Dashboard, typeof(Brush), "InvalidView", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Test]
    public void ActiveViewToBrush_Convert_WhenParameterIsNotString_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var result = converter.Convert(ActiveView.Dashboard, typeof(Brush), 42, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Test]
    public void ActiveViewToBrush_Convert_WhenValueIsNotActiveView_ShouldReturnTransparent()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var result = converter.Convert("NotAnEnum", typeof(Brush), "Dashboard", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Brushes.Transparent);
    }

    [Test]
    public void ActiveViewToBrush_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new ActiveViewToBrushConverter();

        // Act
        var act = () => converter.ConvertBack(Brushes.Transparent, typeof(ActiveView), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- ActiveViewToVisibilityConverter -----

    [Test]
    public void ActiveViewToVisibility_Convert_WhenViewMatchesParameter_ShouldReturnVisible()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var result = converter.Convert(ActiveView.Migration, typeof(Visibility), "Migration", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Visible);
    }

    [Test]
    public void ActiveViewToVisibility_Convert_WhenViewDoesNotMatchParameter_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var result = converter.Convert(ActiveView.Migration, typeof(Visibility), "Dashboard", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Test]
    public void ActiveViewToVisibility_Convert_WhenParameterIsInvalidString_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var result = converter.Convert(ActiveView.File, typeof(Visibility), "InvalidView", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Test]
    public void ActiveViewToVisibility_Convert_WhenParameterIsNotString_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var result = converter.Convert(ActiveView.Handover, typeof(Visibility), 123, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Test]
    public void ActiveViewToVisibility_Convert_WhenValueIsNotActiveView_ShouldReturnCollapsed()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var result = converter.Convert("NotAnEnum", typeof(Visibility), "Dashboard", CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(Visibility.Collapsed);
    }

    [Test]
    public void ActiveViewToVisibility_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new ActiveViewToVisibilityConverter();

        // Act
        var act = () => converter.ConvertBack(Visibility.Visible, typeof(ActiveView), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- ComplexityTierToColorConverter -----

    [Test]
    public void ComplexityTierToColor_Convert_WhenSimple_ShouldReturnBlueBrush()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var result = converter.Convert(ComplexityTier.Simple, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.BlueBrush.Color);
    }

    [Test]
    public void ComplexityTierToColor_Convert_WhenMedium_ShouldReturnAmberBrush()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var result = converter.Convert(ComplexityTier.Medium, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.AmberBrush.Color);
    }

    [Test]
    public void ComplexityTierToColor_Convert_WhenComplex_ShouldReturnOrangeBrush()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var result = converter.Convert(ComplexityTier.Complex, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.OrangeBrush.Color);
    }

    [Test]
    public void ComplexityTierToColor_Convert_WhenVeryComplex_ShouldReturnRedBrush()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var result = converter.Convert(ComplexityTier.VeryComplex, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.RedBrush.Color);
    }

    [Test]
    public void ComplexityTierToColor_Convert_WhenValueIsNotComplexityTier_ShouldReturnGrayBrush()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var result = converter.Convert("NotATier", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GrayBrush.Color);
    }

    [Test]
    public void ComplexityTierToColor_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new ComplexityTierToColorConverter();

        // Act
        var act = () => converter.ConvertBack(DashboardBrushes.BlueBrush, typeof(ComplexityTier), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- DoubleToGridLengthConverter -----

    [Test]
    public void DoubleToGridLength_Convert_WhenPositiveDouble_ShouldReturnStarGridLength()
    {
        // Arrange
        var converter = new DoubleToGridLengthConverter();

        // Act
        var result = converter.Convert(75.0, typeof(GridLength), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<GridLength>();
        var gridLength = (GridLength)result;
        gridLength.Value.Should().Be(75.0);
        gridLength.GridUnitType.Should().Be(GridUnitType.Star);
    }

    [Test]
    public void DoubleToGridLength_Convert_WhenZero_ShouldReturnZeroPixelGridLength()
    {
        // Arrange
        var converter = new DoubleToGridLengthConverter();

        // Act
        var result = converter.Convert(0.0, typeof(GridLength), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<GridLength>();
        var gridLength = (GridLength)result;
        gridLength.Value.Should().Be(0.0);
        gridLength.GridUnitType.Should().Be(GridUnitType.Pixel);
    }

    [Test]
    public void DoubleToGridLength_Convert_WhenNegative_ShouldReturnZeroPixelGridLength()
    {
        // Arrange
        var converter = new DoubleToGridLengthConverter();

        // Act
        var result = converter.Convert(-5.0, typeof(GridLength), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<GridLength>();
        var gridLength = (GridLength)result;
        gridLength.Value.Should().Be(0.0);
        gridLength.GridUnitType.Should().Be(GridUnitType.Pixel);
    }

    [Test]
    public void DoubleToGridLength_Convert_WhenValueIsNotDouble_ShouldReturnZeroPixelGridLength()
    {
        // Arrange
        var converter = new DoubleToGridLengthConverter();

        // Act
        var result = converter.Convert("notADouble", typeof(GridLength), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<GridLength>();
        var gridLength = (GridLength)result;
        gridLength.Value.Should().Be(0.0);
        gridLength.GridUnitType.Should().Be(GridUnitType.Pixel);
    }

    [Test]
    public void DoubleToGridLength_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new DoubleToGridLengthConverter();

        // Act
        var act = () => converter.ConvertBack(new GridLength(1, GridUnitType.Star), typeof(double), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- ImpactToBrushConverter -----

    [Test]
    public void ImpactToBrush_Convert_WhenVeryHighBackground_ShouldReturnVeryHighImpactBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.VeryHigh, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.VeryHighImpactBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenVeryHighForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.VeryHigh, typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenHighBackground_ShouldReturnHighImpactBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.High, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.HighImpactBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenHighForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.High, typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenMediumBackground_ShouldReturnMediumImpactBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.Medium, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.MediumImpactBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenMediumForeground_ShouldReturnDarkForegroundBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.Medium, typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.DarkForegroundBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenLowBackground_ShouldReturnLowImpactBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.Low, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.LowImpactBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenLowForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.Low, typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenValueIsNotImpactLevelBackground_ShouldReturnGrayBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert("NotAnImpact", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GrayBrush.Color);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenValueIsNotImpactLevelForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert("NotAnImpact", typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void ImpactToBrush_Convert_WhenForegroundParameterCaseInsensitive_ShouldReturnForegroundBrush()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var result = converter.Convert(ImpactLevel.Medium, typeof(Brush), "foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.DarkForegroundBrush.Color);
    }

    [Test]
    public void ImpactToBrush_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new ImpactToBrushConverter();

        // Act
        var act = () => converter.ConvertBack(DashboardBrushes.WhiteBrush, typeof(ImpactLevel), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- MigrationStatusToColorConverter -----

    [Test]
    public void MigrationStatusToColor_Convert_WhenDone_ShouldReturnGreenBrush()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var result = converter.Convert(MigrationStatus.Done, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GreenBrush.Color);
    }

    [Test]
    public void MigrationStatusToColor_Convert_WhenInProgress_ShouldReturnAmberBrush()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var result = converter.Convert(MigrationStatus.InProgress, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.AmberBrush.Color);
    }

    [Test]
    public void MigrationStatusToColor_Convert_WhenSkipped_ShouldReturnRedBrush()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var result = converter.Convert(MigrationStatus.Skipped, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.RedBrush.Color);
    }

    [Test]
    public void MigrationStatusToColor_Convert_WhenNotStarted_ShouldReturnGrayBrush()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var result = converter.Convert(MigrationStatus.NotStarted, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GrayBrush.Color);
    }

    [Test]
    public void MigrationStatusToColor_Convert_WhenValueIsNotMigrationStatus_ShouldReturnGrayBrush()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var result = converter.Convert("NotAStatus", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GrayBrush.Color);
    }

    [Test]
    public void MigrationStatusToColor_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new MigrationStatusToColorConverter();

        // Act
        var act = () => converter.ConvertBack(DashboardBrushes.GreenBrush, typeof(MigrationStatus), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- PhaseStatusClassifier -----

    [Test]
    public void PhaseStatusClassifier_Classify_WhenDone_ShouldReturnDone()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("done");

        // Assert
        result.Should().Be(PhaseStatusCategory.Done);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenComplete_ShouldReturnDone()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("complete");

        // Assert
        result.Should().Be(PhaseStatusCategory.Done);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenCompleted_ShouldReturnDone()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("completed");

        // Assert
        result.Should().Be(PhaseStatusCategory.Done);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenDoneUpperCase_ShouldReturnDone()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("DONE");

        // Assert
        result.Should().Be(PhaseStatusCategory.Done);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenDoneWithWhitespace_ShouldReturnDone()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("  done  ");

        // Assert
        result.Should().Be(PhaseStatusCategory.Done);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenNotStarted_ShouldReturnNotStarted()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("not started");

        // Assert
        result.Should().Be(PhaseStatusCategory.NotStarted);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenEmptyString_ShouldReturnNotStarted()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("");

        // Assert
        result.Should().Be(PhaseStatusCategory.NotStarted);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenNull_ShouldReturnNotStarted()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify(null);

        // Assert
        result.Should().Be(PhaseStatusCategory.NotStarted);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenWhitespaceOnly_ShouldReturnNotStarted()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("   ");

        // Assert
        result.Should().Be(PhaseStatusCategory.NotStarted);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenInProgressText_ShouldReturnInProgress()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("Day 1 done");

        // Assert
        result.Should().Be(PhaseStatusCategory.InProgress);
    }

    [Test]
    public void PhaseStatusClassifier_Classify_WhenArbitraryNonEmptyText_ShouldReturnInProgress()
    {
        // Arrange & Act
        var result = PhaseStatusClassifier.Classify("50% complete");

        // Assert
        result.Should().Be(PhaseStatusCategory.InProgress);
    }

    // ----- PhaseStatusDisplayConverter -----

    [Test]
    public void PhaseStatusDisplay_Convert_WhenDone_ShouldReturnDoneDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("done", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Done");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenComplete_ShouldReturnDoneDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("complete", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Done");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenCompleted_ShouldReturnDoneDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("completed", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Done");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenPartialProgress_ShouldReturnInProgressDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("Day 1 done", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("In Progress");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenEmptyString_ShouldReturnNotStartedDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Not Started");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenNull_ShouldReturnNotStartedDisplay()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("Not Started");
    }

    [Test]
    public void PhaseStatusDisplay_Convert_WhenNotStartedText_ShouldReturnOriginalStatus()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var result = converter.Convert("not started", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("not started");
    }

    [Test]
    public void PhaseStatusDisplay_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new PhaseStatusDisplayConverter();

        // Act
        var act = () => converter.ConvertBack("Done", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- PhaseStatusToBackgroundConverter -----

    [Test]
    public void PhaseStatusToBackground_Convert_WhenDone_ShouldReturnDonePastelBrush()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var result = converter.Convert("done", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.DonePastelBrush.Color);
    }

    [Test]
    public void PhaseStatusToBackground_Convert_WhenInProgress_ShouldReturnInProgressPastelBrush()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var result = converter.Convert("Day 2 done", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.InProgressPastelBrush.Color);
    }

    [Test]
    public void PhaseStatusToBackground_Convert_WhenNotStarted_ShouldReturnNotStartedPastelBrush()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var result = converter.Convert("not started", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.NotStartedPastelBrush.Color);
    }

    [Test]
    public void PhaseStatusToBackground_Convert_WhenEmpty_ShouldReturnNotStartedPastelBrush()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var result = converter.Convert("", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.NotStartedPastelBrush.Color);
    }

    [Test]
    public void PhaseStatusToBackground_Convert_WhenNull_ShouldReturnNotStartedPastelBrush()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var result = converter.Convert(null!, typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.NotStartedPastelBrush.Color);
    }

    [Test]
    public void PhaseStatusToBackground_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new PhaseStatusToBackgroundConverter();

        // Act
        var act = () => converter.ConvertBack(DashboardBrushes.DonePastelBrush, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- PhaseStatusToColorConverter -----

    [Test]
    public void PhaseStatusToColor_Convert_WhenDoneNoParameter_ShouldReturnGreenBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("done", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GreenBrush.Color);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenDoneForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("done", typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenInProgressNoParameter_ShouldReturnBlueBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("Day 1 done", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.BlueBrush.Color);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenInProgressForeground_ShouldReturnWhiteBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("Day 1 done", typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.White);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenNotStartedNoParameter_ShouldReturnLightGrayBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("not started", typeof(Brush), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.LightGrayBrush.Color);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenNotStartedForeground_ShouldReturnGrayTextBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("not started", typeof(Brush), "Foreground", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(DashboardBrushes.GrayTextBrush.Color);
    }

    [Test]
    public void PhaseStatusToColor_Convert_WhenNotStartedBadge_ShouldReturnTransparentBrush()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var result = converter.Convert("not started", typeof(Brush), "Badge", CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<SolidColorBrush>();
        ((SolidColorBrush)result).Color.Should().Be(Colors.Transparent);
    }

    [Test]
    public void PhaseStatusToColor_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new PhaseStatusToColorConverter();

        // Act
        var act = () => converter.ConvertBack(DashboardBrushes.GreenBrush, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- PlanHeadingLevelToMarginConverter -----

    [Test]
    public void PlanHeadingLevelToMargin_Convert_WhenLevel1_ShouldReturnDefaultMargin()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var result = converter.Convert(1, typeof(Thickness), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var margin = (Thickness)result;
        margin.Should().Be(new Thickness(0, 2, 0, 2));
    }

    [Test]
    public void PlanHeadingLevelToMargin_Convert_WhenLevel2_ShouldReturnDefaultMargin()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var result = converter.Convert(2, typeof(Thickness), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var margin = (Thickness)result;
        margin.Should().Be(new Thickness(0, 2, 0, 2));
    }

    [Test]
    public void PlanHeadingLevelToMargin_Convert_WhenLevel3_ShouldReturnIndentedMargin()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var result = converter.Convert(3, typeof(Thickness), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var margin = (Thickness)result;
        margin.Should().Be(new Thickness(16, 2, 0, 2));
    }

    [Test]
    public void PlanHeadingLevelToMargin_Convert_WhenLevel5_ShouldReturnIndentedMargin()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var result = converter.Convert(5, typeof(Thickness), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var margin = (Thickness)result;
        margin.Should().Be(new Thickness(16, 2, 0, 2));
    }

    [Test]
    public void PlanHeadingLevelToMargin_Convert_WhenValueIsNotInt_ShouldReturnDefaultMargin()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var result = converter.Convert("notAnInt", typeof(Thickness), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().BeOfType<Thickness>();
        var margin = (Thickness)result;
        margin.Should().Be(new Thickness(0, 2, 0, 2));
    }

    [Test]
    public void PlanHeadingLevelToMargin_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new PlanHeadingLevelToMarginConverter();

        // Act
        var act = () => converter.ConvertBack(new Thickness(0), typeof(int), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }

    // ----- StringToUpperConverter -----

    [Test]
    public void StringToUpper_Convert_WhenLowercaseString_ShouldReturnUppercase()
    {
        // Arrange
        var converter = new StringToUpperConverter();

        // Act
        var result = converter.Convert("hello world", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("HELLO WORLD");
    }

    [Test]
    public void StringToUpper_Convert_WhenMixedCaseString_ShouldReturnUppercase()
    {
        // Arrange
        var converter = new StringToUpperConverter();

        // Act
        var result = converter.Convert("Hello World", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("HELLO WORLD");
    }

    [Test]
    public void StringToUpper_Convert_WhenEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var converter = new StringToUpperConverter();

        // Act
        var result = converter.Convert("", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be("");
    }

    [Test]
    public void StringToUpper_Convert_WhenValueIsNotString_ShouldReturnValueUnchanged()
    {
        // Arrange
        var converter = new StringToUpperConverter();

        // Act
        var result = converter.Convert(42, typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        result.Should().Be(42);
    }

    [Test]
    public void StringToUpper_ConvertBack_ShouldThrowNotSupportedException()
    {
        // Arrange
        var converter = new StringToUpperConverter();

        // Act
        var act = () => converter.ConvertBack("HELLO", typeof(string), null!, CultureInfo.InvariantCulture);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
