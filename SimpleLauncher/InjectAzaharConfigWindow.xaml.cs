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

public partial class InjectAzaharConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }
    private string _emulatorPath;
    private readonly ILogErrors _logErrors;

    public InjectAzaharConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
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
        SelectComboByTag(CmbGraphicsApi, _settings.AzaharGraphicsApi.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbResolution, _settings.AzaharResolutionFactor.ToString(CultureInfo.InvariantCulture));
        SelectComboByTag(CmbLayout, _settings.AzaharLayoutOption.ToString(CultureInfo.InvariantCulture));

        ChkFullscreen.IsChecked = _settings.AzaharFullscreen;
        ChkVsync.IsChecked = _settings.AzaharUseVsync;
        ChkAsyncShader.IsChecked = _settings.AzaharAsyncShaderCompilation;
        ChkNew3Ds.IsChecked = _settings.AzaharIsNew3Ds;
        SldVolume.Value = _settings.AzaharVolume;
        ChkShowBeforeLaunch.IsChecked = _settings.AzaharShowSettingsBeforeLaunch;
        ChkAudioStretching.IsChecked = _settings.AzaharEnableAudioStretching;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;
        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    private void SaveSettings()
    {
        _settings.AzaharGraphicsApi = int.Parse(GetSelectedTag(CmbGraphicsApi), CultureInfo.InvariantCulture);
        _settings.AzaharResolutionFactor = int.Parse(GetSelectedTag(CmbResolution), CultureInfo.InvariantCulture);
        _settings.AzaharLayoutOption = int.Parse(GetSelectedTag(CmbLayout), CultureInfo.InvariantCulture);
        _settings.AzaharFullscreen = ChkFullscreen.IsChecked ?? true;
        _settings.AzaharUseVsync = ChkVsync.IsChecked ?? true;
        _settings.AzaharAsyncShaderCompilation = ChkAsyncShader.IsChecked ?? true;
        _settings.AzaharIsNew3Ds = ChkNew3Ds.IsChecked ?? true;
        _settings.AzaharVolume = (int)SldVolume.Value;
        _settings.AzaharShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;
        _settings.AzaharEnableAudioStretching = ChkAudioStretching.IsChecked ?? true;
        _settings.Save();
    }

    private string EnsureEmulatorPath()
    {
        if (!string.IsNullOrEmpty(_emulatorPath) && File.Exists(_emulatorPath))
        {
            return _emulatorPath;
        }

        // Try to resolve from system.xml
        var resolved = EmulatorPathResolver.TryFindEmulatorPath("Azahar");
        if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
        {
            _emulatorPath = resolved;
            return _emulatorPath;
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Azahar Executable|azahar.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectAzaharEmulator") ?? "Select Azahar Emulator"
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
            AzaharConfigurationService.InjectSettings(path, _settings);
            return true;
        }
        catch (AzaharPermissionException)
        {
            // Show permission error - the caller will handle whether to continue or not
            MessageBoxLibrary.AzaharConfigurationInjectionPermissionErrorMessageBox();
            throw;
        }
        catch (InvalidOperationException ex)
        {
            _logErrors.LogErrorAsync(ex, "Azahar injection failed");
            MessageBoxLibrary.FailedToSaveAzaharConfigurationMessageBox();
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
                ShouldRun = true; // Allow game to launch
            }
        }
        catch (AzaharPermissionException)
        {
            // Permission error already shown inside InjectConfig.
            Close();
            ShouldRun = true; // Allow game to launch
        }
        catch (OperationCanceledException)
        {
            // User cancelled - close silently
            Close();
        }
        catch (Exception ex)
        {
            // Unexpected error
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
                MessageBoxLibrary.AzaharConfigurationSavedSuccessfullyMessageBox();
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
            // Unexpected error
            var emulatorName = InjectionErrorHandler.GetEmulatorName(_emulatorPath, GetType());
            InjectionErrorHandler.HandleSaveButtonFailure(_logErrors, ex, emulatorName, _emulatorPath, this);
        }
    }

    private static void SelectComboByTag(ComboBox cmb, string tag)
    {
        foreach (ComboBoxItem item in cmb.Items)
            if (item.Tag?.ToString() == tag)
            {
                cmb.SelectedItem = item;
                return;
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