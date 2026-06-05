using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class InjectDaphneConfigWindow
{
    private readonly InjectDaphneConfigViewModel _viewModel;

    public InjectDaphneConfigWindow(InjectDaphneConfigViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        _viewModel.CloseRequested += Close;

        DataContext = _viewModel;
    }

    public void Initialize(bool isLauncherMode = true)
    {
        _viewModel.Initialize(isLauncherMode);

        if (!isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    public bool ShouldRun => _viewModel.ShouldRun;
}
