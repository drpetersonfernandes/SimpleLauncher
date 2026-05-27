using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class SetFuzzyMatchingWindow
{
    public SetFuzzyMatchingWindow(SettingsManager settings)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        var viewModel = new SetFuzzyMatchingViewModel(settings, logErrors);
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
