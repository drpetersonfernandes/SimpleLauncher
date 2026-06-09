using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly IMessageDialogService _messageDialog;
    private readonly IApplicationLifetime _applicationLifetime;

    public SettingsViewModel(
        SettingsManager settings,
        IMessageDialogService messageDialog,
        IApplicationLifetime applicationLifetime)
    {
        _settings = settings;
        _messageDialog = messageDialog;
        _applicationLifetime = applicationLifetime;

        // Load current settings
        LoadSettings();
    }

    // ── General Settings ────────────────────────────────────────

    [ObservableProperty] private string _selectedLanguage = "en";

    [ObservableProperty] private ObservableCollection<string> _languages = ["en", "es", "de", "fr", "pt", "ja", "ko", "zh"];

    [ObservableProperty] private string _selectedBaseTheme = "Dark";

    [ObservableProperty] private ObservableCollection<string> _baseThemes = ["Light", "Dark", "Adaptive", "HighContrast", "Midnight"];

    [ObservableProperty] private string _selectedAccentColor = "Blue";

    [ObservableProperty] private ObservableCollection<string> _accentColors = ["Amber", "Blue", "Brown", "Cobalt", "Crimson", "Cyan", "Emerald", "Green", "Indigo", "Lime", "Magenta", "Maroon", "Mauve", "Olive", "OliveDrab", "Orange", "Pink", "Plum", "Purple", "Red", "Sienna", "SkyBlue", "Steel", "Taupe", "Teal", "Violet", "Yellow"];

    // ── View Settings ───────────────────────────────────────────

    [ObservableProperty] private int _thumbnailSize = 250;

    [ObservableProperty] private ObservableCollection<int> _thumbnailSizes = [50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800];

    [ObservableProperty] private int _gamesPerPage = 200;

    [ObservableProperty] private ObservableCollection<int> _gamesPerPageOptions = [100, 200, 300, 400, 500, 1000, 10000, 1000000];

    [ObservableProperty] private string _selectedViewMode = "GridView";

    [ObservableProperty] private ObservableCollection<string> _viewModes = ["GridView", "ListView"];

    [ObservableProperty] private string _selectedButtonAspectRatio = "Square";

    [ObservableProperty] private ObservableCollection<string> _buttonAspectRatios = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

    // ── Filename Display ────────────────────────────────────────

    [ObservableProperty] private string _selectedFilenameDisplayMode = "Original";

    [ObservableProperty] private ObservableCollection<string> _filenameDisplayModes = ["Original", "CleanUp", "NoFilename"];

    [ObservableProperty] private string _selectedFilenameFontSize = "Normal";

    [ObservableProperty] private ObservableCollection<string> _fontSizes = ["Small", "Normal", "Big"];

    [ObservableProperty] private string _selectedMachineNameFontSize = "Normal";

    // ── Gamepad Settings ────────────────────────────────────────

    [ObservableProperty] private bool _enableGamePadNavigation;

    [ObservableProperty] private float _deadZoneX = SettingsManager.DefaultDeadZoneX;

    [ObservableProperty] private float _deadZoneY = SettingsManager.DefaultDeadZoneY;

    // ── Sound Settings ──────────────────────────────────────────

    [ObservableProperty] private bool _enableNotificationSound = true;

    // ── RetroAchievements Settings ──────────────────────────────

    [ObservableProperty] private string _raUsername = string.Empty;

    [ObservableProperty] private string _raApiKey = string.Empty;

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            // Apply settings to SettingsManager
            _settings.Language = SelectedLanguage;
            _settings.BaseTheme = SelectedBaseTheme;
            _settings.AccentColor = SelectedAccentColor;
            _settings.ThumbnailSize = ThumbnailSize;
            _settings.GamesPerPage = GamesPerPage;
            _settings.ViewMode = SelectedViewMode;
            _settings.ButtonAspectRatio = SelectedButtonAspectRatio;
            _settings.FilenameDisplayMode = SelectedFilenameDisplayMode;
            _settings.FilenameFontSize = SelectedFilenameFontSize;
            _settings.MachineNameFontSize = SelectedMachineNameFontSize;
            _settings.EnableGamePadNavigation = EnableGamePadNavigation;
            _settings.DeadZoneX = DeadZoneX;
            _settings.DeadZoneY = DeadZoneY;
            _settings.EnableNotificationSound = EnableNotificationSound;
            _settings.RaUsername = RaUsername;
            _settings.RaApiKey = RaApiKey;

            // Save to file
            await _settings.SaveAsync();

            await _messageDialog.ShowInfoAsync("Settings saved successfully. Some changes may require a restart.", "Settings Saved");
        }
        catch (Exception ex)
        {
            await _messageDialog.ShowErrorAsync($"Failed to save settings: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        // Settings are not saved
    }

    // ── Private Methods ─────────────────────────────────────────

    private void LoadSettings()
    {
        SelectedLanguage = _settings.Language ?? "en";
        SelectedBaseTheme = _settings.BaseTheme ?? "Dark";
        SelectedAccentColor = _settings.AccentColor ?? "Blue";
        ThumbnailSize = _settings.ThumbnailSize;
        GamesPerPage = _settings.GamesPerPage;
        SelectedViewMode = _settings.ViewMode ?? "GridView";
        SelectedButtonAspectRatio = _settings.ButtonAspectRatio ?? "Square";
        SelectedFilenameDisplayMode = _settings.FilenameDisplayMode ?? "Original";
        SelectedFilenameFontSize = _settings.FilenameFontSize ?? "Normal";
        SelectedMachineNameFontSize = _settings.MachineNameFontSize ?? "Normal";
        EnableGamePadNavigation = _settings.EnableGamePadNavigation;
        DeadZoneX = _settings.DeadZoneX;
        DeadZoneY = _settings.DeadZoneY;
        EnableNotificationSound = _settings.EnableNotificationSound;
        RaUsername = _settings.RaUsername ?? string.Empty;
        RaApiKey = _settings.RaApiKey ?? string.Empty;
    }
}
