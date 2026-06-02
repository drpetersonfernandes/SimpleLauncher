using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectDolphinConfigWindow
{
    private readonly InjectDolphinConfigViewModel _viewModel;

    public InjectDolphinConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectDolphinConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "Dolphin Executable|Dolphin.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectDolphinEmulator") ?? "Select Dolphin Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
