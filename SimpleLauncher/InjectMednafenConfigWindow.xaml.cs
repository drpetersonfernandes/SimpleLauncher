using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;

namespace SimpleLauncher;

public partial class InjectMednafenConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectMednafenConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        SelectItemByTag(CmbVideoDriver, _settings.MednafenVideoDriver);
        SelectItemByTag(CmbStretch, _settings.MednafenStretch);

        // Logic to load the correct item in the Shader/Scaler ComboBox
        if (!string.IsNullOrEmpty(_settings.MednafenSpecial) && _settings.MednafenSpecial != "none")
        {
            SelectItemByTag(CmbShader, _settings.MednafenSpecial);
        }
        else
            SelectItemByTag(CmbShader, _settings.MednafenShader);

        ChkFullscreen.IsChecked = _settings.MednafenFullscreen;
        ChkVsync.IsChecked = _settings.MednafenVsync;
        ChkBilinear.IsChecked = _settings.MednafenBilinear;
        SldScanlines.Value = _settings.MednafenScanlines;
        SldVolume.Value = _settings.MednafenVolume;
        ChkCheats.IsChecked = _settings.MednafenCheats;
        ChkRewind.IsChecked = _settings.MednafenRewind;
        ChkShowBeforeLaunch.IsChecked = _settings.MednafenShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private static void SelectItemByTag(System.Windows.Controls.ComboBox comboBox, string tagValue)
    {
        foreach (System.Windows.Controls.ComboBoxItem item in comboBox.Items)
        {
            if (item.Tag?.ToString() == tagValue)
            {
                comboBox.SelectedItem = item;
                return;
            }
        }

        comboBox.SelectedIndex = 0; // Fallback
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        // Try to resolve from system.xml
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Mednafen");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        MessageBoxLibrary.MednafenEmulatorNotFound();

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Mednafen Executable|mednafen.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMednafenEmulator") ?? "Select Mednafen Emulator"
        };

        if (dialog.ShowDialog() != true) return null;

        _emulatorPath = dialog.FileName;
        return _emulatorPath;
    }

    private void SaveSettings()
    {
        _settings.MednafenVideoDriver = (CmbVideoDriver.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "opengl";
        _settings.MednafenStretch = (CmbStretch.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "aspect";

        // Logic to split Shader vs Special Scaler
        var selectedShaderTag = (CmbShader.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "none";
        if (selectedShaderTag is "scale2x" or "snes_ntsc")
        {
            _settings.MednafenSpecial = selectedShaderTag;
            _settings.MednafenShader = "none";
        }
        else
        {
            _settings.MednafenSpecial = "none";
            _settings.MednafenShader = selectedShaderTag;
        }

        _settings.MednafenFullscreen = ChkFullscreen.IsChecked ?? false;
        _settings.MednafenVsync = ChkVsync.IsChecked ?? true;
        _settings.MednafenBilinear = ChkBilinear.IsChecked ?? false;
        _settings.MednafenScanlines = (int)SldScanlines.Value;
        _settings.MednafenVolume = (int)SldVolume.Value;
        _settings.MednafenCheats = ChkCheats.IsChecked ?? true;
        _settings.MednafenRewind = ChkRewind.IsChecked ?? false;
        _settings.MednafenShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        _settings.Save();
    }

    private async Task<bool> InjectConfigAsync()
    {
        var path = EnsureEmulatorPath();
        if (string.IsNullOrEmpty(path))
            throw new OperationCanceledException("User cancelled emulator path selection.");

        try
        {
            MednafenConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, $"Mednafen configuration injection failed for path: {path}");
            return false;
        }
    }

    private async void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            if (await InjectConfigAsync())
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
            _ = _logErrors.LogErrorAsync(ex, "Error in the BtnRun_Click method.");
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleRunButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
            ShouldRun = true; // Game should still launch
        }
    }

    private async void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            SaveSettings();
            if (await InjectConfigAsync())
            {
                MessageBoxLibrary.MednafenConfigurationSavedSuccessfully();
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
            _ = _logErrors.LogErrorAsync(ex, "Error in the BtnSave_Click method.");
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
        }
    }
}