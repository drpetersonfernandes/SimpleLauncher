using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring external application links and paths.
/// </summary>
public partial class SetLinksWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetLinksWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing link configuration logic.</param>
    public SetLinksWindow(SetLinksViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        viewModel.SaveCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.CloseRequested += Close;

        DataContext = viewModel;
    }
}
