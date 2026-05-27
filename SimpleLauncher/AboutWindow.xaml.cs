using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        var logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();
        var viewModel = new AboutViewModel(logErrors);
        viewModel.CloseRequested += Close;
        viewModel.OpenUpdateHistoryRequested += static () =>
        {
            var updateHistoryWindow = new UpdateHistoryWindow();
            updateHistoryWindow.ShowDialog();
        };
        viewModel.GetOwnerWindow += () => this;

        DataContext = viewModel;
    }
}
