using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Debug log window that displays real-time application log messages.
/// </summary>
public partial class DebugWindow
{
    private static readonly object InstanceLock = new();
    private DebugViewModel _viewModel;
    private PropertyChangedEventHandler _logTextPropertyChangedHandler;
    private bool _isReallyClosing;

    // Private constructor to enforce singleton-like access via DebugLogger
    private DebugWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Prevent the log window from appearing in the taskbar
        ShowInTaskbar = false;

        Closed += LogWindow_Closed;
    }

    // Static instance managed by DebugLogger
    internal static DebugWindow Instance { get; private set; }

    // Method to create and show the window (called by DebugLogger)
    internal static void Initialize()
    {
        lock (InstanceLock)
        {
            if (Instance == null)
            {
                Instance = new DebugWindow
                {
                    _viewModel = App.ServiceProvider.GetRequiredService<DebugViewModel>()
                };

                Instance.DataContext = Instance._viewModel;

                // Set owner to MainWindow so closing this window doesn't affect app lifecycle
                if (Application.Current?.MainWindow is { } mainWindow && mainWindow != Instance)
                {
                    Instance.Owner = mainWindow;
                }

                Instance._logTextPropertyChangedHandler = (_, args) =>
                {
                    if (args.PropertyName == nameof(DebugViewModel.LogText))
                    {
                        try
                        {
                            if (Instance is { IsLoaded: true })
                            {
                                Instance.Dispatcher.BeginInvoke(() => Instance.LogTextBox?.ScrollToEnd());
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            // Window is closing, ignore
                        }
                        catch (InvalidOperationException)
                        {
                            // Dispatcher is shutting down, ignore
                        }
                    }
                };

                Instance._viewModel.PropertyChanged += Instance._logTextPropertyChangedHandler;

                Instance.Show();
            }
            else
            {
                // If already initialized, just ensure it's visible and brought to the front
                Instance.Show();
                Instance.WindowState = WindowState.Normal;
                Instance.Activate();
            }
        }
    }

    // Method to append a message from potentially any thread
    internal void AppendLogMessage(string message)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel.AppendLogMessage(message);
            });
        }
        catch (ObjectDisposedException)
        {
            // Window is closing, ignore
        }
        catch (InvalidOperationException)
        {
            // Dispatcher is shutting down, ignore
        }
    }

    // Method to load pre-formatted buffered messages (preserves original timestamps)
    internal void LoadBufferedMessages(IEnumerable<string> formattedMessages)
    {
        try
        {
            Dispatcher.Invoke(() =>
            {
                _viewModel.LoadBufferedMessages(formattedMessages);
            });
        }
        catch (ObjectDisposedException)
        {
            // Window is closing, ignore
        }
        catch (InvalidOperationException)
        {
            // Dispatcher is shutting down, ignore
        }
    }

    /// <summary>
    /// Allow the window to actually close only during app shutdown.
    /// Otherwise, hide instead to keep the instance alive.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (_isReallyClosing)
        {
            base.OnClosing(e);
            return;
        }

        // Cancel the close and hide instead
        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }

    /// <summary>
    /// Called by the app during shutdown to allow the window to actually close.
    /// </summary>
    internal void AllowClose()
    {
        _isReallyClosing = true;
    }

    private static void LogWindow_Closed(object sender, EventArgs e)
    {
        try
        {
            lock (InstanceLock)
            {
                if (Instance is { _logTextPropertyChangedHandler: not null, _viewModel: not null })
                {
                    Instance._viewModel.PropertyChanged -= Instance._logTextPropertyChangedHandler;
                    Instance._logTextPropertyChangedHandler = null;
                }

                Instance = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in LogWindow_Closed: {ex}");
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
