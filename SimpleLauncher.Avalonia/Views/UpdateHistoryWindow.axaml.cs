using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class UpdateHistoryWindow : Window
{
    public UpdateHistoryWindow() : this(App.ServiceProvider.GetRequiredService<UpdateHistoryViewModel>()) { }

    public UpdateHistoryWindow(UpdateHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
