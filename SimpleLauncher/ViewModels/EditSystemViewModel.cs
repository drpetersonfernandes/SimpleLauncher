#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Core.Interfaces.MessageBoxResult;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class EditSystemViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly IConfiguration _configuration;

    private List<SystemManager>? _systems;
    private string? _originalSystemName;

    // System selection
    [ObservableProperty] private ObservableCollection<string> _systemNames = [];

    [ObservableProperty] private int _selectedSystemIndex = -1;

    // System fields
    [ObservableProperty] private string _systemName = string.Empty;

    [ObservableProperty] private string _systemFolder = string.Empty;

    [ObservableProperty] private string _systemImageFolder = string.Empty;

    [ObservableProperty] private ObservableCollection<string> _additionalFolders = [];

    [ObservableProperty] private string _formatsToSearch = string.Empty;

    [ObservableProperty] private string _formatsToLaunch = string.Empty;

    [ObservableProperty] private bool _extractFileBeforeLaunch;

    [ObservableProperty] private bool _groupByFolder;

    [ObservableProperty] private bool _disableRecursiveSearch;

    // Emulator fields (5 emulators)
    [ObservableProperty] private string _emulator1Name = string.Empty;

    [ObservableProperty] private string _emulator1Path = string.Empty;

    [ObservableProperty] private string _emulator1Parameters = string.Empty;

    [ObservableProperty] private string _emulator2Name = string.Empty;

    [ObservableProperty] private string _emulator2Path = string.Empty;

    [ObservableProperty] private string _emulator2Parameters = string.Empty;

    [ObservableProperty] private string _emulator3Name = string.Empty;

    [ObservableProperty] private string _emulator3Path = string.Empty;

    [ObservableProperty] private string _emulator3Parameters = string.Empty;

    [ObservableProperty] private string _emulator4Name = string.Empty;

    [ObservableProperty] private string _emulator4Path = string.Empty;

    [ObservableProperty] private string _emulator4Parameters = string.Empty;

    [ObservableProperty] private string _emulator5Name = string.Empty;

    [ObservableProperty] private string _emulator5Path = string.Empty;

    [ObservableProperty] private string _emulator5Parameters = string.Empty;

    // Button states
    [ObservableProperty] private bool _isSaveEnabled;

    [ObservableProperty] private bool _isDeleteEnabled;

    [ObservableProperty] private bool _isAddEnabled = true;

    // Loading state
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    public EditSystemViewModel(
        SettingsManager settings,
        PlaySoundEffects playSoundEffects,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        IConfiguration configuration)
    {
        _settings = settings;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _configuration = configuration;
    }

    public async Task LoadSystemsAsync()
    {
        IsLoading = true;
        LoadingMessage = _resourceProvider.GetString("Loadingsystems", "Loading systems...");

        try
        {
            _systems = await Task.Run(() => SystemManager.LoadSystemManagers(_configuration));

            if (_systems == null)
            {
                await _messageBox.SystemXmlNotFoundMessageBox();
                return;
            }

            PopulateSystemNamesDropdown();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading systems into Edit window.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void PopulateSystemNamesDropdown()
    {
        if (_systems == null) return;

        var names = _systems
            .Select(static s => s.SystemName)
            .OrderBy(static n => n)
            .ToList();

        SystemNames = new ObservableCollection<string>(names);
    }

    public void OnSystemSelectionChanged(string? selectedSystemName)
    {
        if (string.IsNullOrEmpty(selectedSystemName) || _systems == null)
        {
            ClearFields();
            return;
        }

        var system = _systems.FirstOrDefault(s =>
            s.SystemName.Equals(selectedSystemName, StringComparison.OrdinalIgnoreCase));

        if (system == null) return;

        _originalSystemName = system.SystemName;
        SystemName = system.SystemName;
        SystemFolder = system.PrimarySystemFolder ?? string.Empty;
        SystemImageFolder = system.SystemImageFolder ?? string.Empty;
        AdditionalFolders = new ObservableCollection<string>(system.SystemFolders?.Skip(1) ?? []);
        FormatsToSearch = string.Join(",", system.FileFormatsToSearch ?? []);
        FormatsToLaunch = string.Join(",", system.FileFormatsToLaunch ?? []);
        ExtractFileBeforeLaunch = system.ExtractFileBeforeLaunch;
        GroupByFolder = system.GroupByFolder;
        DisableRecursiveSearch = system.DisableRecursiveSearch;

        // Load emulators
        var emulators = system.Emulators?.Take(5).ToList() ?? [];
        Emulator1Name = emulators.Count > 0 ? emulators[0].EmulatorName : string.Empty;
        Emulator1Path = emulators.Count > 0 ? emulators[0].EmulatorLocation : string.Empty;
        Emulator1Parameters = emulators.Count > 0 ? emulators[0].EmulatorParameters : string.Empty;
        Emulator2Name = emulators.Count > 1 ? emulators[1].EmulatorName : string.Empty;
        Emulator2Path = emulators.Count > 1 ? emulators[1].EmulatorLocation : string.Empty;
        Emulator2Parameters = emulators.Count > 1 ? emulators[1].EmulatorParameters : string.Empty;
        Emulator3Name = emulators.Count > 2 ? emulators[2].EmulatorName : string.Empty;
        Emulator3Path = emulators.Count > 2 ? emulators[2].EmulatorLocation : string.Empty;
        Emulator3Parameters = emulators.Count > 2 ? emulators[2].EmulatorParameters : string.Empty;
        Emulator4Name = emulators.Count > 3 ? emulators[3].EmulatorName : string.Empty;
        Emulator4Path = emulators.Count > 3 ? emulators[3].EmulatorLocation : string.Empty;
        Emulator4Parameters = emulators.Count > 3 ? emulators[3].EmulatorParameters : string.Empty;
        Emulator5Name = emulators.Count > 4 ? emulators[4].EmulatorName : string.Empty;
        Emulator5Path = emulators.Count > 4 ? emulators[4].EmulatorLocation : string.Empty;
        Emulator5Parameters = emulators.Count > 4 ? emulators[4].EmulatorParameters : string.Empty;

        IsSaveEnabled = true;
        IsDeleteEnabled = true;
    }

    [RelayCommand]
    private Task AddSystemAsync()
    {
        _playSoundEffects.PlayNotificationSound();
        _originalSystemName = null;

        ClearFields();
        SelectedSystemIndex = -1;

        IsSaveEnabled = true;
        IsDeleteEnabled = false;

        return _messageBox.YouCanAddANewSystemMessageBox();
    }

    [RelayCommand]
    private async Task SaveSystemAsync()
    {
        _playSoundEffects.PlayNotificationSound();

        // Validation
        if (string.IsNullOrWhiteSpace(SystemName))
        {
            await _messageBox.SystemNameCanNotBeEmptyMessageBox();
            return;
        }

        if (string.IsNullOrWhiteSpace(SystemFolder))
        {
            await _messageBox.SystemFolderCanNotBeEmptyMessageBox();
        }

        // Save logic would go here
        // This would need to call SystemManager.SaveSystemManagers or similar
    }

    [RelayCommand]
    private async Task DeleteSystemAsync()
    {
        _playSoundEffects.PlayNotificationSound();

        if (string.IsNullOrEmpty(_originalSystemName))
        {
            await _messageBox.SelectASystemToDeleteMessageBox();
            return;
        }

        var result = await _messageBox.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
        // ReSharper disable once RedundantJumpStatement
        if (result != CoreMessageBoxResult.Yes) return;

        // Delete logic would go here
        // TODO
    }

    public void AddAdditionalFolder(string folder)
    {
        if (!string.IsNullOrWhiteSpace(folder) && !AdditionalFolders.Contains(folder))
        {
            AdditionalFolders.Add(folder);
        }
    }

    public void RemoveAdditionalFolder(string folder)
    {
        AdditionalFolders.Remove(folder);
    }

    private void ClearFields()
    {
        SystemName = string.Empty;
        SystemFolder = string.Empty;
        SystemImageFolder = string.Empty;
        AdditionalFolders = [];
        FormatsToSearch = string.Empty;
        FormatsToLaunch = string.Empty;
        ExtractFileBeforeLaunch = false;
        GroupByFolder = false;
        DisableRecursiveSearch = false;
        Emulator1Name = string.Empty;
        Emulator1Path = string.Empty;
        Emulator1Parameters = string.Empty;
        Emulator2Name = string.Empty;
        Emulator2Path = string.Empty;
        Emulator2Parameters = string.Empty;
        Emulator3Name = string.Empty;
        Emulator3Path = string.Empty;
        Emulator3Parameters = string.Empty;
        Emulator4Name = string.Empty;
        Emulator4Path = string.Empty;
        Emulator4Parameters = string.Empty;
        Emulator5Name = string.Empty;
        Emulator5Path = string.Empty;
        Emulator5Parameters = string.Empty;
    }
}