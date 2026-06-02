using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectMameConfigWindow
{
    private readonly InjectMameConfigViewModel _viewModel;

    public InjectMameConfigWindow(SettingsManager settings, string emulatorPath = null, string systemRomPath = null, bool isLauncherMode = true, string[] listOfSecondaryRomPaths = null)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectMameConfigViewModel(settings, emulatorPath, isLauncherMode, systemRomPath, listOfSecondaryRomPaths);
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
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
            Filter = "MAME Executable|mame*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMAMEEmulator") ?? "Select MAME Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
