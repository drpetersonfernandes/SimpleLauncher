using System.Globalization;
using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services;

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

        MessageBoxLibrary.BlastemEmulatorNotFound();
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

    private record InjectionResult(bool Success, string ErrorMessage, Exception OriginalException);

    private InjectionResult InjectConfig()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
        {
            var errorMsg = string.IsNullOrEmpty(_emulatorPath)
                ? "Blastem emulator path is not configured."
                : $"Blastem emulator not found at: {_emulatorPath}";
            return new InjectionResult(false, errorMsg, null);
        }

        try
        {
            BlastemConfigurationService.InjectSettings(path, _settings);
            return new InjectionResult(true, null, null);
        }
        catch (FileNotFoundException ex)
        {
            var errorMsg = $"Configuration file not found for Blastem at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return new InjectionResult(false, errorMsg, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMsg = $"Permission denied accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return new InjectionResult(false, errorMsg, ex);
        }
        catch (IOException ex)
        {
            var errorMsg = $"I/O error while accessing Blastem configuration at: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return new InjectionResult(false, errorMsg, ex);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Blastem configuration injection failed for path: {path}. Details: {ex.Message}";
            _logErrors.LogErrorAsync(ex, errorMsg);
            return new InjectionResult(false, errorMsg, ex);
        }
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var result = InjectConfig();

        if (result.Success)
        {
            ShouldRun = true;
            Close();
        }
        else
        {
            // Injection failed: Notify user → Notify developer → Close window → Launch game
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            var exception = result.OriginalException ?? new InvalidOperationException(result.ErrorMessage);
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, exception, emulatorName, _emulatorPath, this);
            ShouldRun = true; // Game should still launch
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        var result = InjectConfig();

        if (result.Success)
        {
            MessageBoxLibrary.BlastemConfigurationSavedSuccessfully();
            Close();
        }
        else
        {
            // Injection failed: Notify user → Notify developer → Close window
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            var exception = result.OriginalException ?? new InvalidOperationException(result.ErrorMessage);
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, exception, emulatorName, _emulatorPath, this);
        }
    }
}