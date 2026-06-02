using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectPcsx2ConfigWindow
{
    private readonly InjectPcsx2ConfigViewModel _viewModel;

    public InjectPcsx2ConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectPcsx2ConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "PCSX2 Executable|pcsx2*.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectPCSX2Emulator") ?? "Select PCSX2 Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
