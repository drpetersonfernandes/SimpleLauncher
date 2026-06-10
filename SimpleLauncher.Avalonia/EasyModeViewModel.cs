using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EasyModeViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;
    private readonly IMessageDialogService _messageDialog;
    private readonly IFilePickerService _filePicker;
    private readonly SettingsManager _settings;
    private readonly ISystemConfigurationWriterService _systemConfigWriter;
    private List<EasyModeSystemConfig> _easyModeSystems = [];

    public EasyModeViewModel(
        IConfiguration configuration,
        IMessageDialogService messageDialog,
        IFilePickerService filePicker,
        SettingsManager settings,
        ISystemConfigurationWriterService systemConfigWriter)
    {
        _configuration = configuration;
        _messageDialog = messageDialog;
        _filePicker = filePicker;
        _settings = settings;
        _systemConfigWriter = systemConfigWriter;
    }

    // ── System Selection ────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<string> _systemNames = [];

    [ObservableProperty] private int _selectedSystemIndex = -1;

    [ObservableProperty] private string _selectedSystemName = string.Empty;

    partial void OnSelectedSystemIndexChanged(int value)
    {
        if (value >= 0 && value < _easyModeSystems.Count)
        {
            SelectedSystemName = _easyModeSystems[value].SystemName;
            SystemImageFolder = _easyModeSystems[value].SystemImageFolder;
        }
        else
        {
            SelectedSystemName = string.Empty;
            SystemImageFolder = string.Empty;
        }

        ResetDownloadStates();
        UpdateAddSystemEnabled();
    }

    // ── Paths ───────────────────────────────────────────────────

    [ObservableProperty] private string _systemFolder = string.Empty;

    partial void OnSystemFolderChanged(string value)
    {
        UpdateAddSystemEnabled();
    }

    [ObservableProperty] private string _systemImageFolder = string.Empty;

    // ── Download States ─────────────────────────────────────────

    [ObservableProperty] private bool _isEmulatorDownloaded;

    [ObservableProperty] private bool _isCoreDownloaded;

    [ObservableProperty] private bool _isImagePack1Downloaded;

    [ObservableProperty] private bool _isImagePack2Downloaded;

    [ObservableProperty] private bool _isImagePack3Downloaded;

    [ObservableProperty] private bool _isImagePack4Downloaded;

    [ObservableProperty] private bool _isImagePack5Downloaded;

    // ── UI State ────────────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    [ObservableProperty] private bool _isOperationInProgress;

    [ObservableProperty] private string _downloadStatus = string.Empty;

    [ObservableProperty] private bool _isAddSystemEnabled;

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task ChooseFolderAsync()
    {
        var folder = await _filePicker.OpenFolderAsync("Select ROM Folder");
        if (!string.IsNullOrEmpty(folder))
        {
            SystemFolder = folder;
        }
    }

    [RelayCommand]
    private async Task DownloadEmulatorAsync()
    {
        if (SelectedSystemIndex < 0)
        {
            await _messageDialog.ShowInfoAsync("Please select a system first.", "Download Emulator");
            return;
        }

        // TODO: Implement emulator download
        IsOperationInProgress = true;
        DownloadStatus = "Downloading emulator...";

        // Simulate download
        await Task.Delay(1000);

        IsEmulatorDownloaded = true;
        IsOperationInProgress = false;
        DownloadStatus = "Emulator downloaded successfully.";
        UpdateAddSystemEnabled();
    }

    [RelayCommand]
    private async Task DownloadCoreAsync()
    {
        if (SelectedSystemIndex < 0)
        {
            await _messageDialog.ShowInfoAsync("Please select a system first.", "Download Core");
            return;
        }

        // TODO: Implement core download
        IsOperationInProgress = true;
        DownloadStatus = "Downloading core...";

        // Simulate download
        await Task.Delay(1000);

        IsCoreDownloaded = true;
        IsOperationInProgress = false;
        DownloadStatus = "Core downloaded successfully.";
        UpdateAddSystemEnabled();
    }

    [RelayCommand]
    private Task DownloadImagePack1Async()
    {
        return DownloadImagePackAsync(1);
    }

    [RelayCommand]
    private Task DownloadImagePack2Async()
    {
        return DownloadImagePackAsync(2);
    }

    [RelayCommand]
    private Task DownloadImagePack3Async()
    {
        return DownloadImagePackAsync(3);
    }

    [RelayCommand]
    private Task DownloadImagePack4Async()
    {
        return DownloadImagePackAsync(4);
    }

    [RelayCommand]
    private Task DownloadImagePack5Async()
    {
        return DownloadImagePackAsync(5);
    }

    [RelayCommand]
    private async Task AddSystemAsync()
    {
        if (string.IsNullOrEmpty(SelectedSystemName))
        {
            await _messageDialog.ShowInfoAsync("Please select a system.", "Add System");
            return;
        }

        if (string.IsNullOrEmpty(SystemFolder))
        {
            await _messageDialog.ShowInfoAsync("Please choose a ROM folder.", "Add System");
            return;
        }

        try
        {
            var easyModeSystem = _easyModeSystems.FirstOrDefault(s =>
                s.SystemName.Equals(SelectedSystemName, StringComparison.OrdinalIgnoreCase));

            if (easyModeSystem == null)
            {
                await _messageDialog.ShowErrorAsync($"System configuration not found for '{SelectedSystemName}'.", "Error");
                return;
            }

            var systemConfig = new SystemManagerConfig
            {
                SystemName = easyModeSystem.SystemName,
                SystemFolders = [SystemFolder],
                SystemImageFolder = easyModeSystem.SystemImageFolder,
                FileFormatsToSearch = easyModeSystem.FileFormatsToSearch,
                ExtractFileBeforeLaunch = easyModeSystem.ExtractFileBeforeLaunch,
                FileFormatsToLaunch = easyModeSystem.FileFormatsToLaunch,
                GroupByFolder = false,
                DisableRecursiveSearch = false,
                Emulators =
                [
                    new Emulator
                    {
                        EmulatorName = easyModeSystem.Emulators.Emulator.EmulatorName,
                        EmulatorLocation = easyModeSystem.Emulators.Emulator.EmulatorLocation,
                        EmulatorParameters = easyModeSystem.Emulators.Emulator.EmulatorParameters,
                        ReceiveANotificationOnEmulatorError = true,
                        ImagePackDownloadLink = easyModeSystem.Emulators.Emulator.ImagePackDownloadLink,
                        ImagePackDownloadLink2 = easyModeSystem.Emulators.Emulator.ImagePackDownloadLink2,
                        ImagePackDownloadLink3 = easyModeSystem.Emulators.Emulator.ImagePackDownloadLink3,
                        ImagePackDownloadLink4 = easyModeSystem.Emulators.Emulator.ImagePackDownloadLink4,
                        ImagePackDownloadLink5 = easyModeSystem.Emulators.Emulator.ImagePackDownloadLink5,
                        ImagePackDownloadExtractPath = easyModeSystem.Emulators.Emulator.ImagePackDownloadExtractPath
                    }
                ]
            };

            await _systemConfigWriter.SaveSystemAsync(systemConfig);
            await _messageDialog.ShowInfoAsync($"System '{SelectedSystemName}' added successfully!", "Add System");
        }
        catch (Exception ex)
        {
            await _messageDialog.ShowErrorAsync($"Failed to add system: {ex.Message}", "Error");
        }
    }

    // ── Public Methods ──────────────────────────────────────────

    public async Task LoadSystemNamesAsync()
    {
        IsLoading = true;
        LoadingMessage = "Loading available systems...";

        try
        {
            const string xmlFile = "easymode.xml";

            var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);

            if (!File.Exists(xmlFilePath))
            {
                // Try alternative path
                xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Services", "EasyMode", "Samples", xmlFile);
            }

            if (File.Exists(xmlFilePath))
            {
                var serializer = new XmlSerializer(typeof(EasyModeXmlRoot));
                await using var stream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var root = (EasyModeXmlRoot?)serializer.Deserialize(stream);
                _easyModeSystems = root?.Systems ?? [];

                SystemNames = new ObservableCollection<string>(
                    _easyModeSystems.Select(static s => s.SystemName).OrderBy(static n => n));
            }
            else
            {
                // Fallback: show common systems
                SystemNames =
                [
                    "Nintendo Entertainment System",
                    "Super Nintendo",
                    "Sega Genesis",
                    "PlayStation",
                    "Nintendo 64",
                    "Game Boy Advance"
                ];
            }
        }
        catch (Exception ex)
        {
            await _messageDialog.ShowErrorAsync($"Failed to load systems: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }

    // ── Private Methods ─────────────────────────────────────────

    private async Task DownloadImagePackAsync(int packNumber)
    {
        if (SelectedSystemIndex < 0)
        {
            await _messageDialog.ShowInfoAsync("Please select a system first.", "Download Image Pack");
            return;
        }

        // TODO: Implement image pack download
        IsOperationInProgress = true;
        DownloadStatus = $"Downloading image pack {packNumber}...";

        // Simulate download
        await Task.Delay(1000);

        switch (packNumber)
        {
            case 1: IsImagePack1Downloaded = true; break;
            case 2: IsImagePack2Downloaded = true; break;
            case 3: IsImagePack3Downloaded = true; break;
            case 4: IsImagePack4Downloaded = true; break;
            case 5: IsImagePack5Downloaded = true; break;
        }

        IsOperationInProgress = false;
        DownloadStatus = $"Image pack {packNumber} downloaded successfully.";
    }

    private void UpdateAddSystemEnabled()
    {
        IsAddSystemEnabled = !string.IsNullOrEmpty(SelectedSystemName) &&
                             !string.IsNullOrEmpty(SystemFolder) &&
                             !IsOperationInProgress;
    }

    private void ResetDownloadStates()
    {
        IsEmulatorDownloaded = false;
        IsCoreDownloaded = false;
        IsImagePack1Downloaded = false;
        IsImagePack2Downloaded = false;
        IsImagePack3Downloaded = false;
        IsImagePack4Downloaded = false;
        IsImagePack5Downloaded = false;
        DownloadStatus = string.Empty;
    }
}

[XmlRoot("EasyMode")]
public class EasyModeXmlRoot
{
    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; } = [];
}
