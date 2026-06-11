#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

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
    [ObservableProperty] private string _systemName = "";

    [ObservableProperty] private string _systemFolder = "";

    [ObservableProperty] private string _systemImageFolder = "";

    [ObservableProperty] private ObservableCollection<string> _additionalFolders = [];

    [ObservableProperty] private string _formatsToSearch = "";

    [ObservableProperty] private string _formatsToLaunch = "";

    [ObservableProperty] private bool _extractFileBeforeLaunch;

    [ObservableProperty] private bool _groupByFolder;

    [ObservableProperty] private bool _disableRecursiveSearch;

    // Emulator fields (5 emulators)
    [ObservableProperty] private string _emulator1Name = "";

    [ObservableProperty] private string _emulator1Path = "";

    [ObservableProperty] private string _emulator1Parameters = "";

    [ObservableProperty] private string _emulator2Name = "";

    [ObservableProperty] private string _emulator2Path = "";

    [ObservableProperty] private string _emulator2Parameters = "";

    [ObservableProperty] private string _emulator3Name = "";

    [ObservableProperty] private string _emulator3Path = "";

    [ObservableProperty] private string _emulator3Parameters = "";

    [ObservableProperty] private string _emulator4Name = "";

    [ObservableProperty] private string _emulator4Path = "";

    [ObservableProperty] private string _emulator4Parameters = "";

    [ObservableProperty] private string _emulator5Name = "";

    [ObservableProperty] private string _emulator5Path = "";

    [ObservableProperty] private string _emulator5Parameters = "";

    // Button states
    [ObservableProperty] private bool _isSaveEnabled;

    [ObservableProperty] private bool _isDeleteEnabled;

    [ObservableProperty] private bool _isAddEnabled = true;

    // Loading state
    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "";

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
        SystemFolder = system.PrimarySystemFolder ?? "";
        SystemImageFolder = system.SystemImageFolder ?? "";
        AdditionalFolders = new ObservableCollection<string>(system.SystemFolders?.Skip(1) ?? []);
        FormatsToSearch = string.Join(",", system.FileFormatsToSearch ?? []);
        FormatsToLaunch = string.Join(",", system.FileFormatsToLaunch ?? []);
        ExtractFileBeforeLaunch = system.ExtractFileBeforeLaunch;
        GroupByFolder = system.GroupByFolder;
        DisableRecursiveSearch = system.DisableRecursiveSearch;

        // Load emulators
        var emulators = system.Emulators?.Take(5).ToList() ?? [];
        Emulator1Name = emulators.Count > 0 ? emulators[0].EmulatorName : "";
        Emulator1Path = emulators.Count > 0 ? emulators[0].EmulatorLocation : "";
        Emulator1Parameters = emulators.Count > 0 ? emulators[0].EmulatorParameters : "";
        Emulator2Name = emulators.Count > 1 ? emulators[1].EmulatorName : "";
        Emulator2Path = emulators.Count > 1 ? emulators[1].EmulatorLocation : "";
        Emulator2Parameters = emulators.Count > 1 ? emulators[1].EmulatorParameters : "";
        Emulator3Name = emulators.Count > 2 ? emulators[2].EmulatorName : "";
        Emulator3Path = emulators.Count > 2 ? emulators[2].EmulatorLocation : "";
        Emulator3Parameters = emulators.Count > 2 ? emulators[2].EmulatorParameters : "";
        Emulator4Name = emulators.Count > 3 ? emulators[3].EmulatorName : "";
        Emulator4Path = emulators.Count > 3 ? emulators[3].EmulatorLocation : "";
        Emulator4Parameters = emulators.Count > 3 ? emulators[3].EmulatorParameters : "";
        Emulator5Name = emulators.Count > 4 ? emulators[4].EmulatorName : "";
        Emulator5Path = emulators.Count > 4 ? emulators[4].EmulatorLocation : "";
        Emulator5Parameters = emulators.Count > 4 ? emulators[4].EmulatorParameters : "";

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
        SystemName = "";
        SystemFolder = "";
        SystemImageFolder = "";
        AdditionalFolders = [];
        FormatsToSearch = "";
        FormatsToLaunch = "";
        ExtractFileBeforeLaunch = false;
        GroupByFolder = false;
        DisableRecursiveSearch = false;
        Emulator1Name = "";
        Emulator1Path = "";
        Emulator1Parameters = "";
        Emulator2Name = "";
        Emulator2Path = "";
        Emulator2Parameters = "";
        Emulator3Name = "";
        Emulator3Path = "";
        Emulator3Parameters = "";
        Emulator4Name = "";
        Emulator4Path = "";
        Emulator4Parameters = "";
        Emulator5Name = "";
        Emulator5Path = "";
        Emulator5Parameters = "";
    }
}