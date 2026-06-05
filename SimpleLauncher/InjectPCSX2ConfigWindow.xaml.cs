using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectPcsx2ConfigWindow
{
    private readonly InjectPcsx2ConfigViewModel _viewModel;

    public InjectPcsx2ConfigWindow(InjectPcsx2ConfigViewModel viewModel)
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
            Filter = "PCSX2 Executable|pcsx2*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectPCSX2Emulator") ?? "Select PCSX2 Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
