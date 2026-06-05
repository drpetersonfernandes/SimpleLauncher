using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectFlycastConfigWindow
{
    private readonly InjectFlycastConfigViewModel _viewModel;

    public InjectFlycastConfigWindow(InjectFlycastConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;
        _viewModel.RequestEmulatorPath += OnRequestEmulatorPath;
        _viewModel.GetOwnerWindow += () => this;

        DataContext = _viewModel;
    }

    public void Initialize(string emulatorPath = null, bool isLauncherMode = true)
    {
        _viewModel.Initialize(emulatorPath, isLauncherMode);

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
            Filter = "Flycast Executable|flycast.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectFlycastEmulator") ?? "Select Flycast Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
