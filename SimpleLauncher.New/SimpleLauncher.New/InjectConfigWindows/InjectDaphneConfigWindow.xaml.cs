using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New.InjectConfigWindows;

/// <summary>
/// Window for injecting Daphne emulator configuration settings.
/// </summary>
public partial class InjectDaphneConfigWindow
{
    private readonly InjectDaphneConfigViewModel _viewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="InjectDaphneConfigWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing configuration logic.</param>
    public InjectDaphneConfigWindow(InjectDaphneConfigViewModel viewModel)
    {
        InitializeComponent();


        _viewModel = viewModel;
        _viewModel.CloseRequested += OnCloseRequested;

        Closing += (_, _) =>
        {
            _viewModel.CloseRequested -= OnCloseRequested;
        };

        DataContext = _viewModel;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Initializes the window with the specified launcher mode.
    /// </summary>
    /// <param name="isLauncherMode">If true, the window operates in launcher mode.</param>
    public void Initialize(bool isLauncherMode = true)
    {
        _viewModel.Initialize(isLauncherMode);

        if (!isLauncherMode)
        {
            BtnSave.IsDefault = true;
        }
    }

    /// <summary>
    /// Gets whether the emulator should be launched after configuration.
    /// </summary>
    public bool ShouldRun => _viewModel.ShouldRun;
}
