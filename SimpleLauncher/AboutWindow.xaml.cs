using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window displaying application information, version, and credits.
/// </summary>
public partial class AboutWindow
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AboutWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing about-window logic.</param>
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
