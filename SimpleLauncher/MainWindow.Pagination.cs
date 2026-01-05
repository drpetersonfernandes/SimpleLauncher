using System;
using System.Windows;
using SimpleLauncher.Services;
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
            if (_isLoadingGames) return;

            if (_currentPage <= 1)
            {
                // If already on the first page, no action needed
                return;
            }

            try
            {
                CancelAndRecreateToken();

                _currentPage--;
                _playSoundEffects.PlayNotificationSound();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);

                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingPreviousPage") ?? "Loading previous page...", this);
            }

            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Previous page button error.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.NavigationButtonErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the PrevPageButtonClickAsync method.");
        }
    }

    private async void NextPageButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            var totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage >= totalPages)
            {
                // If already on the last page, no action needed
                return;
            }

            try
            {
                CancelAndRecreateToken();

                _currentPage++;
                _playSoundEffects.PlayNotificationSound();

                var (sl, sq) = GetLoadGameFilesParams();
                await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);

                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingNextPage") ?? "Loading next page...", this);
            }

            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Next page button error.";
                _ = _logErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.NavigationButtonErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error in the NextPageButtonClickAsync method.");
        }
    }

    private void UpdatePaginationButtons()
    {
        // Only enable if not currently loading
        if (!_isLoadingGames)
        {
            _prevPageButton.IsEnabled = _currentPage > 1;
            _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        }
        else
        {
            _prevPageButton.IsEnabled = false;
            _nextPageButton.IsEnabled = false;
        }
    }
}