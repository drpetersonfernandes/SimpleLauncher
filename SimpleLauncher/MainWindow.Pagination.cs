using System.Windows;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void ResetPaginationButtons()
    {
        _paginationService.Reset();
    }

    private async void PrevPageButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            if (!_paginationService.CanGoPrev())
            {
                return;
            }

            CancelAndRecreateToken();
            _paginationService.GoToPreviousPage();

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingPrevPage") ?? "Loading previous page...");
            _playSoundEffects.PlayNotificationSound();

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
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

            if (!_paginationService.CanGoNext())
            {
                return;
            }

            CancelAndRecreateToken();
            _paginationService.GoToNextPage();

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingNextPage") ?? "Loading next page...");
            _playSoundEffects.PlayNotificationSound();

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            SetLoadingState(false);

            // Notify developer
            _logErrors.LogAndForget(ex, "Error in the NextPageButtonClickAsync method.");

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }
}
