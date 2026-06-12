using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring fuzzy matching settings for game file searches.
/// </summary>
public partial class SetFuzzyMatchingWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SetFuzzyMatchingWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing fuzzy matching configuration logic.</param>
    public SetFuzzyMatchingWindow(SetFuzzyMatchingViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        viewModel.SaveCompleted += () =>
        {
            DialogResult = true;
            Close();
        };
        viewModel.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };

        DataContext = viewModel;
    }

    // No explicit CancelButton_Click needed because IsCancel="True" handles it
}
