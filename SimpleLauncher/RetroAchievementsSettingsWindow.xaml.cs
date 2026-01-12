using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using SimpleLauncher.Services.RetroAchievements;

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
        RaPasswordPasswordBox.Password = _settings.RaPassword;
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingRetroAchievementsSettings") ?? "Saving RetroAchievements settings...", Application.Current.MainWindow as MainWindow);

        var newUsername = UsernameTextBox.Text.Trim();
        var newApiKey = ApiKeyPasswordBox.Password;
        var newPassword = RaPasswordPasswordBox.Password;

        _settings.RaUsername = newUsername;
        _settings.RaApiKey = newApiKey;
        _settings.RaPassword = newPassword;
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error opening RetroAchievements control panel link.");
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }

    private void ConfigureRetroArch_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("RetroArch");
    }

    private void ConfigurePcsx2_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("PCSX2");
    }

    private void ConfigureDuckStation_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("DuckStation");
    }

    private void ConfigurePpspp_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("PPSSPP");
    }

    private void ConfigureDolphin_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("Dolphin");
    }

    private void ConfigureFlycast_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("Flycast");
    }

    private void ConfigureBizHawk_Click(object sender, RoutedEventArgs e)
    {
        ConfigureEmulator("BizHawk");
    }

    private void ConfigureEmulator(string emulatorName)
    {
        var username = UsernameTextBox.Text.Trim();
        var apiKey = ApiKeyPasswordBox.Password;
        var password = RaPasswordPasswordBox.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            MessageBox.Show("Please enter your RetroAchievements username and API key before configuring an emulator.", "Credentials Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = $"Select {emulatorName} Executable",
            Filter = "Executable files (*.exe)|*.exe"
        };

        if (openFileDialog.ShowDialog() != true) return;

        var exePath = openFileDialog.FileName;
        var success = false;
        try
        {
            switch (emulatorName)
            {
                case "RetroArch": success = RetroAchievementsEmulatorConfiguratorService.ConfigureRetroArch(exePath, username, apiKey, password); break;
                // case "PCSX2": success = RetroAchievementsEmulatorConfiguratorService.ConfigurePcsx2(exePath, username, apiKey, password); break;
                // case "DuckStation": success = RetroAchievementsEmulatorConfiguratorService.ConfigureDuckStation(exePath, username, apiKey, password); break;
                // case "PPSSPP": success = RetroAchievementsEmulatorConfiguratorService.ConfigurePpspp(exePath, username, apiKey, password); break;
                // case "Dolphin": success = RetroAchievementsEmulatorConfiguratorService.ConfigureDolphin(exePath, username, apiKey, password); break;
                // case "Flycast": success = RetroAchievementsEmulatorConfiguratorService.ConfigureFlycast(exePath, username, apiKey, password); break;
                // case "BizHawk": success = RetroAchievementsEmulatorConfiguratorService.ConfigureBizHawk(exePath, username, apiKey, password); break;
            }

            if (success)
            {
                MessageBox.Show($"{emulatorName} configured successfully for RetroAchievements!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Failed to configure {emulatorName}. The configuration file might be missing, in an unexpected location, or read-only.", "Configuration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred while configuring {emulatorName}:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to configure {emulatorName}.");
        }
    }
}