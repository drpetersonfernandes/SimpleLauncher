using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectMednafenConfigWindow
{
    private readonly InjectMednafenConfigViewModel _viewModel;

    public InjectMednafenConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectMednafenConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "Mednafen Executable|mednafen.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectMednafenEmulator") ?? "Select Mednafen Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
