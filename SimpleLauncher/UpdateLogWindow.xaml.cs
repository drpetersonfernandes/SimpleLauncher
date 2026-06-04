using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class UpdateLogWindow
{
    private readonly UpdateLogViewModel _viewModel;

    public UpdateLogWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = new UpdateLogViewModel();
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