using System.Windows;
using System.Windows.Input;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UIReset;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async void SearchButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("Searching") ?? "Searching...", this);
            _playSoundEffects.PlayNotificationSound();
            await ExecuteSearchAsync();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SearchButtonClickAsync.";
            _logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async void SearchTextBoxKeyDownAsync(object sender, KeyEventArgs e)
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
            const string contextMessage = "Error in the method SearchTextBoxKeyDownAsync.";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async Task ExecuteSearchAsync()
    {
        if (_isLoadingGames) return;

        UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("ExecutingSearch") ?? "Executing search...", this);
        var searchingMsg = (string)Application.Current.TryFindResource("Searchingpleasewait") ?? "Searching... Please wait.";
        SetLoadingState(true, searchingMsg);

        try
        {
            CancelAndRecreateToken();
            ResetPaginationButtons();

            var searchQuery = SearchTextBox.Text.Trim();
            ((IUiResetHost)this).ActiveSearchQueryOrMode = searchQuery;

            var selectedSystem = SystemComboBox.SelectedItem?.ToString();
            var result = await _searchOrchestratorService.ValidateAndPrepareAsync(searchQuery, selectedSystem, _cancellationSource.Token);

            if (!result.IsValid)
            {
                if (SystemComboBox.SelectedItem == null)
                {
                    MessageBoxLibrary.SelectSystemBeforeSearchMessageBox();
                }
                else
                {
                    MessageBoxLibrary.EnterSearchQueryMessageBox();
                }

                SetLoadingState(false);
                return;
            }

            _topLetterNumberMenu.DeselectLetter();
            ((IUiResetHost)this).CurrentFilter = null;

            try
            {
                await LoadGameFilesAsync(null, result.ValidatedQuery, _cancellationSource.Token);
                SetLoadingState(false);
            }
            catch (Exception ex)
            {
                SetLoadingState(false);

                const string contextMessage = "Error during search execution.";
                _logErrors.LogAndForget(ex, contextMessage);

                MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}