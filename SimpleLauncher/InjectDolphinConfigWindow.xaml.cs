using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectDolphinConfigWindow
{
    private readonly InjectDolphinConfigViewModel _viewModel;

    public InjectDolphinConfigWindow(InjectDolphinConfigViewModel viewModel)
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
            Filter = "Dolphin Executable|Dolphin.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectDolphinEmulator") ?? "Select Dolphin Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
