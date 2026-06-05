using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class AboutWindow
{
    public AboutWindow(AboutViewModel viewModel)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        viewModel.CloseRequested += Close;
        viewModel.OpenUpdateHistoryRequested += static () =>
        {
            var updateHistoryWindow = App.ServiceProvider.GetRequiredService<UpdateHistoryWindow>();
            updateHistoryWindow.ShowDialog();
        };
        viewModel.GetOwnerWindow += () => this;

        DataContext = viewModel;
    }
}
