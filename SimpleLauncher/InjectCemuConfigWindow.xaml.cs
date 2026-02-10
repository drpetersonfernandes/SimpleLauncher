using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectCemuConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectCemuConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _emulatorPath = emulatorPath;
        _isLauncherMode = isLauncherMode;
        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        LoadSettings();
    }

    private void LoadSettings()
    {
        ChkFullscreen.IsChecked = _settings.CemuFullscreen;
        SelectComboByTag(CmbApi, _settings.CemuGraphicApi.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbVsync, _settings.CemuVsync.ToString(CultureInfo.InvariantCulture));
        ChkAsyncCompile.IsChecked = _settings.CemuAsyncCompile;
        SldVolume.Value = _settings.CemuTvVolume;

        // Ensure ComboBoxes have a valid selection
        if (CmbApi.SelectedItem == null && CmbApi.Items.Count > 0)
        {
            CmbApi.SelectedIndex = 1; // Default to Vulkan (index 1, Tag="1")
        }

        if (CmbVsync.SelectedItem == null && CmbVsync.Items.Count > 0)
        {
            CmbVsync.SelectedIndex = 1; // Default to On (index 1, Tag="1")
        }

        if (CmbLanguage.SelectedItem == null && CmbLanguage.Items.Count > 0)
        {
            CmbLanguage.SelectedIndex = 1; // Default to English (index 1, Tag="1")
        }

        ChkDiscord.IsChecked = _settings.CemuDiscordPresence;
        SelectComboByTag(CmbLanguage, _settings.CemuConsoleLanguage.ToString(CultureInfo.InvariantCulture));
        ChkShowBeforeLaunch.IsChecked = _settings.CemuShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath)) return _emulatorPath;

        MessageBoxLibrary.Cemuemulatornotfound();

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Cemu Executable|Cemu.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectCemuEmulator") ?? "Select Cemu Emulator"
        };

        if (dialog.ShowDialog() != true)
        {
            return null;
        }

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.CemuFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.CemuGraphicApi = int.Parse(GetSelectedTag(CmbApi), CultureInfo.InvariantCulture);
        _settings.CemuVsync = int.Parse(GetSelectedTag(CmbVsync), CultureInfo.InvariantCulture);
        _settings.CemuAsyncCompile = ChkAsyncCompile.IsChecked ?? false; // Match XAML default (unchecked)
        _settings.CemuTvVolume = (int)SldVolume.Value;
        _settings.CemuDiscordPresence = ChkDiscord.IsChecked ?? true;
        _settings.CemuConsoleLanguage = int.Parse(GetSelectedTag(CmbLanguage), CultureInfo.InvariantCulture);
        _settings.CemuShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;
        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path)) return false;

        try
        {
            CemuConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Cemu injection failed: {path}");
            return false;
        }
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            ShouldRun = true;
            Close();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var injected = InjectConfig();
        if (injected)
        {
            MessageBoxLibrary.CemuConfigurationSaved();
            ShouldRun = false; // Explicitly set for clarity
            Close();
        }
        // If injection failed, don't close - let user see error or retry
    }

    private static void SelectComboByTag(ComboBox cmb, string tag)
    {
        foreach (ComboBoxItem item in cmb.Items)
            if (item.Tag?.ToString() == tag)
            {
                cmb.SelectedItem = item;
                return;
            }
    }

    private static string GetSelectedTag(ComboBox cmb)
    {
        var tag = (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString();
        if (string.IsNullOrEmpty(tag))
        {
            throw new InvalidOperationException($"No valid selection in ComboBox '{cmb.Name}'. Please select a value.");
        }

        return tag;
    }
}