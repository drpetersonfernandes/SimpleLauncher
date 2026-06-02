using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectSupermodelConfigWindow
{
    private readonly InjectSupermodelConfigViewModel _viewModel;

    public InjectSupermodelConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectSupermodelConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "Supermodel Executable|Supermodel.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectSupermodelEmulator") ?? "Select Supermodel Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
