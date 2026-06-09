using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EasyModeViewModel : ObservableObject
{
    private readonly IConfiguration _configuration;
    private readonly IMessageDialogService _messageDialog;
    private readonly IFilePickerService _filePicker;
    private readonly SettingsManager _settings;

    public EasyModeViewModel(
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

    // ── System Selection ────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<string> _systemNames = [];

    [ObservableProperty] private int _selectedSystemIndex = -1;

    [ObservableProperty] private string _selectedSystemName = string.Empty;

    // ── Paths ───────────────────────────────────────────────────

    [ObservableProperty] private string _systemFolder = string.Empty;

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

        // TODO: Implement system addition
        // This would call SystemManager.AddOrUpdateSystemFromEasyModeAsync()

        await _messageDialog.ShowInfoAsync($"System '{SelectedSystemName}' added successfully!", "Add System");
    }

    // ── Public Methods ──────────────────────────────────────────

    public async Task LoadSystemNamesAsync()
    {
        IsLoading = true;
        LoadingMessage = "Loading available systems...";

        try
        {
            // TODO: Load system names from EasyModeManager
            // For now, use a placeholder list
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
}
