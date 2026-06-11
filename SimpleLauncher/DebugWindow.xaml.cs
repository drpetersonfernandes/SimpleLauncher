using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher;

public partial class DebugWindow
{
    private static readonly object InstanceLock = new();
    private DebugViewModel _viewModel;

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

                Instance.Show();
            }
            else
            {
                // If already initialized, just ensure it's visible and brought to the front
                Instance.Show();
                Instance.Activate();
            }
        }
    }

    // Method to append a message from potentially any thread
    internal void AppendLogMessage(string message)
    {
        // Use Dispatcher to ensure UI update happens on the UI thread
        Dispatcher.Invoke(() =>
        {
            _viewModel.AppendLogMessage(message);
        });
    }

    private static void LogWindow_Closed(object sender, EventArgs e)
    {
        lock (InstanceLock)
        {
            Instance = null;
        }
    }
}
