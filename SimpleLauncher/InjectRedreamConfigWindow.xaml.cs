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

public partial class InjectRedreamConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectRedreamConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        // Video
        CmbCable.Text = _settings.RedreamCable;
        CmbBroadcast.Text = _settings.RedreamBroadcast;
        CmbVsync.SelectedIndex = _settings.RedreamVsync ? 0 : 1;
        CmbFrameskip.SelectedIndex = _settings.RedreamFrameskip ? 0 : 1;
        CmbAspect.Text = _settings.RedreamAspect;
        CmbRes.Text = _settings.RedreamRes.ToString(CultureInfo.InvariantCulture);
        CmbRenderer.Text = _settings.RedreamRenderer;
        // Select Fullmode by Tag
        foreach (ComboBoxItem item in CmbFullmode.Items)
        {
            if (item.Tag?.ToString() == _settings.RedreamFullmode)
            {
                CmbFullmode.SelectedItem = item;
                break;
            }
        }

        CmbWindowSize.Text = $"{_settings.RedreamWidth}x{_settings.RedreamHeight}";

        // System
        CmbLanguage.Text = _settings.RedreamLanguage;
        CmbRegion.Text = _settings.RedreamRegion;
        SldVolume.Value = _settings.RedreamVolume;
        CmbLatency.Text = _settings.RedreamLatency.ToString(CultureInfo.InvariantCulture);
        CmbFramerate.SelectedIndex = _settings.RedreamFramerate ? 1 : 0;

        ChkShowBeforeLaunch.IsChecked = _settings.RedreamShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        // Video
        _settings.RedreamCable = CmbCable.Text;
        _settings.RedreamBroadcast = CmbBroadcast.Text;
        _settings.RedreamVsync = (CmbVsync.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "True";
        _settings.RedreamFrameskip = (CmbFrameskip.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "True";
        _settings.RedreamAspect = CmbAspect.Text;
        if (int.TryParse(CmbRes.Text, out var res))
        {
            _settings.RedreamRes = res;
        }

        _settings.RedreamRenderer = CmbRenderer.Text;

        var fullmodeTag = (CmbFullmode.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "windowed";
        // Validate fullmode value
        if (fullmodeTag != "windowed" && fullmodeTag != "exclusive fullscreen" && fullmodeTag != "borderless fullscreen")
        {
            fullmodeTag = "windowed";
        }

        _settings.RedreamFullmode = fullmodeTag;

        // Extract only numbers from resolution string (e.g. "1920x1080 (16:9)" -> "1920", "1080")
        var rawSize = CmbWindowSize.Text.Split(' ')[0];
        var sizeParts = rawSize.Split('x');
        if (sizeParts.Length == 2 &&
            int.TryParse(sizeParts[0], out var w) &&
            int.TryParse(sizeParts[1], out var h))
        {
            _settings.RedreamWidth = w;
            _settings.RedreamHeight = h;
        }

        // System
        _settings.RedreamLanguage = CmbLanguage.Text;
        _settings.RedreamRegion = CmbRegion.Text;
        _settings.RedreamVolume = (int)SldVolume.Value;
        if (int.TryParse(CmbLatency.Text, out var lat))
        {
            _settings.RedreamLatency = lat;
        }

        _settings.RedreamFramerate = (CmbFramerate.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "True";

        _settings.RedreamShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        MessageBoxLibrary.ReDreamEmulatorPathNotFound();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Redream Executable|redream.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectRedreamEmulator") ?? "Select Redream Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            return false;

        try
        {
            RedreamConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Redream configuration injection failed for path: {path}");
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
        else
        {
            MessageBoxLibrary.FailedToInjectReDreamConfiguration();
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (InjectConfig())
        {
            MessageBoxLibrary.ReDreamConfigurationInjectedSuccessfully();
            Close();
        }
        else
        {
            MessageBoxLibrary.FailedToInjectReDreamConfiguration();
        }
    }
}