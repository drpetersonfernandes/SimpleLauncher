using System.Windows;
using System.Windows.Input;

namespace SimpleLauncher;

using Interfaces;

/// <summary>
/// Partial MainWindow containing search button handlers and search text box interaction logic.
/// </summary>
public partial class MainWindow
{
    private async void SearchButtonClickAsync(object sender, RoutedEventArgs e)
    {
        if (_isDisposed) return;
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("Searching") ?? "Searching...");
            _audioInput.PlayNotificationSound();
            await ExecuteSearchAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SearchButtonClickAsync.";
            _logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            await _messageBox.MainWindowSearchEngineErrorMessageBoxAsync();
        }
    }

    private async void SearchTextBoxKeyDownAsync(object sender, KeyEventArgs e)
    {
        if (_isDisposed) return;
        try
        {
            if (e.Key != Key.Enter) return;

            _audioInput.PlayNotificationSound(); // Play sound immediately
            await ExecuteSearchAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method SearchTextBoxKeyDownAsync.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBox.MainWindowSearchEngineErrorMessageBoxAsync();
        }
    }

    private async Task ExecuteSearchAsync()
    {
        if (_isLoadingGames) return;

        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ExecutingSearch") ?? "Executing search...");
        var searchingMsg = (string)Application.Current.TryFindResource("Searchingpleasewait") ?? "Searching... Please wait.";
        SetLoadingState(true, searchingMsg);

        try
        {
            CancelAndRecreateToken();
            ResetPaginationButtons();

            var searchQuery = SearchTextBox.Text.Trim();
            ((IUiResetHost)this).ActiveSearchQueryOrMode = searchQuery;

            var selectedSystem = SystemComboBox.SelectedItem?.ToString();
            var result = await _gameBrowser.ValidateAndPrepareAsync(searchQuery, selectedSystem, _cancellationSource.Token);

            if (!result.IsValid)
            {
                if (SystemComboBox.SelectedItem == null)
                {
                    await _messageBox.SelectSystemBeforeSearchMessageBoxAsync();
                }
                else
                {
                    await _messageBox.EnterSearchQueryMessageBoxAsync();
                }

                return;
            }

            _topLetterNumberMenu.DeselectLetter();
            ((IUiResetHost)this).CurrentFilter = null;

            try
            {
                await _gameBrowser.LoadGameFilesAsync(null, result.ValidatedQuery, _cancellationSource.Token);
            }
            catch (Exception ex)
            {
                const string contextMessage = "Error during search execution.";
                _logErrors.LogAndForget(ex, contextMessage);

                await _messageBox.MainWindowSearchEngineErrorMessageBoxAsync();
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}
