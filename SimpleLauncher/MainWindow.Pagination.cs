using System.Windows;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void ResetPaginationButtons()
    {
        UiOrchestrator.ResetPaginationButtons();
    }

    private async void PrevPageButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            if (!UiOrchestrator.CanGoToPrevPage())
            {
                return;
            }

            CancelAndRecreateToken();
            UiOrchestrator.GoToPreviousPage();

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingPrevPage") ?? "Loading previous page...");
            _audioInput.PlayNotificationSound();

            var (sl, sq) = GetLoadGameFilesParams();
            await _gameBrowser.LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            SetLoadingState(false);

            // Notify developer
            const string errorMessage = "Previous page button error.";
            _logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }

    private async void NextPageButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            if (!UiOrchestrator.CanGoToNextPage())
            {
                return;
            }

            CancelAndRecreateToken();
            UiOrchestrator.GoToNextPage();

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingNextPage") ?? "Loading next page...");
            _audioInput.PlayNotificationSound();

            var (sl, sq) = GetLoadGameFilesParams();
            await _gameBrowser.LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            SetLoadingState(false);

            _logErrors.LogAndForget(ex, "Error in the NextPageButtonClickAsync method.");

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }
}
