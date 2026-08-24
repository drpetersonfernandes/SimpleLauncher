using Avalonia.Controls;
using SimpleLauncher.Avalonia.Services.LoadingOverlay;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Partial MainWindow implementing <see cref="IAvaloniaLoadingOverlayHost"/> for loading
/// overlay coordination (WPF MainWindow.LoadingOverlayHost.cs parity).
/// </summary>
public partial class MainWindow : IAvaloniaLoadingOverlayHost
{
    void IAvaloniaLoadingOverlayHost.SetIsLoading(bool isLoading)
    {
        LoadingOverlay.IsVisible = isLoading;
    }

    void IAvaloniaLoadingOverlayHost.SetLoadingMessage(string message)
    {
        LoadingMessage.Text = message;
    }

    Task IAvaloniaLoadingOverlayHost.ResetUiAsync()
    {
        return _uiResetService.ResetUiAsync();
    }

    void IAvaloniaLoadingOverlayHost.CancelAndRecreateToken()
    {
        _uiResetCancellationSource.Cancel();
        _uiResetCancellationSource.Dispose();
        _uiResetCancellationSource = new CancellationTokenSource();
    }

    void IAvaloniaLoadingOverlayHost.SetMainContentGridEnabled(bool enabled)
    {
        MainContentGrid.IsEnabled = enabled;
    }
}
