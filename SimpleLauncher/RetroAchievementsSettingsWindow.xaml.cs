using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class RetroAchievementsSettingsWindow
{
    private readonly SettingsManager _settings;

    public RetroAchievementsSettingsWindow(SettingsManager settings)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        LoadSettings();
    }

    private void LoadSettings()
    {
        UsernameTextBox.Text = _settings.RaUsername;
        ApiKeyPasswordBox.Password = _settings.RaApiKey;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingRetroAchievementsSettings") ?? "Saving RetroAchievements settings...", Application.Current.MainWindow as MainWindow);

        var oldUsername = _settings.RaUsername;
        var oldApiKey = _settings.RaApiKey;

        var newUsername = UsernameTextBox.Text.Trim();
        var newApiKey = ApiKeyPasswordBox.Password;

        _settings.RaUsername = newUsername;
        _settings.RaApiKey = newApiKey;
        _settings.Save();

        // If credentials changed, clear the RetroAchievements cache
        if (!string.Equals(oldUsername, newUsername, StringComparison.Ordinal) || !string.Equals(oldApiKey, newApiKey, StringComparison.Ordinal))
        {
            var raService = App.ServiceProvider.GetRequiredService<RetroAchievementsService>();
            raService.ClearCache();
        }

        DialogResult = true;
        Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            _ = LogErrorsService.LogErrorAsync(ex, "Error opening RetroAchievements control panel link.");
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }
}