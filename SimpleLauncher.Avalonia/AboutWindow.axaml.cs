using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Window displaying application information, version, and credits.
/// </summary>
public partial class AboutWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AboutWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The view model providing about-window logic.</param>
    public AboutWindow(AboutViewModel viewModel)
    {
        InitializeComponent();

        viewModel.CloseRequested += (_, _) => Close();
        viewModel.OpenUpdateHistoryRequested += (_, _) =>
        {
            var updateHistoryWindow = App.ServiceProvider.GetRequiredService<UpdateHistoryWindow>();
            updateHistoryWindow.ShowDialog(this);
        };
        viewModel.GetOwnerWindow = () => this;

        DataContext = viewModel;
    }
}