using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class DebugWindow : Window
{
    private readonly AvaloniaDebugViewModel _viewModel;

    public DebugWindow() : this(App.ServiceProvider.GetRequiredService<AvaloniaDebugViewModel>()) { }

    public DebugWindow(AvaloniaDebugViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _viewModel.ClipboardSetText = SetClipboardTextAsync;
        DataContext = _viewModel;

        ShowInTaskbar = false;
    }

    public void AppendLogMessage(string message)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            _viewModel.AppendLogMessage(message);
        });
    }

    private Task SetClipboardTextAsync(string text)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel?.Clipboard != null)
        {
            var dataTransfer = new DataTransfer();
            var item = new DataTransferItem();
            item.SetText(text);
            dataTransfer.Add(item);
            return topLevel.Clipboard.SetDataAsync(dataTransfer);
        }

        return Task.CompletedTask;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
