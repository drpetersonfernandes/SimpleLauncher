using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class UpdateLogWindow : Window
{
    private readonly UpdateLogViewModel _viewModel;

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
