using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectBlastemConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectBlastemConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        ChkFullscreen.IsChecked = _settings.BlastemFullscreen;
        ChkVsync.IsChecked = _settings.BlastemVsync;
        ChkScanlines.IsChecked = _settings.BlastemScanlines;
        CmbAspect.Text = _settings.BlastemAspect;
        CmbScaling.Text = _settings.BlastemScaling;
        CmbAudioRate.Text = _settings.BlastemAudioRate.ToString(CultureInfo.InvariantCulture);
        CmbSyncSource.Text = _settings.BlastemSyncSource;
        ChkShowBeforeLaunch.IsChecked = _settings.BlastemShowSettingsBeforeLaunch;

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
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Blastem");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.BlastemEmulatorNotFoundMessageBox();
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Blastem Executable|blastem.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectBlastemEmulator") ?? "Select Blastem Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.BlastemFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.BlastemVsync = ChkVsync.IsChecked ?? false;
        _settings.BlastemScanlines = ChkScanlines.IsChecked ?? false;
        _settings.BlastemAspect = CmbAspect.Text;
        _settings.BlastemScaling = CmbScaling.Text;
        _settings.BlastemAudioRate = int.Parse(CmbAudioRate.Text, CultureInfo.InvariantCulture);
        _settings.BlastemSyncSource = CmbSyncSource.Text;
        _settings.BlastemShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private bool InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            BlastemConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (FileNotFoundException ex)
        {
            var errorMsg = $"Configuration file not found for Blastem at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMsg = $"Permission denied accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (IOException ex)
        {
            var errorMsg = $"I/O error while accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return false;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Blastem configuration injection failed for path: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
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
                MessageBoxLibrary.BlastemConfigurationSavedSuccessfullyMessageBox();
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
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
        }
    }
}