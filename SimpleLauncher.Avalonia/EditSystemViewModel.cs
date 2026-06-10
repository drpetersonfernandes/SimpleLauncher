using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EditSystemViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;
    private readonly IMessageDialogService _messageDialog;
    private readonly IFilePickerService _filePicker;
    private readonly SettingsManager _settings;
    private readonly ICoreSystemConfigurationService _systemConfigReader;
    private readonly ISystemConfigurationWriterService _systemConfigWriter;

    public EditSystemViewModel(
        IConfiguration configuration,
        IMessageDialogService messageDialog,
        IFilePickerService filePicker,
        SettingsManager settings,
        ICoreSystemConfigurationService systemConfigReader,
        ISystemConfigurationWriterService systemConfigWriter)
    {
        _configuration = configuration;
        _messageDialog = messageDialog;
        _filePicker = filePicker;
        _settings = settings;
        _systemConfigReader = systemConfigReader;
        _systemConfigWriter = systemConfigWriter;
    }

    // ── System Info ─────────────────────────────────────────────

    [ObservableProperty] private string _systemName = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _systemFolders = [];

    [ObservableProperty] private string _systemImageFolder = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _fileFormatsToSearch = [];

    [ObservableProperty] private bool _extractFileBeforeLaunch;

    [ObservableProperty] private ObservableCollection<string> _fileFormatsToLaunch = [];

    [ObservableProperty] private bool _groupByFolder;

    [ObservableProperty] private bool _disableRecursiveSearch;

    // ── Emulators ───────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<EmulatorItem> _emulators = [];

    [ObservableProperty] private EmulatorItem? _selectedEmulator;

    // ── UI State ────────────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    [ObservableProperty] private bool _isNewSystem = true;

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task AddSystemFolderAsync()
    {
        var folder = await _filePicker.OpenFolderAsync("Select System Folder");
        if (!string.IsNullOrEmpty(folder))
        {
            SystemFolders.Add(folder);
        }
    }

    [RelayCommand]
    private void RemoveSystemFolder(string? folder)
    {
        if (!string.IsNullOrEmpty(folder))
        {
            SystemFolders.Remove(folder);
        }
    }

    [RelayCommand]
    private async Task ChooseImageFolderAsync()
    {
        var folder = await _filePicker.OpenFolderAsync("Select Image Folder");
        if (!string.IsNullOrEmpty(folder))
        {
            SystemImageFolder = folder;
        }
    }

    [RelayCommand]
    private async Task AddEmulatorAsync()
    {
        var emulator = new EmulatorItem
        {
            Name = "Emulator " + (Emulators.Count + 1),
            Location = string.Empty,
            Parameters = string.Empty
        };
        Emulators.Add(emulator);
        SelectedEmulator = emulator;
    }

    [RelayCommand]
    private async Task BrowseEmulatorLocationAsync()
    {
        if (SelectedEmulator == null) return;

        var path = await _filePicker.OpenFileAsync("Select Emulator Executable", "Executables|*.exe|All files|*.*");
        if (!string.IsNullOrEmpty(path))
        {
            SelectedEmulator.Location = path;
        }
    }

    [RelayCommand]
    private void RemoveEmulator()
    {
        if (SelectedEmulator != null)
        {
            Emulators.Remove(SelectedEmulator);
            SelectedEmulator = null;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(SystemName))
        {
            await _messageDialog.ShowInfoAsync("System name is required.", "Save System");
            return;
        }

        if (SystemFolders.Count == 0)
        {
            await _messageDialog.ShowInfoAsync("At least one system folder is required.", "Save System");
            return;
        }

        if (string.IsNullOrEmpty(SystemImageFolder))
        {
            await _messageDialog.ShowInfoAsync("Image folder is required.", "Save System");
            return;
        }

        if (FileFormatsToSearch.Count == 0)
        {
            await _messageDialog.ShowInfoAsync("At least one file format to search is required.", "Save System");
            return;
        }

        if (Emulators.Count == 0)
        {
            await _messageDialog.ShowInfoAsync("At least one emulator is required.", "Save System");
            return;
        }

        try
        {
            var systemConfig = new SystemManagerConfig
            {
                SystemName = SystemName,
                SystemFolders = SystemFolders.ToList(),
                SystemImageFolder = SystemImageFolder,
                FileFormatsToSearch = FileFormatsToSearch.ToList(),
                ExtractFileBeforeLaunch = ExtractFileBeforeLaunch,
                FileFormatsToLaunch = FileFormatsToLaunch.ToList(),
                GroupByFolder = GroupByFolder,
                DisableRecursiveSearch = DisableRecursiveSearch,
                Emulators = Emulators.Select(static e => new Emulator
                {
                    EmulatorName = e.Name,
                    EmulatorLocation = e.Location,
                    EmulatorParameters = e.Parameters,
                    ReceiveANotificationOnEmulatorError = true
                }).ToList()
            };

            await _systemConfigWriter.SaveSystemAsync(systemConfig, IsNewSystem ? null : SystemName);
            await _messageDialog.ShowInfoAsync($"System '{SystemName}' saved successfully!", "Save System");
        }
        catch (Exception ex)
        {
            await _messageDialog.ShowErrorAsync($"Failed to save system: {ex.Message}", "Save Error");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // Close without saving
    }

    // ── Public Methods ──────────────────────────────────────────

    public Task LoadSystemAsync(string systemName)
    {
        IsLoading = true;
        LoadingMessage = "Loading system configuration...";
        IsNewSystem = false;

        try
        {
            var systemManagers = _systemConfigReader.LoadSystemManagers();
            var system = systemManagers.FirstOrDefault(s =>
                s.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

            if (system == null)
            {
                SystemName = systemName;
                return Task.CompletedTask;
            }

            SystemName = system.SystemName;
            SystemFolders = new ObservableCollection<string>(system.SystemFolders);
            SystemImageFolder = system.SystemImageFolder;
            FileFormatsToSearch = new ObservableCollection<string>(system.FileFormatsToSearch);
            ExtractFileBeforeLaunch = system.ExtractFileBeforeLaunch;
            FileFormatsToLaunch = new ObservableCollection<string>(system.FileFormatsToLaunch);
            GroupByFolder = system.GroupByFolder;
            DisableRecursiveSearch = system.DisableRecursiveSearch;

            Emulators = new ObservableCollection<EmulatorItem>(system.Emulators.Select(static e => new EmulatorItem
            {
                Name = e.EmulatorName,
                Location = e.EmulatorLocation,
                Parameters = e.EmulatorParameters
            }));

            if (Emulators.Count > 0)
            {
                SelectedEmulator = Emulators[0];
            }
        }
        finally
        {
            IsLoading = false;
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Represents an emulator configuration item.
/// </summary>
public partial class EmulatorItem : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;

    [ObservableProperty] private string _location = string.Empty;

    [ObservableProperty] private string _parameters = string.Empty;
}
