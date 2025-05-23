using System;
using System.Windows;
using SimpleLauncher.Services;

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

    private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            if (_currentPage <= 1)
            {
                // If already on the first page, no action needed
                return;
            }

            try
            {
                _currentPage--;

                PlayClick.PlayNotificationSound();

                if (_currentSearchResults.Count != 0)
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Previous page button error.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.NavigationButtonErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the PrevPageButton_Click method.");
        }
    }

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isGameListLoading) return;

            var totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage >= totalPages)
            {
                // If already on the last page, no action needed
                return;
            }

            try
            {
                _currentPage++;

                PlayClick.PlayNotificationSound();

                if (_currentSearchResults.Count != 0)
                {
                    await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
                }
                else
                {
                    await LoadGameFilesAsync(_currentFilter);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string errorMessage = "Next page button error.";
                _ = LogErrors.LogErrorAsync(ex, errorMessage);

                // Notify user
                MessageBoxLibrary.NavigationButtonErrorMessageBox();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error in the NextPageButton_Click method.");
        }
    }

    private void UpdatePaginationButtons()
    {
        // Only enable if not currently loading
        if (!_isGameListLoading)
        {
            _prevPageButton.IsEnabled = _currentPage > 1;
            _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        }
        else
        {
            _prevPageButton.IsEnabled = false;
            _nextPageButton.IsEnabled = false;
        }

        Scroller.ScrollToTop();
    }
}
