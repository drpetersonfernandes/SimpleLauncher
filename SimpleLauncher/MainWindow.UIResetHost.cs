using System.Windows;
using SimpleLauncher.Services.UIReset;

namespace SimpleLauncher;

public partial class MainWindow
{
    // IUIResetHost explicit implementations
    bool IUiResetHost.IsUiUpdating { get; set; }

    bool IUiResetHost.IsLoadingGames
    {
        get => _isLoadingGames;
        set
        {
            _isLoadingGames = value;
            IsLoadingGames = value;
        }
    }

    string IUiResetHost.CurrentFilter { get; set; }

    string IUiResetHost.ActiveSearchQueryOrMode { get; set; }

    string IUiResetHost.SelectedSystem
    {
        get => SelectedSystem;
        set => SelectedSystem = value;
    }

    string IUiResetHost.PlayTime
    {
        get => PlayTime;
        set => PlayTime = value;
    }

    string IUiResetHost.MameSortOrder { get; set; } = "FileName";

    CancellationToken IUiResetHost.CurrentCancellationToken => _cancellationSource.Token;

    void IUiResetHost.CancelAndRecreateToken()
    {
        CancelAndRecreateToken();
    }

    void IUiResetHost.ResetPaginationButtons()
    {
        UiOrchestrator.ResetPaginationButtons();
    }

    Task IUiResetHost.DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken)
    {
        return _gameBrowser.DisplaySystemSelectionScreenAsync(cancellationToken);
    }

    void IUiResetHost.SetLoadingOverlayVisible(bool isVisible)
    {
        LoadingOverlay.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    void IUiResetHost.SetSearchTextBoxText(string text)
    {
        SearchTextBox.Text = text;
    }

    void IUiResetHost.ClearPreviewImage()
    {
        PreviewImage.Source = null;
    }

    void IUiResetHost.SetSystemComboBoxSelectedItem(object item)
    {
        SystemComboBox.SelectedItem = item;
    }

    void IUiResetHost.SetEmulatorComboBoxSelectedItem(object item)
    {
        EmulatorComboBox.SelectedItem = item;
    }

    void IUiResetHost.SetSortOrderToggleButtonVisible(bool isVisible)
    {
        SortOrderToggleButton.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
    }
}
