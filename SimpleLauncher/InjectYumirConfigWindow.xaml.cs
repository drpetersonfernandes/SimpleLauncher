using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class InjectYumirConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectYumirConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        ChkFullscreen.IsChecked = _settings.YumirFullscreen;
        ChkForceAspect.IsChecked = _settings.YumirForceAspectRatio;
        ChkReduceLatency.IsChecked = _settings.YumirReduceLatency;
        ChkMute.IsChecked = _settings.YumirMute;
        SldVolume.Value = _settings.YumirVolume;
        ChkAutoRegion.IsChecked = _settings.YumirAutoDetectRegion;
        CmbVideoStandard.Text = _settings.YumirVideoStandard;
        ChkPauseUnfocused.IsChecked = _settings.YumirPauseWhenUnfocused;
        ChkShowBeforeLaunch.IsChecked = _settings.YumirShowSettingsBeforeLaunch;

        foreach (ComboBoxItem item in CmbForcedAspect.Items)
        {
            if (item.Tag.ToString() == _settings.YumirForcedAspect.ToString(CultureInfo.InvariantCulture))
            {
                CmbForcedAspect.SelectedItem = item;
                break;
            }
        }

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.YumirFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.YumirForceAspectRatio = ChkForceAspect.IsChecked ?? false;
        _settings.YumirReduceLatency = ChkReduceLatency.IsChecked ?? true;
        _settings.YumirMute = ChkMute.IsChecked ?? false;
        _settings.YumirVolume = SldVolume.Value;
        _settings.YumirAutoDetectRegion = ChkAutoRegion.IsChecked ?? true;
        _settings.YumirVideoStandard = CmbVideoStandard.Text;
        _settings.YumirPauseWhenUnfocused = ChkPauseUnfocused.IsChecked ?? false;
        _settings.YumirShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        if (CmbForcedAspect.SelectedItem is ComboBoxItem selected)
        {
            _settings.YumirForcedAspect = double.Parse(selected.Tag.ToString() ?? "1.7777777777777777", CultureInfo.InvariantCulture);
        }
        else
        {
            _settings.YumirForcedAspect = 1.7777777777777777;
        }

        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        // Try to resolve from system.xml
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Yumir");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.YumirEmulatorNotFound(); // Ensure this exists in MessageBoxLibrary

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Yumir Executable|ymir.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectYumirEmulator") ?? "Select Yumir Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            YumirConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, "Yumir injection failed.");
            return false;
        }
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                ShouldRun = true;
                Close();
            }
            else
            {
                // Injection failed but was already logged inside InjectConfig.
                // Notify user and close without generating a duplicate report.
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
                Close();
                ShouldRun = true; // Game should still launch
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled - close silently
            Close();
        }
        catch (Exception ex)
        {
            // Injection failed: Notify user → Notify developer → Close window → Launch game
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
            ShouldRun = true; // Game should still launch
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        try
        {
            if (InjectConfig())
            {
                MessageBoxLibrary.YumirConfigurationSavedSuccessfully();
                Close();
            }
            else
            {
                // Injection failed but was already logged inside InjectConfig.
                // Notify user and close without generating a duplicate report.
                MessageBoxLibrary.InjectionFailedGenericMessageBox();
                Close();
            }
        }
        catch (OperationCanceledException)
        {
            // User cancelled - close silently
            Close();
        }
        catch (Exception ex)
        {
            // Injection failed: Notify user → Notify developer → Close window
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
        }
    }
}