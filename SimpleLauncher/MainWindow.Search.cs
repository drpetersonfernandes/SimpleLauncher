using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("StartingGlobalSearch") ?? "Starting global search...", this);
            _playSoundEffects.PlayNotificationSound();
            await ExecuteSearchAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SearchButton_Click.";
            _ = _logErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key != Key.Enter) return;

            _playSoundEffects.PlayNotificationSound(); // Play sound immediately
            await ExecuteSearchAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method SearchTextBox_KeyDown.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async Task ExecuteSearchAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ExecutingSearch") ?? "Executing search...", this);
        if (_isLoadingGames) return;

        ResetPaginationButtons();

        _currentSearchResults.Clear(); // Clear previous search results

        // Call DeselectLetter to clear any selected letter filter UI
        _topLetterNumberMenu.DeselectLetter();
        _currentFilter = null; // Clear active letter filter

        var searchQuery = SearchTextBox.Text.Trim();
        _activeSearchQueryOrMode = searchQuery; // Set active search mode to the text query

        if (SystemComboBox.SelectedItem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectSystemBeforeSearchMessageBox();
            return;
        }

        if (string.IsNullOrEmpty(searchQuery))
        {
            // Notify user
            MessageBoxLibrary.EnterSearchQueryMessageBox();
            // If search query is empty, we might want to revert to "All" games for the system
            // or do nothing. Current behavior is to show a message and return.
            // If we want to show "All", then _activeSearchQueryOrMode should be null.
            // For now, stick to the message.
            return;
        }

        try
        {
            // LoadGameFilesAsync will use _activeSearchQueryOrMode (which is searchQuery here)
            // and _currentFilter (which is null here). It will also manage the loading indicator.
            await LoadGameFilesAsync(null, searchQuery);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error during search execution.";
            _ = _logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }
}