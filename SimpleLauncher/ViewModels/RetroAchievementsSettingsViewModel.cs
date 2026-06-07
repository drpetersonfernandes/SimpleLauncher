using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
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

    [ObservableProperty] private string _username;
    [ObservableProperty] private string _apiKey;
    [ObservableProperty] private string _password;

    public RetroAchievementsSettingsViewModel(SettingsManager settings, ILogErrors logErrors)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors;

        _username = _settings.RaUsername;
        _apiKey = _settings.RaApiKey;
        _password = _settings.RaPassword;
    }

    /// <summary>Event raised when settings have been saved successfully.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised to request the emulator executable path from the view.</summary>
    public event Func<string> RequestExePath;

    [RelayCommand]
    private void Save()
    {
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("SavingRetroAchievementsSettings") ?? "Saving RetroAchievements settings...");

        _settings.RaUsername = (Username).Trim();
        _settings.RaApiKey = ApiKey;
        _settings.RaPassword = Password;
        _settings.SaveAsync();

        try
        {
            Process.Start(new ProcessStartInfo("https://retroachievements.org/controlpanel.php") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error opening RetroAchievements control panel link.");
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }

        SaveCompleted?.Invoke();
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
                MessageBoxLibrary.EnterUsernamePasswordMessageBox();
                return;
            }

            SaveCurrentSettings();

            var token = _settings.RaToken;
            if (emulatorName != "RetroArch")
            {
                if (string.IsNullOrEmpty(token) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
                {
                    (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
                        (string)Application.Current.TryFindResource("RaStatusLoggingIn") ?? "Logging in to RetroAchievements...");

                    var raService = App.ServiceProvider.GetRequiredService<RetroAchievementsService>();
                    token = await raService.GetSessionTokenAsync(username, password);

                    if (!string.IsNullOrEmpty(token))
                    {
                        _settings.RaToken = token;
                        await _settings.SaveAsync();
                    }
                    else
                    {
                        MessageBoxLibrary.FailedToLoginToRetroAchievementsMessageBox();
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
                    case "RetroArch": success = RetroAchievementsEmulatorConfiguratorService.ConfigureRetroArch(exePath, username, password, _logErrors); break;
                    case "PCSX2": success = RetroAchievementsEmulatorConfiguratorService.ConfigurePcsx2(exePath, username, token, _logErrors); break;
                    case "DuckStation": success = RetroAchievementsEmulatorConfiguratorService.ConfigureDuckStation(exePath, username, token, _logErrors); break;
                    case "PPSSPP": success = RetroAchievementsEmulatorConfiguratorService.ConfigurePpspp(exePath, username, token, _logErrors); break;
                    case "Dolphin": success = RetroAchievementsEmulatorConfiguratorService.ConfigureDolphin(exePath, username, token, _logErrors); break;
                    case "Flycast": success = RetroAchievementsEmulatorConfiguratorService.ConfigureFlycast(exePath, username, token, _logErrors); break;
                    case "BizHawk": success = RetroAchievementsEmulatorConfiguratorService.ConfigureBizHawk(exePath, username, token, _logErrors); break;
                }

                if (success)
                {
                    MessageBoxLibrary.EmulatorConfiguredSuccessfullyMessageBox();
                }
                else
                {
                    MessageBoxLibrary.FailedToConfigureTheEmulatorMessageBox();
                }
            }
            catch (Exception ex)
            {
                MessageBoxLibrary.AnErrorOccurredWhileConfiguringTheEmulatorMessageBox();
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
        _settings.SaveAsync();
    }
}
