using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class UpdateLogWindow
{
    private readonly UpdateLogViewModel _viewModel;

    public UpdateLogWindow(UpdateLogViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.AppendLog(message);
        });
    }
}