using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window for configuring fuzzy matching settings for game file searches.
/// </summary>
public partial class SetFuzzyMatchingWindow
{
    private readonly Action _saveCompletedHandler;
    private readonly Action _cancelRequestedHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="SetFuzzyMatchingWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing fuzzy matching configuration logic.</param>
    public SetFuzzyMatchingWindow(SetFuzzyMatchingViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _saveCompletedHandler = () =>
        {
            if (IsLoaded) DialogResult = true;
            Close();
        };
        _cancelRequestedHandler = () =>
        {
            if (IsLoaded) DialogResult = false;
            Close();
        };

        viewModel.SaveCompleted += _saveCompletedHandler;
        viewModel.CancelRequested += _cancelRequestedHandler;

        Closing += (_, _) =>
        {
            viewModel.SaveCompleted -= _saveCompletedHandler;
            viewModel.CancelRequested -= _cancelRequestedHandler;
        };

        Closed += (_, _) => { DialogResult ??= false; };

        DataContext = viewModel;
    }

    // No explicit CancelButton_Click needed because IsCancel="True" handles it
}
