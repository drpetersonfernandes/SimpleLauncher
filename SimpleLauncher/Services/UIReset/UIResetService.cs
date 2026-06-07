using System.Windows;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.UIReset;

public class UiResetService : IUiResetService
{
    private readonly ILogErrors _logErrors;
    private IUiResetHost _host;

    public UiResetService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    public void Initialize(IUiResetHost host)
    {
        _host = host;
    }

    public async Task ResetUiAsync()
    {
        try
        {
            _host.CancelAndRecreateToken();

            if (_host.IsUiUpdating) return;

            _host.IsUiUpdating = true;

            if (_host.IsLoadingGames)
            {
                _host.IsLoadingGames = false;
                _host.SetLoadingOverlayVisible(false);
            }

            try
            {
                _host.ResetPaginationButtons();

                _host.SetSearchTextBoxText("");

                _host.CurrentFilter = null;
                _host.ActiveSearchQueryOrMode = null;

                _host.SelectedSystem = null;
                _host.ClearPreviewImage();
                _host.SetSystemComboBoxSelectedItem(null);
                _host.SetEmulatorComboBoxSelectedItem(null);
                _host.SetSortOrderToggleButtonVisible(false);
                _host.MameSortOrder = "FileName";

                var nosystemselected = (string)Application.Current.TryFindResource("Nosystemselected") ?? "No system selected";
                _host.SelectedSystem = nosystemselected;
                _host.PlayTime = "00:00:00";

                await _host.DisplaySystemSelectionScreenAsync(_host.CurrentCancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Do nothing.
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in the method ResetUiAsync.");
            }
            finally
            {
                _host.IsUiUpdating = false;
            }
        }
        catch (OperationCanceledException)
        {
            // Do nothing - cancellation is expected when the UI is reset multiple times
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method ResetUiAsync.");
        }
    }
}
