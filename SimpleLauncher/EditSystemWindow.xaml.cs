using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.CheckApplicationControlPolicy;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SimpleLauncher;

internal partial class EditSystemWindow : ILoadingState
{
    private List<SystemManager> _systems;
    private static readonly char[] SplitSeparators = [',', '|', ';'];
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IHelpUserService _helpUserService;
    private readonly IImageLoader _imageLoader;
    private string _originalSystemName;
    private readonly IConfiguration _configuration;
    private readonly string _preSelectedSystemName;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;

    public EditSystemWindow(SettingsManager settings, PlaySoundEffects playSoundEffects, IConfiguration configuration, ILogErrors logErrors, IHelpUserService helpUserService, IImageLoader imageLoader, IMessageBoxLibraryService messageBox, QuitSimpleLauncher quitSimpleLauncher, string preSelectedSystemName = null)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _configuration = configuration;
        _settings = settings;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _helpUserService = helpUserService;
        _imageLoader = imageLoader;
        _preSelectedSystemName = preSelectedSystemName;
        _messageBox = messageBox;
        _quitSimpleLauncher = quitSimpleLauncher;

        ApplyExpanderSettings();

        _ = LoadSystemsAsync();

        Closing += EditSystem_Closing;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };

        SaveSystemButton.IsEnabled = false;
        DeleteSystemButton.IsEnabled = false;
    }

    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            MainContentGrid.IsEnabled = !isLoading;
            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    private void ApplyExpanderSettings()
    {
        AdditionalFoldersExpander.IsExpanded = _settings.AdditionalSystemFoldersExpanded;
        Emulator1Expander.IsExpanded = _settings.Emulator1Expanded;
        Emulator2Expander.IsExpanded = _settings.Emulator2Expanded;
        Emulator3Expander.IsExpanded = _settings.Emulator3Expanded;
        Emulator4Expander.IsExpanded = _settings.Emulator4Expanded;
        Emulator5Expander.IsExpanded = _settings.Emulator5Expanded;
    }

    private async Task LoadSystemsAsync()
    {
        try
        {
            SetLoadingState(true, (string)Application.Current.TryFindResource("Loadingsystems") ?? "Loading systems...");
            var systems = await Task.Run(() => SystemManager.LoadSystemManagers(_configuration));

            if (systems == null)
            {
                // Notify user on UI thread
                await _messageBox.SystemXmlNotFoundMessageBox();
                _quitSimpleLauncher.SimpleQuitApplication(); // Or just Close();
            }
            else
            {
                _systems = systems;
                PopulateSystemNamesDropdown();

                if (!string.IsNullOrEmpty(_preSelectedSystemName))
                {
                    SystemNameDropdown.SelectedItem = _preSelectedSystemName;
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error loading systems into Edit window.");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void ChooseSystemFolder(object sender, RoutedEventArgs e)
    {
        var pleaseselecttheSystemFolder2 = (string)Application.Current.TryFindResource("PleaseselecttheSystemFolder") ?? "Please select the System Folder";

        // Create new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = pleaseselecttheSystemFolder2
        };

        // Show dialog and handle result
        if (openFolderDialog.ShowDialog() != true) return;

        var foldername = openFolderDialog.FolderName;
        SystemFolderTextBox.Text = foldername;

        MarkValid(SystemFolderTextBox);
    }

    private void ChooseSystemImageFolder(object sender, RoutedEventArgs e)
    {
        var pleaseselecttheSystemImage2 = (string)Application.Current.TryFindResource("PleaseselecttheSystemImage") ?? "Please select the System Image Folder";

        // Create new OpenFolderDialog
        var openFolderDialog = new OpenFolderDialog
        {
            Title = pleaseselecttheSystemImage2
        };

        // Show dialog and handle result
        var result = openFolderDialog.ShowDialog();

        if (result != true) return;

        var foldername = openFolderDialog.FolderName.Trim();
        SystemImageFolderTextBox.Text = foldername;
        MarkValid(SystemImageFolderTextBox);
    }

    private void ChooseEmulator1Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator12 = (string)Application.Current.TryFindResource("SelectEmulator1") ?? "Select Emulator 1";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Executable Files (*.exe;*.bat)|*.exe;*.bat",
            Title = selectEmulator12
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator1PathTextBox.Text = filename;
            MarkValid(Emulator1PathTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private void ChooseEmulator2Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator22 = (string)Application.Current.TryFindResource("SelectEmulator2") ?? "Select Emulator 2";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Executable Files (*.exe;*.bat)|*.exe;*.bat",
            Title = selectEmulator22
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator2PathTextBox.Text = filename;
            MarkValid(Emulator2PathTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private void ChooseEmulator3Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator32 = (string)Application.Current.TryFindResource("SelectEmulator3") ?? "Select Emulator 3";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Executable Files (*.exe;*.bat)|*.exe;*.bat",
            Title = selectEmulator32
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator3PathTextBox.Text = filename;
            MarkValid(Emulator3PathTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private void ChooseEmulator4Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator42 = (string)Application.Current.TryFindResource("SelectEmulator4") ?? "Select Emulator 4";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Executable Files (*.exe;*.bat)|*.exe;*.bat",
            Title = selectEmulator42
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator4PathTextBox.Text = filename;
            MarkValid(Emulator4PathTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private void ChooseEmulator5Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator52 = (string)Application.Current.TryFindResource("SelectEmulator5") ?? "Select Emulator 5";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "Executable Files (*.exe;*.bat)|*.exe;*.bat",
            Title = selectEmulator52
        };
        var result = dialog.ShowDialog();
        if (result == true)
        {
            var filename = dialog.FileName;
            Emulator5PathTextBox.Text = filename;
            MarkValid(Emulator5PathTextBox);
        }

        // Update the HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private async void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _originalSystemName = null;

            EnableFields();
            ClearFields();
            SystemNameDropdown.SelectedItem = null;
            ClearFieldsForNoSelection();
            EnableFields();

            HelpUserTextBlock.Document.Blocks.Clear();

            SaveSystemButton.IsEnabled = true;
            DeleteSystemButton.IsEnabled = false;

            // Notify user
            await _messageBox.YouCanAddANewSystemMessageBox();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in the method AddSystemButton_Click.");
        }
    }

    private void EnableFields()
    {
        SystemNameTextBox.IsReadOnly = false;
        SystemNameTextBox.IsEnabled = true;

        SystemFolderTextBox.IsReadOnly = false;
        SystemFolderTextBox.IsEnabled = true;
        AdditionalFoldersListBox.IsEnabled = true;
        AddFolderButton.IsEnabled = true;
        RemoveFolderButton.IsEnabled = true;

        SystemImageFolderTextBox.IsReadOnly = false;
        SystemImageFolderTextBox.IsEnabled = true;

        FormatToSearchTextBox.IsReadOnly = false;
        FormatToSearchTextBox.IsEnabled = true;

        ExtractFileBeforeLaunchComboBox.IsEnabled = true;

        GroupByFolderComboBox.IsEnabled = true;

        DisableRecursiveSearchComboBox.IsEnabled = true;

        FormatToLaunchTextBox.IsReadOnly = false;
        FormatToLaunchTextBox.IsEnabled = true;

        Emulator1NameTextBox.IsReadOnly = false;
        Emulator1NameTextBox.IsEnabled = true;
        Emulator1PathTextBox.IsReadOnly = false;
        Emulator1PathTextBox.IsEnabled = true;
        Emulator1ParametersTextBox.IsReadOnly = false;
        Emulator1ParametersTextBox.IsEnabled = true;
        ReceiveANotificationOnEmulatorError1.IsEnabled = true;

        Emulator2NameTextBox.IsReadOnly = false;
        Emulator2NameTextBox.IsEnabled = true;
        Emulator2PathTextBox.IsReadOnly = false;
        Emulator2PathTextBox.IsEnabled = true;
        Emulator2ParametersTextBox.IsReadOnly = false;
        Emulator2ParametersTextBox.IsEnabled = true;
        ReceiveANotificationOnEmulatorError2.IsEnabled = true;

        Emulator3NameTextBox.IsReadOnly = false;
        Emulator3NameTextBox.IsEnabled = true;
        Emulator3PathTextBox.IsReadOnly = false;
        Emulator3PathTextBox.IsEnabled = true;
        Emulator3ParametersTextBox.IsReadOnly = false;
        Emulator3ParametersTextBox.IsEnabled = true;
        ReceiveANotificationOnEmulatorError3.IsEnabled = true;

        Emulator4NameTextBox.IsReadOnly = false;
        Emulator4NameTextBox.IsEnabled = true;
        Emulator4PathTextBox.IsReadOnly = false;
        Emulator4PathTextBox.IsEnabled = true;
        Emulator4ParametersTextBox.IsReadOnly = false;
        Emulator4ParametersTextBox.IsEnabled = true;
        ReceiveANotificationOnEmulatorError4.IsEnabled = true;

        Emulator5NameTextBox.IsReadOnly = false;
        Emulator5NameTextBox.IsEnabled = true;
        Emulator5PathTextBox.IsReadOnly = false;
        Emulator5PathTextBox.IsEnabled = true;
        Emulator5ParametersTextBox.IsReadOnly = false;
        Emulator5ParametersTextBox.IsEnabled = true;
        ReceiveANotificationOnEmulatorError5.IsEnabled = true;

        ChooseSystemFolderButton.IsEnabled = true;
        ChooseSystemImageFolderButton.IsEnabled = true;
        ChooseEmulator1PathButton.IsEnabled = true;
        ChooseEmulator2PathButton.IsEnabled = true;
        ChooseEmulator3PathButton.IsEnabled = true;
        ChooseEmulator4PathButton.IsEnabled = true;
        ChooseEmulator5PathButton.IsEnabled = true;
        ChooseSystemImageButton.IsEnabled = true;
        SuggestEmulator1ParametersButton.IsEnabled = true;
        SuggestEmulator2ParametersButton.IsEnabled = true;
        SuggestEmulator3ParametersButton.IsEnabled = true;
        SuggestEmulator4ParametersButton.IsEnabled = true;
        SuggestEmulator5ParametersButton.IsEnabled = true;
    }

    private void DisableAllEditableFields()
    {
        SystemNameTextBox.IsReadOnly = true;
        SystemNameTextBox.IsEnabled = false;

        SystemFolderTextBox.IsReadOnly = true;
        SystemFolderTextBox.IsEnabled = false;
        AdditionalFoldersListBox.IsEnabled = false;
        AddFolderButton.IsEnabled = false;
        RemoveFolderButton.IsEnabled = false;

        SystemImageFolderTextBox.IsReadOnly = true;
        SystemImageFolderTextBox.IsEnabled = false;

        FormatToSearchTextBox.IsReadOnly = true;
        FormatToSearchTextBox.IsEnabled = false;

        ExtractFileBeforeLaunchComboBox.IsEnabled = false;

        GroupByFolderComboBox.IsEnabled = false;

        DisableRecursiveSearchComboBox.IsEnabled = false;

        FormatToLaunchTextBox.IsReadOnly = true;
        FormatToLaunchTextBox.IsEnabled = false;

        Emulator1NameTextBox.IsReadOnly = true;
        Emulator1NameTextBox.IsEnabled = false;
        Emulator1PathTextBox.IsReadOnly = true;
        Emulator1PathTextBox.IsEnabled = false;
        Emulator1ParametersTextBox.IsReadOnly = true;
        Emulator1ParametersTextBox.IsEnabled = false;
        ReceiveANotificationOnEmulatorError1.IsEnabled = false;

        Emulator2NameTextBox.IsReadOnly = true;
        Emulator2NameTextBox.IsEnabled = false;
        Emulator2PathTextBox.IsReadOnly = true;
        Emulator2PathTextBox.IsEnabled = false;
        Emulator2ParametersTextBox.IsReadOnly = true;
        Emulator2ParametersTextBox.IsEnabled = false;
        ReceiveANotificationOnEmulatorError2.IsEnabled = false;

        Emulator3NameTextBox.IsReadOnly = true;
        Emulator3NameTextBox.IsEnabled = false;
        Emulator3PathTextBox.IsReadOnly = true;
        Emulator3PathTextBox.IsEnabled = false;
        Emulator3ParametersTextBox.IsReadOnly = true;
        Emulator3ParametersTextBox.IsEnabled = false;
        ReceiveANotificationOnEmulatorError3.IsEnabled = false;

        Emulator4NameTextBox.IsReadOnly = true;
        Emulator4NameTextBox.IsEnabled = false;
        Emulator4PathTextBox.IsReadOnly = true;
        Emulator4PathTextBox.IsEnabled = false;
        Emulator4ParametersTextBox.IsReadOnly = true;
        Emulator4ParametersTextBox.IsEnabled = false;
        ReceiveANotificationOnEmulatorError4.IsEnabled = false;

        Emulator5NameTextBox.IsReadOnly = true;
        Emulator5NameTextBox.IsEnabled = false;
        Emulator5PathTextBox.IsReadOnly = true;
        Emulator5PathTextBox.IsEnabled = false;
        Emulator5ParametersTextBox.IsReadOnly = true;
        Emulator5ParametersTextBox.IsEnabled = false;
        ReceiveANotificationOnEmulatorError5.IsEnabled = false;

        ChooseSystemFolderButton.IsEnabled = false;
        ChooseSystemImageFolderButton.IsEnabled = false;
        ChooseEmulator1PathButton.IsEnabled = false;
        ChooseEmulator2PathButton.IsEnabled = false;
        ChooseEmulator3PathButton.IsEnabled = false;
        ChooseEmulator4PathButton.IsEnabled = false;
        ChooseEmulator5PathButton.IsEnabled = false;
        ChooseSystemImageButton.IsEnabled = false;
        SuggestEmulator1ParametersButton.IsEnabled = false;
        SuggestEmulator2ParametersButton.IsEnabled = false;
        SuggestEmulator3ParametersButton.IsEnabled = false;
        SuggestEmulator4ParametersButton.IsEnabled = false;
        SuggestEmulator5ParametersButton.IsEnabled = false;
    }

    private void ClearFields()
    {
        SystemNameDropdown.SelectedItem = null;
        ClearFieldsForNoSelection();
    }

    // Clears fields when no system is selected, without affecting SystemNameDropdown itself.
    private void ClearFieldsForNoSelection()
    {
        SystemNameTextBox.Text = "";
        MarkValid(SystemNameTextBox);

        SystemFolderTextBox.Text = "";
        MarkValid(SystemFolderTextBox);
        AdditionalFoldersListBox.Items.Clear();

        SystemImageFolderTextBox.Text = "";
        MarkValid(SystemImageFolderTextBox);

        FormatToSearchTextBox.Text = "";
        MarkValid(FormatToSearchTextBox);

        ExtractFileBeforeLaunchComboBox.SelectedItem = null;

        GroupByFolderComboBox.SelectedItem = null;

        DisableRecursiveSearchComboBox.SelectedItem = null;

        FormatToLaunchTextBox.Text = "";
        MarkValid(FormatToLaunchTextBox);

        ClearAllEmulatorFieldsInternal();
        UpdateSystemImagePreview();
    }

    private void ClearAllEmulatorFieldsInternal()
    {
        Emulator1NameTextBox.Text = "";
        MarkValid(Emulator1NameTextBox);
        Emulator1PathTextBox.Text = "";
        MarkValid(Emulator1PathTextBox);
        Emulator1ParametersTextBox.Text = "";
        MarkValid(Emulator1ParametersTextBox);
        ReceiveANotificationOnEmulatorError1.SelectedItem = null;

        Emulator2NameTextBox.Text = "";
        MarkValid(Emulator2NameTextBox);
        Emulator2PathTextBox.Text = "";
        MarkValid(Emulator2PathTextBox);
        Emulator2ParametersTextBox.Text = "";
        MarkValid(Emulator2ParametersTextBox);
        ReceiveANotificationOnEmulatorError2.SelectedItem = null;

        Emulator3NameTextBox.Text = "";
        MarkValid(Emulator3NameTextBox);
        Emulator3PathTextBox.Text = "";
        MarkValid(Emulator3PathTextBox);
        Emulator3ParametersTextBox.Text = "";
        MarkValid(Emulator3ParametersTextBox);
        ReceiveANotificationOnEmulatorError3.SelectedItem = null;

        Emulator4NameTextBox.Text = "";
        MarkValid(Emulator4NameTextBox);
        Emulator4PathTextBox.Text = "";
        MarkValid(Emulator4PathTextBox);
        Emulator4ParametersTextBox.Text = "";
        MarkValid(Emulator4ParametersTextBox);
        ReceiveANotificationOnEmulatorError4.SelectedItem = null;

        Emulator5NameTextBox.Text = "";
        MarkValid(Emulator5NameTextBox);
        Emulator5PathTextBox.Text = "";
        MarkValid(Emulator5PathTextBox);
        Emulator5ParametersTextBox.Text = "";
        MarkValid(Emulator5ParametersTextBox);
        ReceiveANotificationOnEmulatorError5.SelectedItem = null;
    }

    private async void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            HelpUserTextBlock.Document.Blocks.Clear();

            if (SystemNameDropdown.SelectedItem == null)
            {
                // Notify user
                await _messageBox.SelectASystemToDeleteMessageBox();
                return;
            }

            var selectedSystemName = SystemNameDropdown.SelectedItem.ToString();

            var result = await _messageBox.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
            if (result != CoreMessageBoxResult.Yes) return;

            await SystemManager.DeleteSystemAsync(selectedSystemName, _logErrors);
            _playSoundEffects.PlayNotificationSound();

            await LoadSystemsAsync();
            if (SystemNameDropdown.Items.Count == 0 || SystemNameDropdown.SelectedItem == null)
            {
                PopulateSystemNamesDropdown();
            }

            // Notify user
            await _messageBox.SystemHasBeenDeletedMessageBox(selectedSystemName);
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"Error in method DeleteSystemButton_Click: {ex.Message}");
            _logErrors.LogAndForget(ex, "Error in method DeleteSystemButton_Click");
        }
    }

    private void EditSystem_Closing(object sender, CancelEventArgs e)
    {
        // Save expander states
        _settings.AdditionalSystemFoldersExpanded = AdditionalFoldersExpander.IsExpanded;
        _settings.Emulator1Expanded = Emulator1Expander.IsExpanded;
        _settings.Emulator2Expanded = Emulator2Expander.IsExpanded;
        _settings.Emulator3Expanded = Emulator3Expander.IsExpanded;
        _settings.Emulator4Expanded = Emulator4Expander.IsExpanded;
        _settings.Emulator5Expanded = Emulator5Expander.IsExpanded;
        _settings.SaveAsync();

        // Create a backup file
        var appFolderPath = AppDomain.CurrentDomain.BaseDirectory;
        var sourceFilePath = Path.Combine(appFolderPath, "system.xml");
        var backupFileName = $"system_backup{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var backupFilePath = Path.Combine(appFolderPath, backupFileName);

        if (!File.Exists(sourceFilePath)) return;

        try
        {
            File.Copy(sourceFilePath, backupFilePath, true);
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Error in method EditSystem_Closing");
        }
    }

    private async void HelpLink_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            var searchUrl = _configuration.GetValue<string>("WikiParametersUrl") ?? "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters/";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = searchUrl,
                    UseShellExecute = true
                });
            }
            catch (Win32Exception ex) // Catch Win32Exception specifically
            {
                if (CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
                {
                    // Specific message for application control policy blocking links
                    await _messageBox.ApplicationControlPolicyBlockedManualLinkMessageBox(searchUrl);
                    _logErrors.LogAndForget(ex, "Application control policy blocked opening HelpLink.");
                }
                else
                {
                    // Existing error handling for other Win32Exceptions
                    _logErrors.LogAndForget(ex, "Error in method HelpLink_Click");
                    await _messageBox.ErrorOpeningUrlMessageBox();
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in method HelpLink_Click");
                await _messageBox.ErrorOpeningUrlMessageBox();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method HelpLink_Click");
        }
    }

    private void SystemNameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Update HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        _helpUserService.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox.Text.Trim());
    }

    private void AddFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var title = (string)Application.Current.TryFindResource("SelectAdditionalSystemFolder") ?? "Select an additional system folder";
        var openFolderDialog = new OpenFolderDialog { Title = title };
        if (openFolderDialog.ShowDialog() == true)
        {
            AdditionalFoldersListBox.Items.Add(openFolderDialog.FolderName);
        }
    }

    private void RemoveFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (AdditionalFoldersListBox.SelectedItem != null)
        {
            AdditionalFoldersListBox.Items.Remove(AdditionalFoldersListBox.SelectedItem);
        }
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        // Hide overlay and re-enable UI
        LoadingOverlay.Visibility = Visibility.Collapsed;
        MainContentGrid?.IsEnabled = true;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in EditSystemWindow.");
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }

    private async void ChooseSystemImageButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var systemName = SystemNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(systemName))
            {
                await _messageBox.SystemNameRequiredBeforeChoosingImageMessageBox();
                return;
            }

            var dialog = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Title = "Select System Image"
            };

            if (dialog.ShowDialog() != true) return;

            var sourceFilePath = dialog.FileName;
            var extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            if (extension != ".png" && extension != ".jpg" && extension != ".jpeg")
            {
                await _messageBox.InvalidImageFormatMessageBox();
                return;
            }

            var imagesSystemsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "systems");
            try
            {
                if (!Directory.Exists(imagesSystemsDir))
                {
                    Directory.CreateDirectory(imagesSystemsDir);
                }

                var destFilePath = Path.Combine(imagesSystemsDir, $"{systemName}{extension}");
                SystemImagePreview.Source = null; // Release any file lock before overwriting

                const int maxRetries = 3;
                const int retryDelayMs = 500;
                for (var attempt = 1; attempt <= maxRetries; attempt++)
                {
                    try
                    {
                        File.Copy(sourceFilePath, destFilePath, true);
                        break;
                    }
                    catch (IOException) when (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelayMs * attempt);
                    }
                }

                UpdateSystemImagePreview();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error copying system image.");
                await _messageBox.FailedToCopySystemImageMessageBox(ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error copying system image.");
        }
    }

    private void UpdateSystemImagePreview()
    {
        var systemName = SystemNameTextBox.Text.Trim();
        var imagesSystemsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "systems");
        string imagePath = null;

        if (!string.IsNullOrEmpty(systemName))
        {
            foreach (var ext in new[] { ".png", ".jpg", ".jpeg" })
            {
                var path = Path.Combine(imagesSystemsDir, $"{systemName}{ext}");
                if (File.Exists(path))
                {
                    imagePath = path;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(imagePath))
        {
            imagePath = Path.Combine(imagesSystemsDir, "default.png");
        }

        if (!File.Exists(imagePath))
        {
            SystemImagePreview.Source = null;
            return;
        }

        try
        {
            var imageBytes = _imageLoader.LoadImageBytes(imagePath);
            SystemImagePreview.Source = imageBytes?.ToBitmapImage();
        }
        catch
        {
            SystemImagePreview.Source = null;
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private async void SuggestEmulator1Parameters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SuggestParametersAsync(
                Emulator1NameTextBox.Text,
                Emulator1PathTextBox.Text,
                Emulator1ParametersTextBox.Text,
                SuggestEmulator1ParametersButton);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SuggestEmulator1Parameters_Click");
        }
    }

    private async void SuggestEmulator2Parameters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SuggestParametersAsync(
                Emulator2NameTextBox.Text,
                Emulator2PathTextBox.Text,
                Emulator2ParametersTextBox.Text,
                SuggestEmulator2ParametersButton);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SuggestEmulator2Parameters_Click");
        }
    }

    private async void SuggestEmulator3Parameters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SuggestParametersAsync(
                Emulator3NameTextBox.Text,
                Emulator3PathTextBox.Text,
                Emulator3ParametersTextBox.Text,
                SuggestEmulator3ParametersButton);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SuggestEmulator3Parameters_Click");
        }
    }

    private async void SuggestEmulator4Parameters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SuggestParametersAsync(
                Emulator4NameTextBox.Text,
                Emulator4PathTextBox.Text,
                Emulator4ParametersTextBox.Text,
                SuggestEmulator4ParametersButton);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SuggestEmulator4Parameters_Click");
        }
    }

    private async void SuggestEmulator5Parameters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SuggestParametersAsync(
                Emulator5NameTextBox.Text,
                Emulator5PathTextBox.Text,
                Emulator5ParametersTextBox.Text,
                SuggestEmulator5ParametersButton);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in method SuggestEmulator5Parameters_Click");
        }
    }

    private async Task SuggestParametersAsync(string emulatorName, string emulatorPath, string currentParameters, Button suggestButton)
    {
        var successTitle = (string)Application.Current.TryFindResource("ParameterResolverSuccess") ?? "Parameter Suggestion";
        var errorTitle = (string)Application.Current.TryFindResource("ParameterResolverError") ?? "Error";
        var errorMessage = (string)Application.Current.TryFindResource("ErrorProcessingRequest") ?? "There was an error processing your request.";
        var confirmMessage = (string)Application.Current.TryFindResource("ParameterResolverConfirmApply") ?? "Do you want to apply this parameter?";

        if (string.IsNullOrWhiteSpace(emulatorName))
        {
            var enterEmulatorNameMsg = (string)Application.Current.TryFindResource("ParameterResolverEnterEmulatorName") ?? "Please enter an emulator name first.";
            await _messageBox.WarningMessageBox(enterEmulatorNameMsg);
            return;
        }

        suggestButton.IsEnabled = false;
        Mouse.OverrideCursor = Cursors.Wait;

        var loadingMessage = (string)Application.Current.TryFindResource("ParameterResolverLoading") ?? "Resolving parameters, please wait...";
        LoadingOverlay.Content = loadingMessage;
        LoadingOverlay.Visibility = Visibility.Visible;

        try
        {
            var request = new
            {
                SystemName = SystemNameTextBox.Text.Trim(),
                SystemFolder = SystemFolderTextBox.Text.Trim(),
                FileFormatsToSearch = SplitAndTrim(FormatToSearchTextBox.Text),
                ExtractFileBeforeLaunch = ExtractFileBeforeLaunchComboBox.SelectedItem?.ToString() == "true",
                FileFormatsToLaunch = SplitAndTrim(FormatToLaunchTextBox.Text),
                GroupByFolder = GroupByFolderComboBox.SelectedItem?.ToString() == "true",
                DisableRecursiveSearch = DisableRecursiveSearchComboBox.SelectedItem?.ToString() == "true",
                EmulatorName = emulatorName.Trim(),
                EmulatorPath = emulatorPath?.Trim(),
                CurrentParameters = currentParameters?.Trim()
            };

            var apiKey = _configuration["ApiKey"];

            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("ParameterResolverClient");

            var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ParameterResolver/resolve");
            httpRequest.Headers.Add("X-Api-Key", apiKey);
            httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(httpRequest);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ParameterResolverResult>(responseBody, JsonOptions);
                var suggestedParam = result?.SuggestedParameter ?? "";
                var explanation = result?.Explanation ?? "";

                var dialogMessage = $"{confirmMessage}\n\n{suggestedParam}";
                if (!string.IsNullOrEmpty(explanation))
                {
                    dialogMessage += $"\n\nExplanation: {explanation}";
                }

                var applyResult = await _messageBox.CustomQuestionMessageBox(successTitle, dialogMessage);

                if (applyResult)
                {
                    var textBox = FindParametersTextBox(emulatorName);
                    textBox?.Text = suggestedParam;
                }
            }
            else
            {
                var apiException = new InvalidOperationException($"ParameterResolver API returned {(int)response.StatusCode}: {responseBody}");
                _logErrors.LogAndForget(apiException, "ParameterResolver API error");
                await _messageBox.CustomErrorMessageBox(errorMessage, errorTitle);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error calling ParameterResolver API");
            await _messageBox.CustomErrorMessageBox(errorMessage, errorTitle);
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
            suggestButton.IsEnabled = true;
            Mouse.OverrideCursor = null;
        }
    }

    private TextBox FindParametersTextBox(string emulatorName)
    {
        if (emulatorName == Emulator1NameTextBox.Text) return Emulator1ParametersTextBox;
        if (emulatorName == Emulator2NameTextBox.Text) return Emulator2ParametersTextBox;
        if (emulatorName == Emulator3NameTextBox.Text) return Emulator3ParametersTextBox;
        if (emulatorName == Emulator4NameTextBox.Text) return Emulator4ParametersTextBox;
        if (emulatorName == Emulator5NameTextBox.Text) return Emulator5ParametersTextBox;

        return null;
    }

    private static List<string> SplitAndTrim(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return text.Split([',', '|', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}