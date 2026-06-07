using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.DisplaySystemInfo;
using SimpleLauncher.Services.GameItemRender;
using SimpleLauncher.Services.MenuActionHandler;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UIReset;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async void SystemComboBoxSelectionChangedAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            try
            {
                if (((IUiResetHost)this).IsUiUpdating)
                {
                    return; // Prevent re-entrance
                }

                if (SystemComboBox.SelectedItem == null)
                {
                    // Clear the cached list when no system is selected
                    await _gameCacheService.InvalidateAsync(CancellationToken.None);
                    IsPlayTimeVisible = false; // Hide when no system is selected

                    // Stop watching ROM folders when no system is selected
                    _gameFileWatcherService.StopWatching();

                    return;
                }

                ((IUiResetHost)this).IsUiUpdating = true;
                SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...");
                await Task.Delay(100, _cancellationSource.Token); // Give the UI thread time to render the loading overlay
                try
                {
                    SearchTextBox.Text = "";
                    EmulatorComboBox.ItemsSource = null;
                    EmulatorComboBox.SelectedIndex = -1;
                    PreviewImage.Source = null;

                    await _gameCacheService.SetSearchResultsAsync([], _cancellationSource.Token);
                    ((IUiResetHost)this).CurrentFilter = null;
                    ((IUiResetHost)this).ActiveSearchQueryOrMode = null;

                    GameFileGrid.Visibility = Visibility.Visible;
                    ListViewPreviewArea.Visibility = Visibility.Collapsed;

                    var selectedSystem = SystemComboBox.SelectedItem?.ToString();

                    // // --- "Microsoft Windows" rescan prompt ---
                    // if (await ReScanMicrosoftWindowsSystem(selectedSystem)) return; // Exit after handling rescan, do not proceed with normal load for this selection

                    var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
                    if (selectedSystem == null || selectedManager == null)
                    {
                        // Notify developer
                        const string errorMessage = "Selected system or its configuration is null.";
                        _logErrors.LogAndForget(null, errorMessage);

                        // Notify user
                        MessageBoxLibrary.InvalidSystemConfigMessageBox();
                        SortOrderToggleButton.Visibility = Visibility.Collapsed;

                        SystemComboBox.SelectedItem = null;
                        await DisplaySystemSelectionScreenAsync(((IMenuActionHost)this).CurrentCancellationToken);

                        // Clear the cached list on error
                        await _gameCacheService.InvalidateAsync(_cancellationSource.Token);

                        return;
                    }

                    var formats = selectedManager.FileFormatsToSearch;
                    IsPlayTimeVisible = formats == null || !formats.Any(static f =>
                        f.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                        f.Equals("lnk", StringComparison.OrdinalIgnoreCase));

                    ((IUiResetHost)this).MameSortOrder = "FileName";
                    UpdateSortOrderButtonUi();
                    SortOrderToggleButton.Visibility = Visibility.Visible;

                    EmulatorComboBox.ItemsSource = selectedManager.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                    if (EmulatorComboBox.Items.Count > 0)
                    {
                        EmulatorComboBox.SelectedIndex = 0;
                    }

                    SelectedSystem = selectedSystem;

                    var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
                    PlayTime = systemPlayTime != null ? systemPlayTime.FormattedPlayTime : "00:00:00";

                    // Display SystemInfo and get the validation result. Game count is now handled inside this method.
                    var validationResult = await DisplaySystemInformation.DisplaySystemInfoAsync(selectedManager, ((IGameItemRenderHost)this).GameFileGrid, ((IMenuActionHost)this).CurrentCancellationToken);

                    // If validation failed, show the message box with aggregated errors
                    if (!validationResult.IsValid)
                    {
                        var errorMessages = new StringBuilder();
                        foreach (var msg in validationResult.ErrorMessages)
                        {
                            errorMessages.Append(msg);
                        }

                        MessageBoxLibrary.ListOfErrorsMessageBox(errorMessages);
                    }

                    // Resolve the system image folder path using PathHelper
                    var resolvedSystemImageFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedManager.SystemImageFolder);

                    _selectedRomFolders = selectedManager.SystemFolders.Select(PathHelper.ResolveRelativeToAppDirectory).ToList();
                    _selectedImageFolder = string.IsNullOrWhiteSpace(resolvedSystemImageFolderPath)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedManager.SystemName) // Use the default resolved path
                        : resolvedSystemImageFolderPath; // Use resolved configured path

                    await PopulateAllGamesForCurrentSystem(selectedManager, selectedSystem);

                    // Start watching ROM folders for external file changes
                    _gameFileWatcherService.StartWatching(
                        selectedManager.SystemFolders,
                        selectedSystem,
                        selectedManager.FileFormatsToSearch);

                    _topLetterNumberMenu.DeselectLetter();
                    ResetPaginationButtons();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string errorMessage = "Error in the method SystemComboBoxSelectionChangedAsync.";
                    _logErrors.LogAndForget(ex, errorMessage);

                    // Notify user
                    MessageBoxLibrary.InvalidSystemConfigMessageBox();

                    // Clear cached list on error
                    await _gameCacheService.InvalidateAsync(_cancellationSource.Token);
                }
                finally
                {
                    SetLoadingState(false);
                    ((IUiResetHost)this).IsUiUpdating = false;
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in SystemComboBoxSelectionChangedAsync.");
            }

            return;

            async Task PopulateAllGamesForCurrentSystem(SystemManager selectedManager, string currentSelectedSystem)
            {
                var uniqueFilesForSystem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var folder in _selectedRomFolders)
                {
                    var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                    if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || selectedManager.FileFormatsToSearch == null) continue;

                    var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager, ((IMenuActionHost)this).CurrentCancellationToken);
                    foreach (var file in filesInFolder)
                    {
                        uniqueFilesForSystem.TryAdd(Path.GetFileName(file), file);
                    }
                }

                await _gameCacheService.SetAllGamesAsync(uniqueFilesForSystem.Values.ToList(), currentSelectedSystem, ((IMenuActionHost)this).CurrentCancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SystemComboBoxSelectionChangedAsync.");
        }
    }
}
