using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using SimpleLauncher.Managers;

namespace SimpleLauncher;

public partial class InjectXeniaConfigWindow
{
    private readonly SettingsManager _settings;
    private readonly bool _isLauncherMode;
    public bool ShouldRun { get; private set; }

    public InjectXeniaConfigWindow(SettingsManager settings, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        _settings = settings;
        _isLauncherMode = isLauncherMode;
        LoadSettings();
    }

    private void LoadSettings()
    {
        // GPU
        CmbGpu.Text = _settings.XeniaGpu;
        ChkVsync.IsChecked = _settings.XeniaVsync;
        ChkFullscreen.IsChecked = _settings.XeniaFullscreen;
        CmbResX.Text = _settings.XeniaResScaleX.ToString(CultureInfo.InvariantCulture);
        CmbResY.Text = _settings.XeniaResScaleY.ToString(CultureInfo.InvariantCulture);

        SelectComboByTag(CmbAa, _settings.XeniaAa);
        SelectComboByTag(CmbScaling, _settings.XeniaScaling);
        SelectComboByTag(CmbReadback, _settings.XeniaReadbackResolve);
        ChkGammaSrgb.IsChecked = _settings.XeniaGammaSrgb;

        // APU
        CmbApu.Text = _settings.XeniaApu;
        ChkMute.IsChecked = _settings.XeniaMute;

        // System
        ChkMountCache.IsChecked = _settings.XeniaMountCache;
        ChkVibration.IsChecked = _settings.XeniaVibration;

        // System
        ChkPatches.IsChecked = _settings.XeniaApplyPatches;
        CmbHid.Text = _settings.XeniaHid;
        ChkShowBeforeLaunch.IsChecked = _settings.XeniaShowSettingsBeforeLaunch;

        BtnRun.Visibility = _isLauncherMode ? Visibility.Visible : Visibility.Collapsed;

        if (!_isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }

        SelectComboByTag(CmbLang, _settings.XeniaUserLanguage.ToString(CultureInfo.InvariantCulture));
    }

    private void SaveSettings()
    {
        // GPU
        _settings.XeniaGpu = CmbGpu.Text;
        _settings.XeniaVsync = ChkVsync.IsChecked ?? true;
        _settings.XeniaFullscreen = ChkFullscreen.IsChecked ?? false;

        if (int.TryParse(CmbResX.Text, out var resX))
        {
            _settings.XeniaResScaleX = resX;
        }

        if (int.TryParse(CmbResY.Text, out var resY))
        {
            _settings.XeniaResScaleY = resY;
        }

        _settings.XeniaAa = GetSelectedTag(CmbAa);
        _settings.XeniaScaling = GetSelectedTag(CmbScaling);
        _settings.XeniaReadbackResolve = GetSelectedTag(CmbReadback);
        _settings.XeniaGammaSrgb = ChkGammaSrgb.IsChecked ?? false;

        // APU
        _settings.XeniaApu = CmbApu.Text;
        _settings.XeniaMute = ChkMute.IsChecked ?? false;

        // System
        _settings.XeniaMountCache = ChkMountCache.IsChecked ?? true;
        _settings.XeniaVibration = ChkVibration.IsChecked ?? true;

        // System
        _settings.XeniaApplyPatches = ChkPatches.IsChecked ?? true;
        _settings.XeniaHid = CmbHid.Text;
        _settings.XeniaShowSettingsBeforeLaunch = ChkShowBeforeLaunch.IsChecked ?? true;

        if (int.TryParse(GetSelectedTag(CmbLang), out var lang))
        {
            _settings.XeniaUserLanguage = lang;
        }

        _settings.Save();
    }

    private void BtnRun_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        ShouldRun = true;
        Close();
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        Close();
    }

    private static void SelectComboByTag(ComboBox cmb, string tagValue)
    {
        foreach (ComboBoxItem item in cmb.Items)
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
        if (cmb.SelectedItem is ComboBoxItem item)
        {
            return item.Tag?.ToString() ?? "";
        }

        return "";
    }
}