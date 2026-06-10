using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class UpdateLogWindow : Window
{
    private readonly UpdateLogViewModel _viewModel;

    public UpdateLogWindow() : this(App.ServiceProvider.GetRequiredService<UpdateLogViewModel>()) { }

    public UpdateLogWindow(UpdateLogViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    public void Log(string message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _viewModel.AppendLog(message);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
