using System.Windows;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
///     Window for configuring fuzzy matching settings for game file searches.
/// </summary>
public partial class SetFuzzyMatchingWindow
{
    private readonly EventHandler _cancelRequestedHandler;
    private readonly EventHandler _saveCompletedHandler;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SetFuzzyMatchingWindow" /> class.
    /// </summary>
    /// <param name="viewModel">The view model providing fuzzy matching configuration logic.</param>
    public SetFuzzyMatchingWindow(SetFuzzyMatchingViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _saveCompletedHandler = (_, _) =>
        {
            if (IsLoaded) DialogResult = true;

            Close();
        };
        _cancelRequestedHandler = (_, _) =>
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