using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SimpleLauncher.Models;
using SimpleLauncher.Services.GameListUI;
using SimpleLauncher.Services.LoadingOverlay;
using SimpleLauncher.Services.Pagination;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.UIReset;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher.Services.UiOrchestrator;

public class UiOrchestrator : IUiOrchestrator, ILoadingOverlayHost, IGameListUiHost, IPaginationHost
{
    private IUiOrchestratorHost _host;
    private readonly LoadingOverlayService _loadingOverlayService;
    private readonly GameListUiService _gameListUiService;
    private readonly IPaginationService _paginationService;
    private readonly IUiResetService _uiResetService;
    private readonly IUpdateStatusBar _updateStatusBarService;
    private readonly PlaySoundEffects _playSoundEffects;

    // ReSharper disable once NotAccessedField.Local
    private readonly SettingsManager.SettingsManager _settings;

    public UiOrchestrator(
        LoadingOverlayService loadingOverlayService,
        GameListUiService gameListUiService,
        IPaginationService paginationService,
        IUiResetService uiResetService,
        IUpdateStatusBar updateStatusBarService,
        PlaySoundEffects playSoundEffects,
        SettingsManager.SettingsManager settings)
    {
        _loadingOverlayService = loadingOverlayService;
        _gameListUiService = gameListUiService;
        _paginationService = paginationService;
        _uiResetService = uiResetService;
        _updateStatusBarService = updateStatusBarService;
        _playSoundEffects = playSoundEffects;
        _settings = settings;
    }

    public void Initialize(IUiOrchestratorHost host)
    {
        _host = host;
        _loadingOverlayService.Initialize(this);
        _gameListUiService.Initialize(this);
        _paginationService.Initialize(this);
        _uiResetService.Initialize(host as IUiResetHost);
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        _loadingOverlayService.SetLoadingState(isLoading, message);
    }

    public void EmergencyRelease()
    {
        _loadingOverlayService.EmergencyRelease();
    }

    public void NavigateToPage(Page page)
    {
        _host.MainGameContent.Visibility = Visibility.Collapsed;
        _host.PageContentFrame.Visibility = Visibility.Visible;
        _host.PageContentFrame.Content = page;
    }

    public void NavigateBackToMainContent()
    {
        _host.PageContentFrame.Content = null;
        _host.MainGameContent.Visibility = Visibility.Visible;
        _host.PageContentFrame.Visibility = Visibility.Collapsed;
        _playSoundEffects.PlayNotificationSound();
    }

    public void ResetPaginationButtons()
    {
        _paginationService.Reset();
    }

    public void SetPaginationButtonsDefault()
    {
        _host.PrevPageButton2.IsEnabled = false;
        _host.NextPageButton2.IsEnabled = false;
    }

    public void SetPaginationButtonsVisibility(Visibility visibility)
    {
        _host.PrevPageButton2.Visibility = visibility;
        _host.NextPageButton2.Visibility = visibility;
    }

    public void SetPaginationButtonsEnabled(bool prevEnabled, bool nextEnabled)
    {
        _host.PrevPageButton2.IsEnabled = prevEnabled;
        _host.NextPageButton2.IsEnabled = nextEnabled;
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        _gameListUiService.SetGameButtonsEnabled(isEnabled);
    }

    public void ClearGameButtonImages()
    {
        GameListUiService.ClearGameButtonImages(_host.GameFileGrid);
    }

    public void SetGameFileGridVisibility(Visibility visibility)
    {
        _host.GameFileGrid.Visibility = visibility;
    }

    public void SetListViewPreviewAreaVisibility(Visibility visibility)
    {
        _host.ListViewPreviewArea.Visibility = visibility;
    }

    public void ScrollToTop()
    {
        _host.Scroller.ScrollToTop();
    }

    public void UpdateTotalFilesLabel(string text)
    {
        _host.TotalFilesLabel.Dispatcher.Invoke(() => _host.TotalFilesLabel.Content = text);
    }

    public void AddNoFilesMessage()
    {
        _gameListUiService.AddNoFilesMessage();
    }

    public void ClearPreviewImage()
    {
        _host.PreviewImage.Source = null;
    }

    public void SetSearchTextBoxText(string text)
    {
        _host.SearchTextBox.Text = text;
    }

    public void SetSortOrderToggleButtonVisibility(Visibility visibility)
    {
        _host.SortOrderToggleButton.Visibility = visibility;
    }

    public void SetLoadingOverlayVisibility(Visibility visibility)
    {
        _host.LoadingOverlay.Visibility = visibility;
    }

    public Task SetUiBeforeLoadGameFilesAsync()
    {
        return _gameListUiService.SetUiBeforeLoadGameFilesAsync();
    }

    public int PaginationFilesPerPage
    {
        get => _paginationService.FilesPerPage;
        set => _paginationService.FilesPerPage = value;
    }

    public int PaginationThreshold
    {
        get => _paginationService.PaginationThreshold;
        set => _paginationService.PaginationThreshold = value;
    }

    public List<string> ApplyPagination(List<string> allFiles)
    {
        return _paginationService.ApplyPagination(allFiles);
    }

    public bool CanGoToPrevPage()
    {
        return _paginationService.CanGoPrev();
    }

    public bool CanGoToNextPage()
    {
        return _paginationService.CanGoNext();
    }

    public void GoToPreviousPage()
    {
        _paginationService.GoToPreviousPage();
    }

    public void GoToNextPage()
    {
        _paginationService.GoToNextPage();
    }

    Dispatcher ILoadingOverlayHost.Dispatcher => _host.Dispatcher;

    void ILoadingOverlayHost.SetIsLoadingGamesInternal(bool value)
    {
        _host.SetIsLoadingGamesInternal(value);
    }

    void ILoadingOverlayHost.SetLoadingOverlayVisibility(Visibility visibility)
    {
        _host.LoadingOverlay.Visibility = visibility;
    }

    void ILoadingOverlayHost.SetLoadingOverlayContent(object content)
    {
        if (_host.LoadingOverlay is ContentControl contentControl)
        {
            contentControl.Content = content;
        }
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

    Dispatcher IGameListUiHost.Dispatcher => _host.Dispatcher;
    ScrollViewer IGameListUiHost.Scroller => _host.Scroller;
    Image IGameListUiHost.PreviewImage => _host.PreviewImage;
    WrapPanel IGameListUiHost.GameFileGrid => _host.GameFileGrid;
    Grid IGameListUiHost.ListViewPreviewArea => _host.ListViewPreviewArea;

    ObservableCollection<GameListViewItem> IGameListUiHost.GameListItems => _host.GameListItems;

    void IGameListUiHost.SetGameFileGridVisibility(Visibility visibility)
    {
        _host.GameFileGrid.Visibility = visibility;
    }

    void IGameListUiHost.SetListViewPreviewAreaVisibility(Visibility visibility)
    {
        _host.ListViewPreviewArea.Visibility = visibility;
    }

    void IGameListUiHost.SetPaginationButtonsVisibility(Visibility visibility)
    {
        SetPaginationButtonsVisibility(visibility);
    }

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

    void IPaginationHost.UpdateTotalFilesLabel(string text)
    {
        _host.TotalFilesLabel.Dispatcher.Invoke(() => _host.TotalFilesLabel.Content = text);
    }

    void IPaginationHost.AddNoFilesMessage()
    {
        _gameListUiService.AddNoFilesMessage();
    }
}
