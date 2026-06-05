using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectDuckStationConfigWindow
{
    private readonly InjectDuckStationConfigViewModel _viewModel;

    public InjectDuckStationConfigWindow(InjectDuckStationConfigViewModel viewModel)
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
            Filter = "DuckStation Executable|duckstation*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectDuckStationEmulator") ?? "Select DuckStation Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
