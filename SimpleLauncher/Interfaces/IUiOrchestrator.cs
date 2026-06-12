using System.Windows.Controls;

namespace SimpleLauncher.Interfaces;

public interface IUiOrchestrator
{
    void Initialize(IUiOrchestratorHost host);

    void SetLoadingState(bool isLoading, string message = null);
    void EmergencyRelease();

    void NavigateToPage(Page page);
    void NavigateBackToMainContent();

    void ResetPaginationButtons();
    void SetPaginationButtonsDefault();
    void SetPaginationButtonsVisible(bool isVisible);
    void SetPaginationButtonsEnabled(bool prevEnabled, bool nextEnabled);

    void SetGameButtonsEnabled(bool isEnabled);
    void ClearGameButtonImages();
    void SetGameFileGridVisible(bool isVisible);
    void SetListViewPreviewAreaVisible(bool isVisible);
    void ScrollToTop();
    void UpdateTotalFilesLabel(string text);
    void AddNoFilesMessage();

    void ClearPreviewImage();
    void SetSearchTextBoxText(string text);
    void SetSortOrderToggleButtonVisible(bool isVisible);
    void SetLoadingOverlayVisible(bool isVisible);

    Task SetUiBeforeLoadGameFilesAsync();

    int PaginationFilesPerPage { get; set; }
    int PaginationThreshold { get; set; }
    List<string> ApplyPagination(List<string> allFiles);
    bool CanGoToPrevPage();
    bool CanGoToNextPage();
    void GoToPreviousPage();
    void GoToNextPage();
}
