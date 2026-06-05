using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectRpcs3ConfigWindow
{
    private readonly InjectRpcs3ConfigViewModel _viewModel;

    public InjectRpcs3ConfigWindow(InjectRpcs3ConfigViewModel viewModel)
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
            Filter = "RPCS3 Executable|rpcs3.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectRPCS3Emulator") ?? "Select RPCS3 Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
