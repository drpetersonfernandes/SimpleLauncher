using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EditSystemViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;
    private readonly IMessageDialogService _messageDialog;
    private readonly IFilePickerService _filePicker;
    private readonly SettingsManager _settings;

    public EditSystemViewModel(
        IConfiguration configuration,
        IMessageDialogService messageDialog,
        IFilePickerService filePicker,
        SettingsManager settings)
    {
        _configuration = configuration;
        _messageDialog = messageDialog;
        _filePicker = filePicker;
        _settings = settings;
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
        // TODO: Show emulator selection dialog
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

        // TODO: Save system configuration
        await _messageDialog.ShowInfoAsync($"System '{SystemName}' saved successfully!", "Save System");
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
            // TODO: Load system from SystemManager
            SystemName = systemName;
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
