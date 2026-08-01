using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class DebugWindow
{
    private static readonly Lock InstanceLock = new();
    private DebugViewModel _viewModel = null!;
    private PropertyChangedEventHandler? _logTextPropertyChangedHandler;
    private bool _isReallyClosing;

    internal static DebugWindow? Instance { get; private set; }

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
