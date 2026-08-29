using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.LoadingOverlay;

namespace SimpleLauncher.Services.UiOrchestrator;

/// <summary>
///     Orchestrates UI operations including loading overlays, game list rendering, pagination, and page navigation.
/// </summary>
public class UiOrchestratorService : IUiOrchestrator, ILoadingOverlayHost, IGameListUiHost, IPaginationHost
{
    private readonly GameListUiService _gameListUiService;
    private readonly LoadingOverlayService _loadingOverlayService;
    private readonly IPaginationService _paginationService;
    private readonly PlaySoundEffects _playSoundEffects;

    // ReSharper disable once NotAccessedField.Local
    private readonly SettingsManagerService _settings;
    private readonly IUiResetService _uiResetService;
    private readonly IUpdateStatusBar _updateStatusBarService;
    private IUiOrchestratorHost _host = null!;

    /// <summary>Initializes a new instance of the UiOrchestratorService with the specified dependencies.</summary>
    public UiOrchestratorService(
        LoadingOverlayService loadingOverlayService,
        GameListUiService gameListUiService,
        IPaginationService paginationService,
        IUiResetService uiResetService,
        IUpdateStatusBar updateStatusBarService,
        PlaySoundEffects playSoundEffects,
        SettingsManagerService settings)
    {
        _loadingOverlayService = loadingOverlayService;
        _gameListUiService = gameListUiService;
        _paginationService = paginationService;
        _uiResetService = uiResetService;
        _updateStatusBarService = updateStatusBarService;
        _playSoundEffects = playSoundEffects;
        _settings = settings;
    }

    Dispatcher IGameListUiHost.Dispatcher => _host.Dispatcher;
    ScrollViewer IGameListUiHost.Scroller => _host.Scroller;
    Image IGameListUiHost.PreviewImage => _host.PreviewImage;
    WrapPanel IGameListUiHost.GameFileGrid => _host.GameFileGrid;
    Grid IGameListUiHost.ListViewPreviewArea => _host.ListViewPreviewArea;

    ObservableCollection<GameListViewItem> IGameListUiHost.GameListItems => _host.GameListItems;

    void IGameListUiHost.SetGameFileGridVisible(bool isVisible)
    {
        _host.GameFileGrid.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    void IGameListUiHost.SetListViewPreviewAreaVisible(bool isVisible)
    {
        _host.ListViewPreviewArea.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    void IGameListUiHost.SetPaginationButtonsVisible(bool isVisible)
    {
        SetPaginationButtonsVisible(isVisible);
    }

    Dispatcher ILoadingOverlayHost.Dispatcher => _host.Dispatcher;

    void ILoadingOverlayHost.SetIsLoadingGamesInternal(bool value)
    {
        _host.SetIsLoadingGamesInternal(value);
    }

    void ILoadingOverlayHost.SetLoadingOverlayVisible(bool isVisible)
    {
        _host.LoadingOverlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    void ILoadingOverlayHost.SetLoadingOverlayContent(object content)
    {
        if (_host.LoadingOverlay is ContentControl contentControl) contentControl.Content = content;
    }

    void ILoadingOverlayHost.SetMainContentGridEnabled(bool enabled)
    {
        _host.MainContentGrid.IsEnabled = enabled;
    }

    void ILoadingOverlayHost.CancelAndRecreateToken()
    {
        _host.CancelAndRecreateToken();
    }

    Task ILoadingOverlayHost.ResetUiAsync()
    {
        return _host.ResetUiAsync();
    }

    IUpdateStatusBar ILoadingOverlayHost.UpdateStatusBarService => _updateStatusBarService;

    void IPaginationHost.SetPrevPageButtonEnabled(bool enabled)
    {
        _host.PrevPageButton2.IsEnabled = enabled;
    }

    void IPaginationHost.SetNextPageButtonEnabled(bool enabled)
    {
        _host.NextPageButton2.IsEnabled = enabled;
    }

    void IPaginationHost.ScrollToTop()
    {
        _host.Scroller.ScrollToTop();
    }

    void IPaginationHost.UpdateTotalFilesLabel(string? text)
    {
        _host.TotalFilesLabel.Dispatcher.Invoke(() => _host.TotalFilesLabel.Content = text);
    }

    void IPaginationHost.AddNoFilesMessage()
    {
        _gameListUiService.AddNoFilesMessage();
    }

    /// <summary>Initializes the orchestrator and its child services with the specified UI host.</summary>
    public void Initialize(IUiOrchestratorHost host)
    {
        _host = host;
        _loadingOverlayService.Initialize(this);
        _gameListUiService.Initialize(this);
        _paginationService.Initialize(this);
        _uiResetService.Initialize((host as IUiResetHost)!);
    }

    /// <summary>Sets the loading state, optionally displaying a loading overlay with a message.</summary>
    public void SetLoadingState(bool isLoading, string? message = null)
    {
        _loadingOverlayService.SetLoadingState(isLoading, message);
    }

    /// <summary>Forces an emergency release of the loading overlay regardless of current state.</summary>
    public void EmergencyRelease()
    {
        _loadingOverlayService.EmergencyRelease();
    }

    /// <summary>Navigates to a Page within the content frame, hiding the main game content.</summary>
    public void NavigateToPage(Page page)
    {
        _host.MainGameContent.Visibility = Visibility.Collapsed;
        _host.PageContentFrame.Visibility = Visibility.Visible;
        _host.PageContentFrame.Content = page;
    }

    /// <summary>Navigates back to the main game content, clearing the content frame.</summary>
    public void NavigateBackToMainContent()
    {
        _host.PageContentFrame.Content = null;
        _host.MainGameContent.Visibility = Visibility.Visible;
        _host.PageContentFrame.Visibility = Visibility.Collapsed;
        _playSoundEffects.PlayNotificationSound();
    }

    /// <summary>Resets pagination buttons to their initial state.</summary>
    public void ResetPaginationButtons()
    {
        _paginationService.Reset();
    }

    /// <summary>Sets pagination buttons to their default disabled state.</summary>
    public void SetPaginationButtonsDefault()
    {
        _host.PrevPageButton2.IsEnabled = false;
        _host.NextPageButton2.IsEnabled = false;
    }

    /// <summary>Sets the visibility of pagination buttons.</summary>
    /// <param name="isVisible">Whether the buttons should be visible.</param>
    public void SetPaginationButtonsVisible(bool isVisible)
    {
        var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        _host.PrevPageButton2.Visibility = visibility;
        _host.NextPageButton2.Visibility = visibility;
    }

    /// <summary>Enables or disables the previous and next page buttons.</summary>
    /// <param name="prevEnabled">Whether the previous page button is enabled.</param>
    /// <param name="nextEnabled">Whether the next page button is enabled.</param>
    public void SetPaginationButtonsEnabled(bool prevEnabled, bool nextEnabled)
    {
        _host.PrevPageButton2.IsEnabled = prevEnabled;
        _host.NextPageButton2.IsEnabled = nextEnabled;
    }

    /// <summary>Enables or disables the game buttons.</summary>
    /// <param name="isEnabled">Whether the game buttons should be enabled.</param>
    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameListUiService.SetGameButtonsEnabled(isEnabled);
    }

    /// <summary>Clears all images from the game file grid buttons.</summary>
    public void ClearGameButtonImages()
    {
        GameListUiService.ClearGameButtonImages(_host.GameFileGrid);
    }

    /// <summary>Sets the visibility of the game file grid.</summary>
    /// <param name="isVisible">Whether the grid should be visible.</param>
    public void SetGameFileGridVisible(bool isVisible)
    {
        _host.GameFileGrid.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Sets the visibility of the list view preview area.</summary>
    /// <param name="isVisible">Whether the preview area should be visible.</param>
    public void SetListViewPreviewAreaVisible(bool isVisible)
    {
        _host.ListViewPreviewArea.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Scrolls the main content scroller to the top.</summary>
    public void ScrollToTop()
    {
        _host.Scroller.ScrollToTop();
    }

    /// <summary>Updates the total files label with the specified text.</summary>
    /// <param name="text">The text to display in the label.</param>
    public void UpdateTotalFilesLabel(string text)
    {
        _host.TotalFilesLabel.Dispatcher.Invoke(() => _host.TotalFilesLabel.Content = text);
    }

    /// <summary>Adds a message indicating no files were found.</summary>
    public void AddNoFilesMessage()
    {
        _gameListUiService.AddNoFilesMessage();
    }

    /// <summary>Clears the preview image.</summary>
    public void ClearPreviewImage()
    {
        _host.PreviewImage.Source = null;
    }

    /// <summary>Sets the text content of the search text box.</summary>
    /// <param name="text">The text to set.</param>
    public void SetSearchTextBoxText(string text)
    {
        _host.SearchTextBox.Text = text;
    }

    /// <summary>Sets the visibility of the loading overlay.</summary>
    /// <param name="isVisible">Whether the overlay should be visible.</param>
    public void SetLoadingOverlayVisible(bool isVisible)
    {
        _host.LoadingOverlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Configures the UI state before loading game files.</summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SetUiBeforeLoadGameFilesAsync()
    {
        return _gameListUiService.SetUiBeforeLoadGameFilesAsync();
    }

    /// <summary>Gets or sets the number of files displayed per page.</summary>
    public int PaginationFilesPerPage
    {
        get => _paginationService.FilesPerPage;
        set => _paginationService.FilesPerPage = value;
    }

    /// <summary>Gets or sets the file count threshold at which pagination activates.</summary>
    public int PaginationThreshold
    {
        get => _paginationService.PaginationThreshold;
        set => _paginationService.PaginationThreshold = value;
    }

    /// <summary>Applies pagination to the given file list and returns the current page subset.</summary>
    /// <param name="allFiles">The complete list of files to paginate.</param>
    /// <returns>The subset of files for the current page.</returns>
    public IList<string> ApplyPagination(IList<string> allFiles)
    {
        return _paginationService.ApplyPagination(allFiles);
    }

    /// <summary>Determines whether navigation to the previous page is possible.</summary>
    /// <returns><c>true</c> if a previous page exists; otherwise <c>false</c>.</returns>
    public bool CanGoToPrevPage()
    {
        return _paginationService.CanGoPrev();
    }

    /// <summary>Determines whether navigation to the next page is possible.</summary>
    /// <returns><c>true</c> if a next page exists; otherwise <c>false</c>.</returns>
    public bool CanGoToNextPage()
    {
        return _paginationService.CanGoNext();
    }

    /// <summary>Navigates to the previous page of results.</summary>
    public void GoToPreviousPage()
    {
        _paginationService.GoToPreviousPage();
    }

    /// <summary>Navigates to the next page of results.</summary>
    public void GoToNextPage()
    {
        _paginationService.GoToNextPage();
    }
}