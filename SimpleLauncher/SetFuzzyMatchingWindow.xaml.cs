using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow
{
    public SetFuzzyMatchingWindow(SettingsManager settings)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var viewModel = new SetFuzzyMatchingViewModel(settings);
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
