using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class LogWindow
{
    // Private constructor to enforce singleton-like access via DebugLogger
    private LogWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Prevent the log window from appearing in the taskbar
        ShowInTaskbar = false;

        Closed += LogWindow_Closed;
    }

    // Static instance managed by DebugLogger
    internal static LogWindow Instance { get; private set; }

    // Method to create and show the window (called by DebugLogger)
    internal static void Initialize()
    {
        if (Instance == null)
        {
            Instance = new LogWindow();
            Instance.Show();
        }
        else
        {
            // If already initialized, just ensure it's visible and brought to front
            Instance.Show();
            Instance.Activate();
        }
    }

    // Method to append a message from potentially any thread
    internal void AppendLogMessage(string message)
    {
        // Use Dispatcher to ensure UI update happens on the UI thread
        Dispatcher.Invoke(() =>
        {
            // Add timestamp and append to the TextBox
            LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff} - {message}{Environment.NewLine}");
            // Auto-scroll to the bottom
            LogTextBox.ScrollToEnd();
        });
    }

    private static void LogWindow_Closed(object sender, EventArgs e)
    {
        Instance = null;
    }


    // Button click handler to clear the log
    private void ClearLogButton_Click(object sender, RoutedEventArgs e)
    {
        LogTextBox.Clear();
    }

    // Button click handler to copy the log content
    private void CopyLogButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(LogTextBox.Text))
            {
                Clipboard.SetText(LogTextBox.Text);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error copying log");

            // Notify user
            MessageBoxLibrary.FailedToCopyLogContent();
            DebugLogger.Log("Failed to copy log content.");
        }
    }
}