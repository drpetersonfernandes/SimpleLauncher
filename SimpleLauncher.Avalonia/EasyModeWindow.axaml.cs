using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// OpenEmu-themed window for the EasyMode "Add System" workflow.
/// Uses MVVM via EasyModeViewModel for all logic; code-behind handles only
/// window lifecycle and view-specific interactions (folder browser, overlay).
/// </summary>
public partial class EasyModeWindow : Window, IDisposable
{
    private readonly EasyModeViewModel _viewModel;
    private readonly PropertyChangedEventHandler _onViewModelPropertyChanged;
    private bool _disposed;

    public EasyModeWindow(EasyModeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        // Set up the close callback so the ViewModel can request window close after successful add
        _viewModel.RequestClose = () => Dispatcher.UIThread.InvokeAsync(() => Close());

        // Subscribe to IsLoading for overlay visibility.
        // Handler stored in a field so it can be unsubscribed in Dispose().
        _onViewModelPropertyChanged = (_, args) =>
        {
            if (args.PropertyName == nameof(EasyModeViewModel.IsLoading))
            {
                LoadingOverlay.IsVisible = _viewModel.IsLoading;
            }
        };
        _viewModel.PropertyChanged += _onViewModelPropertyChanged;
    }

    private async void Window_Opened(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "EasyModeWindow load failed");
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        Dispose();
    }

    private void CloseButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BrowseFolder_Click(object? sender, RoutedEventArgs e)
    {
        var filePicker = App.ServiceProvider.GetRequiredService<IFilePickerService>();
        var folder = await filePicker.OpenFolderAsync("Choose a folder with ROMs or ISOs for this system");
        if (!string.IsNullOrEmpty(folder))
        {
            _viewModel.SystemFolderPath = folder;
        }
    }

    private void EmergencyOverlay_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.StopDownloadCommand.Execute(null);
        LoadingOverlay.IsVisible = false;
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Unsubscribe so the ViewModel no longer holds a reference to this window
        _viewModel.PropertyChanged -= _onViewModelPropertyChanged;
        _viewModel.RequestClose = null;
        _viewModel?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
