using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the RetroAchievements settings window.
/// </summary>
public partial class RetroAchievementsSettingsViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly RetroAchievementsService _raService;
    private readonly IResourceProvider _resourceProvider;
    private readonly IRetroAchievementsEmulatorConfiguratorService _configurator;

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private string _password;

    public RetroAchievementsSettingsViewModel(SettingsManager settings, ILogErrors logErrors, IMessageBoxLibraryService messageBox, RetroAchievementsService raService, IResourceProvider resourceProvider, IRetroAchievementsEmulatorConfiguratorService configurator)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors;
        _messageBox = messageBox;
        _raService = raService;
        _resourceProvider = resourceProvider;
        _configurator = configurator;

        _username = _settings.RaUsername;
        _apiKey = _settings.RaApiKey;
        _password = _settings.RaPassword;
    }

    /// <summary>Event raised when settings have been saved successfully.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised to request the emulator executable path from the view.</summary>
    public event Func<string> RequestExePath;

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                _resourceProvider.GetString("SavingRetroAchievementsSettings", "Saving RetroAchievements settings..."));

            _settings.RaUsername = (Username).Trim();
            _settings.RaApiKey = ApiKey;
            _settings.RaPassword = Password;
            await _settings.SaveAsync();

            Process.Start(new ProcessStartInfo("https://retroachievements.org/controlpanel.php") { UseShellExecute = true });

            SaveCompleted?.Invoke();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error saving RetroAchievements settings.");
            await _messageBox.FailedToSaveSettingsMessageBoxAsync();
        }
    }

    [RelayCommand]
    private async Task ConfigureEmulatorAsync(string emulatorName)
    {
        try
        {
            var username = (Username).Trim();
            var password = Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await _messageBox.EnterUsernamePasswordMessageBoxAsync();
                return;
            }

            SaveCurrentSettings();

            var token = _settings.RaToken;
            if (emulatorName != "RetroArch")
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
                {
                    (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                        _resourceProvider.GetString("RaStatusLoggingIn", "Logging in to RetroAchievements..."));

                    token = await _raService.GetSessionTokenAsync(username, password);

                    if (!string.IsNullOrEmpty(token))
                    {
                        _settings.RaToken = token;
                        await _settings.SaveAsync();
                    }
                    else
                    {
                        await _messageBox.FailedToLoginToRetroAchievementsMessageBoxAsync();
                        return;
                    }
                }
            }

            var exePath = RequestExePath?.Invoke();
            if (string.IsNullOrEmpty(exePath)) return;

            var success = false;
            try
            {
                switch (emulatorName)
                {
                    case "RetroArch": success = _configurator.ConfigureRetroArch(exePath, username, password); break;
                    case "PCSX2": success = _configurator.ConfigurePcsx2(exePath, username, token); break;
                    case "DuckStation": success = _configurator.ConfigureDuckStation(exePath, username, token); break;
                    case "PPSSPP": success = _configurator.ConfigurePpspp(exePath, username, token); break;
                    case "Dolphin": success = _configurator.ConfigureDolphin(exePath, username, token); break;
                    case "Flycast": success = _configurator.ConfigureFlycast(exePath, username, token); break;
                    case "BizHawk": success = _configurator.ConfigureBizHawk(exePath, username, token); break;
                }

                if (success)
                {
                    await _messageBox.EmulatorConfiguredSuccessfullyMessageBoxAsync();
                }
                else
                {
                    await _messageBox.FailedToConfigureTheEmulatorMessageBoxAsync();
                }
            }
            catch (Exception ex)
            {
                await _messageBox.AnErrorOccurredWhileConfiguringTheEmulatorMessageBoxAsync();
                _logErrors.LogAndForget(ex, $"Failed to configure {emulatorName}.");
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in ConfigureEmulator method.");
        }
    }

    private void SaveCurrentSettings()
    {
        _settings.RaUsername = (Username).Trim();
        _settings.RaApiKey = ApiKey;
        _settings.RaPassword = Password;
        _ = _settings.SaveAsync();
    }
}
