using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectMesenConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectMesenConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        ChkFullscreen.IsChecked = _settings.MesenFullscreen;
        ChkVsync.IsChecked = _settings.MesenVsync;
        CmbAspectRatio.Text = _settings.MesenAspectRatio;
        ChkBilinear.IsChecked = _settings.MesenBilinear;
        CmbVideoFilter.Text = _settings.MesenVideoFilter;
        ChkEnableAudio.IsChecked = _settings.MesenEnableAudio;
        SldMasterVolume.Value = _settings.MesenMasterVolume;
        ChkRewind.IsChecked = _settings.MesenRewind;
        SldRunAhead.Value = _settings.MesenRunAhead;
        ChkPauseInBackground.IsChecked = _settings.MesenPauseInBackground;
        ChkShowBeforeLaunch.IsChecked = _settings.MesenShowSettingsBeforeLaunch;

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
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Mesen", _logErrors);
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MesenEmulatorNotFoundMessageBox();

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Mesen Executable|Mesen.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMesenEmulator") ?? "Select Mesen Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.MesenFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.MesenVsync = ChkVsync.IsChecked ?? false;
        _settings.MesenAspectRatio = CmbAspectRatio.Text;
        _settings.MesenBilinear = ChkBilinear.IsChecked ?? false;
        _settings.MesenVideoFilter = CmbVideoFilter.Text;
        _settings.MesenEnableAudio = ChkEnableAudio.IsChecked ?? true;
        _settings.MesenMasterVolume = (int)SldMasterVolume.Value;
        _settings.MesenRewind = ChkRewind.IsChecked ?? false;
        _settings.MesenRunAhead = (int)SldRunAhead.Value;
        _settings.MesenPauseInBackground = ChkPauseInBackground.IsChecked ?? false;
        _settings.MesenShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            MesenConfigurationService.InjectSettings(path, _settings, _logErrors);
            return true;
        }
        catch (Exception ex)
        {
            _logErrors.LogErrorAsync(ex, $"Mesen configuration injection failed for path: {path}");
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
                MessageBoxLibrary.MesenConfigurationSavedSuccessfullyMessageBox();
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