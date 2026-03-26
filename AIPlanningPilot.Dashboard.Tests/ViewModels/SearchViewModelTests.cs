using FluentAssertions;
using Moq;
using AIPlanningPilot.Dashboard.Models;
using AIPlanningPilot.Dashboard.Services;
using AIPlanningPilot.Dashboard.ViewModels;

namespace AIPlanningPilot.Dashboard.Tests.ViewModels;

/// <summary>
/// Unit tests for <see cref="SearchViewModel"/>.
/// </summary>
[TestFixture]
[Apartment(ApartmentState.STA)]
public class SearchViewModelTests
{
    private Mock<ISearchService> mockSearchService = null!;
    private Mock<IConfigurationService> mockConfig = null!;
    private Mock<INavigationService> mockNavigation = null!;

    private SearchViewModel? _sut;

    /// <summary>
    /// Initializes mock dependencies before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        mockSearchService = new Mock<ISearchService>(MockBehavior.Strict);
        mockConfig = new Mock<IConfigurationService>(MockBehavior.Strict);
        mockNavigation = new Mock<INavigationService>(MockBehavior.Strict);
    }

    /// <summary>
    /// Verifies all strict mock expectations and disposes the ViewModel.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _sut?.Dispose();
        mockSearchService.VerifyAll();
        mockConfig.VerifyAll();
        mockNavigation.VerifyAll();
    }

    /// <summary>
    /// Verifies that setting the query to empty clears results and hides the overlay.
    /// </summary>
    [Test]
    public void Query_WhenSetToEmpty_ShouldClearResultsAndHideOverlay()
    {
        // Arrange
        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);

        // Act
        _sut.Query = "";

        // Assert
        _sut.Results.Should().BeEmpty();
        _sut.HasSearched.Should().BeFalse();
        _sut.IsOverlayVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NavigateToResult calls the navigation service with the correct file path.
    /// </summary>
    [Test]
    public void NavigateToResult_WhenResultSelected_ShouldCallNavigationService()
    {
        // Arrange
        var result = new SearchResult { FilePath = @"C:\root\main\STATE.md" };
        mockNavigation.Setup(n => n.NavigateToFile(@"C:\root\main\STATE.md"));

        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);

        // Act
        _sut.NavigateToResult(result);

        // Assert
        mockNavigation.Verify(n => n.NavigateToFile(@"C:\root\main\STATE.md"), Times.Once);
    }

    /// <summary>
    /// Verifies that NavigateToResult hides the overlay after navigating.
    /// </summary>
    [Test]
    public void NavigateToResult_WhenResultSelected_ShouldHideOverlay()
    {
        // Arrange
        var result = new SearchResult { FilePath = @"C:\root\main\STATE.md" };
        mockNavigation.Setup(n => n.NavigateToFile(It.IsAny<string>()));

        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);
        _sut.IsOverlayVisible = true;

        // Act
        _sut.NavigateToResult(result);

        // Assert
        _sut.IsOverlayVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that NavigateToResult does not throw when called with null.
    /// </summary>
    [Test]
    public void NavigateToResult_WhenNull_ShouldNotThrow()
    {
        // Arrange
        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);

        // Act
        var act = () => _sut.NavigateToResult(null);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Verifies that HideOverlay sets IsOverlayVisible to false.
    /// </summary>
    [Test]
    public void HideOverlay_WhenCalled_ShouldSetOverlayInvisible()
    {
        // Arrange
        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);
        _sut.IsOverlayVisible = true;

        // Act
        _sut.HideOverlay();

        // Assert
        _sut.IsOverlayVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that OnSearchFieldGotFocus shows the overlay when query and results exist.
    /// </summary>
    [Test]
    public void OnSearchFieldGotFocus_WhenQueryExistsAndHasSearched_ShouldShowOverlay()
    {
        // Arrange
        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);

        // Setting Query triggers OnQueryChanged which creates a debounce timer (no mock needed for sync test)
        _sut.Query = "test";
        _sut.HasSearched = true;
        _sut.IsOverlayVisible = false;

        // Act
        _sut.OnSearchFieldGotFocus();

        // Assert
        _sut.IsOverlayVisible.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that OnSearchFieldGotFocus does not show the overlay when query is empty.
    /// </summary>
    [Test]
    public void OnSearchFieldGotFocus_WhenQueryIsEmpty_ShouldNotShowOverlay()
    {
        // Arrange
        _sut = new SearchViewModel(mockSearchService.Object, mockConfig.Object, mockNavigation.Object);
        _sut.Query = "";
        _sut.IsOverlayVisible = false;

        // Act
        _sut.OnSearchFieldGotFocus();

        // Assert
        _sut.IsOverlayVisible.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when searchService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullSearchService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SearchViewModel(null!, mockConfig.Object, mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("searchService");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when configurationService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullConfigurationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SearchViewModel(mockSearchService.Object, null!, mockNavigation.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("configurationService");
    }

    /// <summary>
    /// Verifies that an <see cref="ArgumentNullException"/> is thrown when navigationService is null.
    /// </summary>
    [Test]
    public void Constructor_WhenNullNavigationService_ShouldThrow()
    {
        // Arrange & Act
        var act = () => new SearchViewModel(mockSearchService.Object, mockConfig.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("navigationService");
    }
}
