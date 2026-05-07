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

public partial class InjectStellaConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectStellaConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        ChkFullscreen.IsChecked = _settings.StellaFullscreen;
        ChkVsync.IsChecked = _settings.StellaVsync;
        ChkCorrectAspect.IsChecked = _settings.StellaCorrectAspect;
        CmbVideoDriver.Text = _settings.StellaVideoDriver;
        SelectComboByTag(CmbTvFilter, _settings.StellaTvFilter.ToString(CultureInfo.InvariantCulture));
        SldScanlines.Value = _settings.StellaScanlines;
        ChkAudioEnabled.IsChecked = _settings.StellaAudioEnabled;
        SldAudioVolume.Value = _settings.StellaAudioVolume;
        ChkTimeMachine.IsChecked = _settings.StellaTimeMachine;
        ChkConfirmExit.IsChecked = _settings.StellaConfirmExit;
        ChkShowBeforeLaunch.IsChecked = _settings.StellaShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        // Try to resolve from system.xml
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Stella");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.StellaEmulatorNotFoundMessageBox();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Stella Executable|stella.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectStellaEmulator") ?? "Select Stella Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.StellaFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.StellaVsync = ChkVsync.IsChecked ?? true;
        _settings.StellaCorrectAspect = ChkCorrectAspect.IsChecked ?? true;
        _settings.StellaVideoDriver = CmbVideoDriver.Text;
        _settings.StellaTvFilter = int.Parse(GetSelectedTag(CmbTvFilter), CultureInfo.InvariantCulture);
        _settings.StellaScanlines = (int)SldScanlines.Value;
        _settings.StellaAudioEnabled = ChkAudioEnabled.IsChecked ?? true;
        _settings.StellaAudioVolume = (int)SldAudioVolume.Value;
        _settings.StellaTimeMachine = ChkTimeMachine.IsChecked ?? true;
        _settings.StellaConfirmExit = ChkConfirmExit.IsChecked ?? false;
        _settings.StellaShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            StellaConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Stella configuration injection failed for path: {path}");
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
                MessageBoxLibrary.StellaConfigurationSavedSuccessfullyMessageBox();
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

    private static void SelectComboByTag(ComboBox cmb, string tagValue)
    {
        foreach (var item in cmb.Items.OfType<ComboBoxItem>())
        {
            if (item.Tag?.ToString() == tagValue)
            {
                cmb.SelectedItem = item;
                return;
            }
        }

        if (cmb.Items.Count > 0)
        {
            cmb.SelectedIndex = 0;
        }
    }

    private static string GetSelectedTag(ComboBox cmb)
    {
        return (cmb.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "0";
    }
}