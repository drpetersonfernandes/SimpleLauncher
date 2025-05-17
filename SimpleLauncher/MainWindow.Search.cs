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
            PlayClick.PlayNotificationSound();
            await ExecuteSearch();
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method SearchButton_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key == Key.Enter)
            {
                await ExecuteSearch();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the method SearchTextBox_KeyDown.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.MainWindowSearchEngineErrorMessageBox();
        }
    }

    private async Task ExecuteSearch()
    {
        ResetPaginationButtons();

        _currentSearchResults.Clear();

        // Call DeselectLetter to clear any selected letter
        _topLetterNumberMenu.DeselectLetter();

        var searchQuery = SearchTextBox.Text.Trim();

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

            return;
        }

        var searchingpleasewait = (string)Application.Current.TryFindResource("Searchingpleasewait") ??
                                  "Searching, please wait...";
        var pleaseWaitWindow = new PleaseWaitWindow(searchingpleasewait);
        await ShowPleaseWaitWindowAsync(pleaseWaitWindow);

        try
        {
            await LoadGameFilesAsync(null, searchQuery);
        }
        finally
        {
            await ClosePleaseWaitWindowAsync(pleaseWaitWindow);
        }
    }
}