using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectRaineConfigWindow
{
    private readonly InjectRaineConfigViewModel _viewModel;

    public InjectRaineConfigWindow(SettingsManager settings, string emulatorPath = null, string gameFilePath = null, string systemRomPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectRaineConfigViewModel(settings, emulatorPath, isLauncherMode, gameFilePath, systemRomPath);
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
        _viewModel.RequestFilePath += OnRequestFilePath;
        _viewModel.RequestFolderPath += OnRequestFolderPath;
        _viewModel.GetOwnerWindow += () => this;

        DataContext = _viewModel;

        if (!isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    public bool ShouldRun => _viewModel.ShouldRun;

    private static string OnRequestEmulatorPath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Raine Executable|raine*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectExeTitle") ?? "Select Raine Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private static string OnRequestFilePath()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "NeoGeo CD BIOS (neocd.bin)|neocd.bin|All Files (*.*)|*.*",
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectNeoCdBios") ?? "Select NeoGeo CD BIOS File"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    private static string OnRequestFolderPath()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = (string)Application.Current.TryFindResource("RaineConfig_SelectRomDirectory") ?? "Select Raine ROM Directory"
        };

        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    private void BtnSelectNeoCdBios_Click(object sender, RoutedEventArgs e)
    {
        var path = OnRequestFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            _viewModel.RaineNeoCdBios = path;
        }
    }

    private void BtnSelectRaineRomDirectory_Click(object sender, RoutedEventArgs e)
    {
        var path = OnRequestFolderPath();
        if (!string.IsNullOrEmpty(path))
        {
            _viewModel.RaineRomDirectory = path;
        }
    }
}
