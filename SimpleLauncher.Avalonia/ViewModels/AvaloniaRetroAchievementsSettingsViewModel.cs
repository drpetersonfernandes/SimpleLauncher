using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class AvaloniaRetroAchievementsSettingsViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private string _password;

    public AvaloniaRetroAchievementsSettingsViewModel(
        SettingsManager settings,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IDebugLogger debugLogger)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors;
        _messageBox = messageBox;
        _debugLogger = debugLogger;

        _username = _settings.RaUsername ?? string.Empty;
        _apiKey = _settings.RaApiKey ?? string.Empty;
        _password = _settings.RaPassword ?? string.Empty;
    }

    public event Action? SaveCompleted;
    public event Action? CloseRequested;

    [RelayCommand]
    private async Task SaveAsync()
    {
        SaveCurrentSettings();

        try
        {
            Process.Start(new ProcessStartInfo("https://retroachievements.org/controlpanel.php") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error opening RetroAchievements control panel.");
        }

        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }

    private void SaveCurrentSettings()
    {
        _settings.RaUsername = Username;
        _settings.RaApiKey = ApiKey;
        _settings.RaPassword = Password;
        _settings.SaveAsync();
    }
}
