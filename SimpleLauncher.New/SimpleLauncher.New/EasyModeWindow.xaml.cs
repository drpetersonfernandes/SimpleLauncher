using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Navigation;
using Microsoft.Win32;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New;

/// <summary>
/// OpenEmu-themed window for the EasyMode "Add System" workflow.
/// Uses MVVM via EasyModeViewModel for all logic; code-behind handles only
/// window lifecycle and view-specific interactions (folder browser, hyperlinks, overlay).
/// </summary>
public partial class EasyModeWindow : IDisposable
{
    private readonly EasyModeViewModel _viewModel;
    private readonly PropertyChangedEventHandler _onViewModelPropertyChanged;
    private bool _disposed;

    public EasyModeWindow(EasyModeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = _viewModel = viewModel;

        // Apply dark title bar via DWM
        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useDarkMode = 1;
            const int darkModeAttributeWin11 = 20;
            const int darkModeAttributeWin10 = 19;
            var result = DwmSetWindowAttribute(hwnd, darkModeAttributeWin11, ref useDarkMode, sizeof(int));
            if (result != 0)
            {
                _ = DwmSetWindowAttribute(hwnd, darkModeAttributeWin10, ref useDarkMode, sizeof(int));
            }
        };

        // Set up the close callback so the ViewModel can request window close after successful add
        _viewModel.RequestClose = () => Dispatcher.InvokeAsync(() => Close());

        // Subscribe to IsLoading for overlay visibility.
        // Handler stored in a field so it can be unsubscribed in Dispose().
        _onViewModelPropertyChanged = (_, args) =>
        {
            if (args.PropertyName == nameof(EasyModeViewModel.IsLoading))
            {
                LoadingOverlay.Visibility = _viewModel.IsLoading
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        };
        _viewModel.PropertyChanged += _onViewModelPropertyChanged;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private async void Window_Loaded(object sender, RoutedEventArgs e)
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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog
        {
            Title = "Choose a folder with ROMs or ISOs for this system"
        };

        if (dialog.ShowDialog() == true)
        {
            _viewModel.SystemFolderPath = dialog.FolderName;
        }
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "EasyModeWindow failed to open link {Uri}", e.Uri);
        }
    }

    private void EmergencyOverlay_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.StopDownloadCommand.Execute(null);
        LoadingOverlay.Visibility = Visibility.Collapsed;
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
