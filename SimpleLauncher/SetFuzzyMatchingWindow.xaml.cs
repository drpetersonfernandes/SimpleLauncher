using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow
{
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
