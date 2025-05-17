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

    private void InitializePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
        Scroller.ScrollToTop();
    }

    private async void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_currentPage <= 1) return;

            _currentPage--;
            if (_currentSearchResults.Count != 0)
            {
                PlayClick.PlayNotificationSound();
                await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
            }
            else
            {
                PlayClick.PlayNotificationSound();
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

    private async void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var totalPages = (int)Math.Ceiling(_totalFiles / (double)_filesPerPage);

            if (_currentPage >= totalPages) return;

            _currentPage++;
            if (_currentSearchResults.Count != 0)
            {
                PlayClick.PlayNotificationSound();
                await LoadGameFilesAsync(searchQuery: SearchTextBox.Text);
            }
            else
            {
                PlayClick.PlayNotificationSound();
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

    private void UpdatePaginationButtons()
    {
        _prevPageButton.IsEnabled = _currentPage > 1;
        _nextPageButton.IsEnabled = _currentPage * _filesPerPage < _totalFiles;
    }
}