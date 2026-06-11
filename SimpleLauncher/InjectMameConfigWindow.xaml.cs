using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectMameConfigWindow
{
    private readonly InjectMameConfigViewModel _viewModel;

    public InjectMameConfigWindow(InjectMameConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
        _viewModel.GetOwnerWindow += () => this;

        DataContext = _viewModel;
    }

    public void Initialize(string emulatorPath = null, bool isLauncherMode = true, string systemRomPath = null, string[] listOfSecondaryRomPaths = null)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode, systemRomPath, listOfSecondaryRomPaths);

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
