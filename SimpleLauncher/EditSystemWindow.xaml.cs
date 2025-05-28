using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using Application = System.Windows.Application;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace SimpleLauncher;

public partial class EditSystemWindow
{
    private XDocument _xmlDoc;
    private const string XmlFilePath = "system.xml";
    private static readonly char[] SplitSeparators = [',', '|', ';'];
    private readonly SettingsManager _settings;
    private string _originalSystemName;

    public EditSystemWindow(SettingsManager settings)
    {
        InitializeComponent();

        _settings = settings;

        _ = LoadXml();

        App.ApplyThemeToWindow(this);
        Closing += EditSystem_Closing;

        SaveSystemButton.IsEnabled = false;
        DeleteSystemButton.IsEnabled = false;
    }

    private async Task LoadXml()
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
                return XDocument.Load(reader);
            });

            if (xmlDoc == null)
            {
                // Notify user on UI thread
                Dispatcher.Invoke(static () =>
                {
                    MessageBoxLibrary.SystemXmlNotFoundMessageBox();
                    QuitApplication.SimpleQuitApplication();
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
            _ = LogErrors.LogErrorAsync(ex, "Error loading XML file");
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
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
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
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
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
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
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
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
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
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }

    private void AddSystemButton_Click(object sender, RoutedEventArgs e)
    {
        _originalSystemName = null;

        EnableFields();
        ClearFields();
        HelpUserTextBlock.Text = string.Empty;

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

        SystemImageFolderTextBox.IsReadOnly = false;
        SystemImageFolderTextBox.IsEnabled = true;

        SystemIsMameComboBox.IsReadOnly = false;
        SystemIsMameComboBox.IsEnabled = true;

        FormatToSearchTextBox.IsReadOnly = false;
        FormatToSearchTextBox.IsEnabled = true;

        ExtractFileBeforeLaunchComboBox.IsReadOnly = false;
        ExtractFileBeforeLaunchComboBox.IsEnabled = true;

        FormatToLaunchTextBox.IsReadOnly = false;
        FormatToLaunchTextBox.IsEnabled = true;

        Emulator1NameTextBox.IsReadOnly = false;
        Emulator1NameTextBox.IsEnabled = true;

        Emulator1PathTextBox.IsReadOnly = false;
        Emulator1PathTextBox.IsEnabled = true;

        Emulator1ParametersTextBox.IsReadOnly = false;
        Emulator1ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError1.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError1.IsEnabled = true;

        Emulator2NameTextBox.IsReadOnly = false;
        Emulator2NameTextBox.IsEnabled = true;

        Emulator2PathTextBox.IsReadOnly = false;
        Emulator2PathTextBox.IsEnabled = true;

        Emulator2ParametersTextBox.IsReadOnly = false;
        Emulator2ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError2.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError2.IsEnabled = true;

        Emulator3NameTextBox.IsReadOnly = false;
        Emulator3NameTextBox.IsEnabled = true;

        Emulator3PathTextBox.IsReadOnly = false;
        Emulator3PathTextBox.IsEnabled = true;

        Emulator3ParametersTextBox.IsReadOnly = false;
        Emulator3ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError3.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError3.IsEnabled = true;

        Emulator4NameTextBox.IsReadOnly = false;
        Emulator4NameTextBox.IsEnabled = true;

        Emulator4PathTextBox.IsReadOnly = false;
        Emulator4PathTextBox.IsEnabled = true;

        Emulator4ParametersTextBox.IsReadOnly = false;
        Emulator4ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError4.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError4.IsEnabled = true;

        Emulator5NameTextBox.IsReadOnly = false;
        Emulator5NameTextBox.IsEnabled = true;

        Emulator5PathTextBox.IsReadOnly = false;
        Emulator5PathTextBox.IsEnabled = true;

        Emulator5ParametersTextBox.IsReadOnly = false;
        Emulator5ParametersTextBox.IsEnabled = true;

        ReceiveANotificationOnEmulatorError5.IsReadOnly = false;
        ReceiveANotificationOnEmulatorError5.IsEnabled = true;

        ChooseSystemFolderButton.IsEnabled = true;
        ChooseSystemImageFolderButton.IsEnabled = true;
        ChooseEmulator1PathButton.IsEnabled = true;
        ChooseEmulator2PathButton.IsEnabled = true;
        ChooseEmulator3PathButton.IsEnabled = true;
        ChooseEmulator4PathButton.IsEnabled = true;
        ChooseEmulator5PathButton.IsEnabled = true;
    }

    private void ClearFields()
    {
        SystemNameDropdown.SelectedItem = null;
        SystemNameTextBox.Text = string.Empty;
        SystemFolderTextBox.Text = string.Empty;
        SystemImageFolderTextBox.Text = string.Empty;
        SystemIsMameComboBox.SelectedItem = null;
        FormatToSearchTextBox.Text = string.Empty;
        ExtractFileBeforeLaunchComboBox.SelectedItem = null;
        FormatToLaunchTextBox.Text = string.Empty;
        Emulator1NameTextBox.Text = string.Empty;
        Emulator1PathTextBox.Text = string.Empty;
        Emulator1ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError1.SelectedItem = null;
        Emulator2NameTextBox.Text = string.Empty;
        Emulator2PathTextBox.Text = string.Empty;
        Emulator2ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError2.SelectedItem = null;
        Emulator3NameTextBox.Text = string.Empty;
        Emulator3PathTextBox.Text = string.Empty;
        Emulator3ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError3.SelectedItem = null;
        Emulator4NameTextBox.Text = string.Empty;
        Emulator4PathTextBox.Text = string.Empty;
        Emulator4ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError4.SelectedItem = null;
        Emulator5NameTextBox.Text = string.Empty;
        Emulator5PathTextBox.Text = string.Empty;
        Emulator5ParametersTextBox.Text = string.Empty;
        ReceiveANotificationOnEmulatorError5.SelectedItem = null;
    }

    private void DeleteSystemButton_Click(object sender, RoutedEventArgs e)
    {
        HelpUserTextBlock.Text = string.Empty;

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
                if (result != MessageBoxResult.Yes) return;

                systemNode.Remove();
                _xmlDoc.Save(XmlFilePath);
                PopulateSystemNamesDropdown();
                ClearFields();

                MessageBoxLibrary.SystemHasBeenDeletedMessageBox(selectedSystemName);
            }
        }
        else
        {
            // Notify user
            MessageBoxLibrary.SystemNotFoundInTheXmlMessageBox();
        }
    }

    private static void EditSystem_Closing(object sender, CancelEventArgs e)
    {
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
            _ = LogErrors.LogErrorAsync(ex, "Error in method EditSystem_Closing");
        }
    }

    private void HelpLink_Click(object sender, RoutedEventArgs e)
    {
        const string searchUrl = "https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters";
        Process.Start(new ProcessStartInfo
        {
            FileName = searchUrl,
            UseShellExecute = true
        });

        PlayClick.PlayNotificationSound();
    }

    private void SystemNameTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Update HelpUserTextBlock
        HelpUserTextBlock.Text = string.Empty;
        UiHelpers.HelpUser.UpdateHelpUserTextBlock(HelpUserTextBlock, SystemNameTextBox);
    }
}