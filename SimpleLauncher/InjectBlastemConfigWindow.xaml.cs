using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectBlastemConfigWindow
{
    private readonly InjectBlastemConfigViewModel _viewModel;

    public InjectBlastemConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectBlastemConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "Blastem Executable|blastem.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectBlastemEmulator") ?? "Select Blastem Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
