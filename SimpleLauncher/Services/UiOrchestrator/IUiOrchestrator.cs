using System.Windows;
using System.Windows.Controls;

namespace SimpleLauncher.Services.UiOrchestrator;

public interface IUiOrchestrator
{
    void Initialize(IUiOrchestratorHost host);

    void SetLoadingState(bool isLoading, string message = null);
    void EmergencyRelease();

    void NavigateToPage(Page page);
    void NavigateBackToMainContent();

    void ResetPaginationButtons();
    void SetPaginationButtonsDefault();
    void SetPaginationButtonsVisibility(Visibility visibility);
    void SetPaginationButtonsEnabled(bool prevEnabled, bool nextEnabled);

    void SetGameButtonsEnabled(bool isEnabled);
    void ClearGameButtonImages();
    void SetGameFileGridVisibility(Visibility visibility);
    void SetListViewPreviewAreaVisibility(Visibility visibility);
    void ScrollToTop();
    void UpdateTotalFilesLabel(string text);
    void AddNoFilesMessage();

    void ClearPreviewImage();
    void SetSearchTextBoxText(string text);
    void SetSortOrderToggleButtonVisibility(Visibility visibility);
    void SetLoadingOverlayVisibility(Visibility visibility);

    Task SetUiBeforeLoadGameFilesAsync();

    int PaginationFilesPerPage { get; set; }
    int PaginationThreshold { get; set; }
    List<string> ApplyPagination(List<string> allFiles);
    bool CanGoToPrevPage();
    bool CanGoToNextPage();
    void GoToPreviousPage();
    void GoToNextPage();
}
