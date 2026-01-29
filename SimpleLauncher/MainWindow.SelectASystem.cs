using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.DisplaySystemInfo;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.MessageBox;
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
                if (_isUiUpdating)
                {
                    return; // Prevent re-entrance
                }

                if (SystemComboBox.SelectedItem == null)
                {
                    // Clear the cached list when no system is selected
                    _allGamesForCurrentSystem.Clear();
                    IsPlayTimeVisible = false; // Hide when no system is selected

                    return;
                }

                SetUiLoadingState(true, (string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...");
                await Task.Delay(100); // Give the UI thread time to render the loading overlay
                _isUiUpdating = true; // Set after UI is frozen
                try
                {
                    SearchTextBox.Text = "";
                    EmulatorComboBox.ItemsSource = null;
                    EmulatorComboBox.SelectedIndex = -1;
                    PreviewImage.Source = null;

                    _currentSearchResults.Clear();
                    _currentFilter = null;
                    _activeSearchQueryOrMode = null;

                    GameFileGrid.Visibility = Visibility.Visible;
                    ListViewPreviewArea.Visibility = Visibility.Collapsed;

                    var selectedSystem = SystemComboBox.SelectedItem?.ToString();

                    // // --- "Microsoft Windows" rescan prompt ---
                    // if (await ReScanMicrosoftWindowsSystem(selectedSystem)) return; // Exit after handling rescan, do not proceed with normal load for this selection

                    var selectedManager = _systemManagers.FirstOrDefault(c => c.SystemName == selectedSystem);
                    if (selectedSystem == null || selectedManager == null)
                    {
                        // Notify developer
                        const string errorMessage = "Selected system or its configuration is null.";
                        _ = _logErrors.LogErrorAsync(null, errorMessage);

                        // Notify user
                        MessageBoxLibrary.InvalidSystemConfigMessageBox();
                        SortOrderToggleButton.Visibility = Visibility.Collapsed;

                        SystemComboBox.SelectedItem = null;
                        await DisplaySystemSelectionScreenAsync();

                        // Clear the cached list on error
                        _allGamesForCurrentSystem.Clear();

                        return;
                    }

                    var formats = selectedManager.FileFormatsToSearch;
                    IsPlayTimeVisible = formats == null || !formats.Any(static f =>
                        f.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                        f.Equals("lnk", StringComparison.OrdinalIgnoreCase));

                    _mameSortOrder = "FileName";
                    UpdateSortOrderButtonUi();
                    SortOrderToggleButton.Visibility = selectedManager.SystemIsMame ? Visibility.Visible : Visibility.Collapsed;

                    EmulatorComboBox.ItemsSource = selectedManager.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                    if (EmulatorComboBox.Items.Count > 0)
                    {
                        EmulatorComboBox.SelectedIndex = 0;
                    }

                    SelectedSystem = selectedSystem;

                    var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == selectedSystem);
                    PlayTime = systemPlayTime != null ? systemPlayTime.PlayTime : "00:00:00";

                    // Display SystemInfo and get the validation result. Game count is now handled inside this method.
                    var validationResult = await DisplaySystemInformation.DisplaySystemInfoAsync(selectedManager, _gameFileGrid);

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

                    _topLetterNumberMenu.DeselectLetter();
                    ResetPaginationButtons();
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string errorMessage = "Error in the method SystemComboBoxSelectionChangedAsync.";
                    _ = _logErrors.LogErrorAsync(ex, errorMessage);

                    // Notify user
                    MessageBoxLibrary.InvalidSystemConfigMessageBox();

                    // Clear cached list on error
                    _allGamesForCurrentSystem.Clear();
                }
                finally
                {
                    SetUiLoadingState(false);
                    _isUiUpdating = false;
                }
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error in SystemComboBoxSelectionChangedAsync.");
            }

            return;

            async Task PopulateAllGamesForCurrentSystem(SystemManager selectedManager, string currentSelectedSystem)
            {
                var uniqueFilesForSystem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var folder in _selectedRomFolders)
                {
                    // This part can run concurrently with other file system operations, but not modify _allGamesForCurrentSystem
                    var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
                    if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || selectedManager.FileFormatsToSearch == null) continue;

                    var filesInFolder = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch);
                    foreach (var file in filesInFolder)
                    {
                        uniqueFilesForSystem.TryAdd(Path.GetFileName(file), file);
                    }
                }

                await _allGamesLock.WaitAsync(); // Acquire lock before modifying _allGamesForCurrentSystem
                try
                {
                    _allGamesForCurrentSystem = uniqueFilesForSystem.Values.ToList(); // WRITE
                    DebugLogger.Log($"[SystemComboBoxSelectionChangedAsync] Populated _allGamesForCurrentSystem for '{currentSelectedSystem}'. Count: {_allGamesForCurrentSystem.Count}");
                }
                finally
                {
                    _allGamesLock.Release(); // Release lock
                }
            }
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in SystemComboBoxSelectionChangedAsync.");
        }
    }
}