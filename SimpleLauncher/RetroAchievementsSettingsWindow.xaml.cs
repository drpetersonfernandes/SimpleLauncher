using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
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
        _settings.RaUsername = UsernameTextBox.Text.Trim();
        _settings.RaApiKey = ApiKeyPasswordBox.Password; // No trim for password
        _settings.Save();

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
            _ = LogErrors.LogErrorAsync(ex, "Error opening RetroAchievements control panel link.");
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }
}