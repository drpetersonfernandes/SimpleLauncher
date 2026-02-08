using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using SimpleLauncher.Services.CheckApplicationControlPolicy;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.HelpUser;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UpdateStatusBar;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SimpleLauncher;

internal partial class EditSystemWindow : ILoadingState
{
    private XDocument _xmlDoc;
    private const string XmlFilePath = "system.xml";
    private static readonly char[] SplitSeparators = [',', '|', ';'];
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private string _originalSystemName;
    private readonly IConfiguration _configuration;

    public EditSystemWindow(SettingsManager settings, PlaySoundEffects playSoundEffects, IConfiguration configuration)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _configuration = configuration;
        _settings = settings;
        _playSoundEffects = playSoundEffects;

        ApplyExpanderSettings();

        _ = LoadXmlAsync();

        Closing += EditSystem_Closing;

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

    private async Task LoadXmlAsync()
    {
        try
        {
            var xmlDoc = await Task.Run(static () =>
            {
                if (!File.Exists(XmlFilePath))
                {
                    return null;
                }

                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                using var reader = XmlReader.Create(XmlFilePath, settings);
                return XDocument.Load(reader, LoadOptions.None);
            });

            if (xmlDoc == null)
            {
                // Notify user on UI thread
                Dispatcher.Invoke(static () =>
                {
                    MessageBoxLibrary.SystemXmlNotFoundMessageBox();
                    QuitSimpleLauncher.SimpleQuitApplication();
                });
            }
            else
            {
                _xmlDoc = xmlDoc;

                PopulateSystemNamesDropdown();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading XML file");
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
            Filter = "Exe File (.exe)|*.exe",
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
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator2Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator22 = (string)Application.Current.TryFindResource("SelectEmulator2") ?? "Select Emulator 2";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
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
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator3Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator32 = (string)Application.Current.TryFindResource("SelectEmulator3") ?? "Select Emulator 3";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
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
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator4Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator42 = (string)Application.Current.TryFindResource("SelectEmulator4") ?? "Select Emulator 4";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
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
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void ChooseEmulator5Path(object sender, RoutedEventArgs e)
    {
        var selectEmulator52 = (string)Application.Current.TryFindResource("SelectEmulator5") ?? "Select Emulator 5";
        var dialog = new OpenFileDialog
        {
            DefaultExt = ".exe",
            Filter = "EXE File (.exe)|*.exe",
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
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
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
        MessageBoxLibrary.YouCanAddANewSystemMessageBox();
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

        SystemIsMameComboBox.IsEnabled = true;

        FormatToSearchTextBox.IsReadOnly = false;
        FormatToSearchTextBox.IsEnabled = true;

        ExtractFileBeforeLaunchComboBox.IsEnabled = true;

        GroupByFolderComboBox.IsEnabled = true;

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

        SystemIsMameComboBox.IsEnabled = false;

        FormatToSearchTextBox.IsReadOnly = true;
        FormatToSearchTextBox.IsEnabled = false;

        ExtractFileBeforeLaunchComboBox.IsEnabled = false;

        GroupByFolderComboBox.IsEnabled = false;

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
    }

    private void ClearFields()
    {
        SystemNameDropdown.SelectedItem = null;
        ClearFieldsForNoSelection();
    }

    // Clears fields when no system is selected, without affecting SystemNameDropdown itself.
    private void ClearFieldsForNoSelection()
    {
        SystemNameTextBox.Text = string.Empty;
        MarkValid(SystemNameTextBox);

        SystemFolderTextBox.Text = string.Empty;
        MarkValid(SystemFolderTextBox);
        AdditionalFoldersListBox.Items.Clear();

        SystemImageFolderTextBox.Text = string.Empty;
        MarkValid(SystemImageFolderTextBox);

        SystemIsMameComboBox.SelectedItem = null;

        FormatToSearchTextBox.Text = string.Empty;
        MarkValid(FormatToSearchTextBox);

        ExtractFileBeforeLaunchComboBox.SelectedItem = null;

        GroupByFolderComboBox.SelectedItem = null;

        FormatToLaunchTextBox.Text = string.Empty;
        MarkValid(FormatToLaunchTextBox);

        ClearAllEmulatorFieldsInternal();
    }

    private void ClearAllEmulatorFieldsInternal()
    {
        Emulator1NameTextBox.Text = string.Empty;
        MarkValid(Emulator1NameTextBox);
        Emulator1PathTextBox.Text = string.Empty;
        MarkValid(Emulator1PathTextBox);
        Emulator1ParametersTextBox.Text = string.Empty;
        MarkValid(Emulator1ParametersTextBox);
        ReceiveANotificationOnEmulatorError1.SelectedItem = null;

        Emulator2NameTextBox.Text = string.Empty;
        MarkValid(Emulator2NameTextBox);
        Emulator2PathTextBox.Text = string.Empty;
        MarkValid(Emulator2PathTextBox);
        Emulator2ParametersTextBox.Text = string.Empty;
        MarkValid(Emulator2ParametersTextBox);
        ReceiveANotificationOnEmulatorError2.SelectedItem = null;

        Emulator3NameTextBox.Text = string.Empty;
        MarkValid(Emulator3NameTextBox);
        Emulator3PathTextBox.Text = string.Empty;
        MarkValid(Emulator3PathTextBox);
        Emulator3ParametersTextBox.Text = string.Empty;
        MarkValid(Emulator3ParametersTextBox);
        ReceiveANotificationOnEmulatorError3.SelectedItem = null;

        Emulator4NameTextBox.Text = string.Empty;
        MarkValid(Emulator4NameTextBox);
        Emulator4PathTextBox.Text = string.Empty;
        MarkValid(Emulator4PathTextBox);
        Emulator4ParametersTextBox.Text = string.Empty;
        MarkValid(Emulator4ParametersTextBox);
        ReceiveANotificationOnEmulatorError4.SelectedItem = null;

        Emulator5NameTextBox.Text = string.Empty;
        MarkValid(Emulator5NameTextBox);
        Emulator5PathTextBox.Text = string.Empty;
        MarkValid(Emulator5PathTextBox);
        Emulator5ParametersTextBox.Text = string.Empty;
        MarkValid(Emulator5ParametersTextBox);
        ReceiveANotificationOnEmulatorError5.SelectedItem = null;
    }

    private void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
    {
        HelpUserTextBlock.Document.Blocks.Clear();

        if (SystemNameDropdown.SelectedItem == null)
        {
            // Notify user
            MessageBoxLibrary.SelectASystemToDeleteMessageBox();

            return;
        }

        var selectedSystemName = SystemNameDropdown.SelectedItem.ToString();

        if (_xmlDoc == null) return;

        var systemNode = _xmlDoc.Descendants("SystemConfig")
            .FirstOrDefault(element => element.Element("SystemName")?.Value == selectedSystemName);

        if (systemNode != null)
        {
            //Ask user if he really wants to delete the system
            DoYouWanToDeleteSystemMessageBox();

            void DoYouWanToDeleteSystemMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
                if (result == MessageBoxResult.Yes)
                {
                    systemNode.Remove();
                    _xmlDoc.Save(XmlFilePath);

                    _playSoundEffects.PlayNotificationSound();
                }
                else
                {
                    return;
                }

                PopulateSystemNamesDropdown();

                if (SystemNameDropdown.Items.Count == 0 || SystemNameDropdown.SelectedItem == null)
                {
                    ClearFieldsForNoSelection();
                    DisableAllEditableFields();
                    SaveSystemButton.IsEnabled = false;
                    DeleteSystemButton.IsEnabled = false;
                }

                // Notify user
                MessageBoxLibrary.SystemHasBeenDeletedMessageBox(selectedSystemName);
            }
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SystemNotFoundInTheXmlMessageBox();
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
        _settings.Save();

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in method EditSystem_Closing");
        }
    }

    private void HelpLink_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        var searchUrl = App.Configuration["WikiParametersUrl"] ?? "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters";
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
                MessageBoxLibrary.ApplicationControlPolicyBlockedManualLinkMessageBox(searchUrl);
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked opening HelpLink.");
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in method HelpLink_Click");
                MessageBoxLibrary.ErrorOpeningUrlMessageBox();
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in method HelpLink_Click");
            MessageBoxLibrary.ErrorOpeningUrlMessageBox();
        }
    }

    private void SystemNameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Update HelpUserTextBlock
        HelpUserTextBlock.Document.Blocks.Clear();
        HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
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
        UpdateStatusBar.UpdateContent("Emergency reset performed.", Application.Current.MainWindow as MainWindow);
    }
}