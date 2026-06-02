using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectDaphneConfigWindow
{
    private readonly InjectDaphneConfigViewModel _viewModel;

    public InjectDaphneConfigWindow(SettingsManager settings, bool isLauncherMode = true)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new InjectDaphneConfigViewModel(settings, isLauncherMode);
        _viewModel.CloseRequested += Close;

        DataContext = _viewModel;

        if (!isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    public bool ShouldRun => _viewModel.ShouldRun;
}
