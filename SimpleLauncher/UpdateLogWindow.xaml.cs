using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window displaying real-time update installation log messages.
/// </summary>
public partial class UpdateLogWindow
{
    private readonly UpdateLogViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateLogWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing update log logic.</param>
    public UpdateLogWindow(UpdateLogViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    /// <summary>
    /// Appends a log message to the update log display.
    /// </summary>
    /// <param name="message">The message to append.</param>
    public void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            _viewModel.AppendLog(message);
        });
    }
}
