using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

/// <summary>
/// Window that displays real-time debug log output.
/// </summary>
public partial class DebugWindow
{
    private static readonly Lock InstanceLock = new();
    private DebugViewModel _viewModel = null!;
    private PropertyChangedEventHandler? _logTextPropertyChangedHandler;
    private bool _isReallyClosing;

    /// <summary>
    /// Gets the current singleton instance of the debug window, or <c>null</c> when it has not been created.
    /// </summary>
    internal static DebugWindow? Instance { get; private set; }

    /// <summary>
    /// Creates and shows the singleton debug window, wiring it to the <see cref="DebugViewModel"/> and auto-scrolling
    /// the log text box whenever new output arrives. If the window already exists it is restored and activated instead.
    /// </summary>
    internal static void Initialize()
    {
        lock (InstanceLock)
        {
            if (Instance != null)
            {
                Instance.Show();
                Instance.WindowState = WindowState.Normal;
                Instance.Activate();
                return;
            }

            Instance = new DebugWindow
            {
                _viewModel = App.ServiceProvider.GetRequiredService<DebugViewModel>()
            };

            Instance.DataContext = Instance._viewModel;

            if (Application.Current?.MainWindow is { } mainWindow && mainWindow != Instance)
            {
                Instance.Owner = mainWindow;
            }

            Instance._logTextPropertyChangedHandler = (_, args) =>
            {
                if (string.Equals(args.PropertyName, nameof(DebugViewModel.LogText), StringComparison.Ordinal))
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
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            };

            Instance._viewModel.PropertyChanged += Instance._logTextPropertyChangedHandler;

            Instance.Show();
        }
    }

    /// <summary>
    /// Shows the debug window, creating it if necessary, or brings it to the foreground if already open.
    /// </summary>
    public static void ShowDebugWindow()
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(ShowDebugWindow);
            return;
        }

        Initialize();
    }

    /// <summary>
    /// Detaches the view model event handler, disconnects the debug log sink, closes the singleton debug window
    /// and clears the cached instance.
    /// </summary>
    internal static void ShutdownWindow()
    {
        lock (InstanceLock)
        {
            switch (Instance)
            {
                case null:
                    return;
                case { _logTextPropertyChangedHandler: not null }:
                    Instance._viewModel.PropertyChanged -= Instance._logTextPropertyChangedHandler;
                    Instance._logTextPropertyChangedHandler = null;
                    break;
            }

            DebugWindowSink.Disconnect();
            Instance._isReallyClosing = true;
            Instance.Close();
            Instance = null;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (_isReallyClosing)
        {
            base.OnClosing(e);
            return;
        }

        e.Cancel = true;
        Hide();
        base.OnClosing(e);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Hide();
    }
}
