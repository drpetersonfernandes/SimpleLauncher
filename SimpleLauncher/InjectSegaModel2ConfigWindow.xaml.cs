using System.Windows;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectSegaModel2ConfigWindow
{
    private readonly InjectSegaModel2ConfigViewModel _viewModel;

    public InjectSegaModel2ConfigWindow(SettingsManager settings, string emulatorPath = null, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectSegaModel2ConfigViewModel(settings, emulatorPath, isLauncherMode);
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
            Filter = "SEGA Model 2 Executable|emulator.exe|All Executables|*.exe",
            Title = (string)Application.Current.TryFindResource("SelectSEGAModel2Emulator") ?? "Select SEGA Model 2 Emulator"
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
