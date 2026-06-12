namespace SimpleLauncher.Interfaces;

public interface IUiResetHost
{
    // State
    bool IsUiUpdating { get; set; }
    bool IsLoadingGames { get; set; }
    string CurrentFilter { get; set; }
    string ActiveSearchQueryOrMode { get; set; }
    string SelectedSystem { get; set; }
    string PlayTime { get; set; }
    string MameSortOrder { get; set; }
    CancellationToken CurrentCancellationToken { get; }

    // Operations
    void CancelAndRecreateToken();
    void ResetPaginationButtons();
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken);

    // UI element setters
    void SetLoadingOverlayVisible(bool isVisible);
    void SetSearchTextBoxText(string text);
    void ClearPreviewImage();
    void SetSystemComboBoxSelectedItem(object item);
    void SetEmulatorComboBoxSelectedItem(object item);
    void SetSortOrderToggleButtonVisible(bool isVisible);
}
