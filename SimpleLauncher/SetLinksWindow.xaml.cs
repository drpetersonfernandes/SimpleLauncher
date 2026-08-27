using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring external application links and paths.
/// </summary>
public partial class SetLinksWindow
{
    private readonly EventHandler _saveCompletedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetLinksWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing link configuration logic.</param>
    public SetLinksWindow(SetLinksViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _saveCompletedHandler = (_, _) =>
        {
            if (IsLoaded)
            {
                DialogResult = true;
            }

            Close();
        };

        viewModel.SaveCompleted += _saveCompletedHandler;
        viewModel.CloseRequested += OnCloseRequested;

        Closing += (_, _) =>
        {
            viewModel.SaveCompleted -= _saveCompletedHandler;
            viewModel.CloseRequested -= OnCloseRequested;
        };

        DataContext = viewModel;
    }

    private void OnCloseRequested(object? sender, EventArgs e)
    {
        Close();
    }
}