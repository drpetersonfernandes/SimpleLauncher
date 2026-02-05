using System;
using System.Windows;
using SimpleLauncher.Services.MessageBox;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void ResetPaginationButtons()
    {
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentPage = 1;
        Scroller.ScrollToTop();
        TotalFilesLabel.Content = null;
    }

    private async void PrevPageButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames)
            {
                return;
            }

            if (_currentPage <= 1)
            {
                // If already on the first page, no action needed
                return;
            }

            CancelAndRecreateToken();
            _currentPage--;

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
            _ = _logErrors.LogErrorAsync(ex, errorMessage);

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

            var totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);
            if (_currentPage >= totalPages)
            {
                // If already on the last page, no action needed
                return;
            }

            CancelAndRecreateToken();
            _currentPage++;

            SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingNextPage") ?? "Loading next page...");
            _playSoundEffects.PlayNotificationSound();

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            SetLoadingState(false);

            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the NextPageButtonClickAsync method.");

            // Notify user
            MessageBoxLibrary.NavigationButtonErrorMessageBox();
        }
    }

    private void UpdatePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
    }
}