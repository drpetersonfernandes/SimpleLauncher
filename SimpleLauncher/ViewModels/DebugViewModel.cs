using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the DebugWindow.
/// </summary>
public partial class DebugViewModel : ObservableObject
{
    private readonly object _logLock = new();
    private string _logText = string.Empty;

    /// <summary>
    /// Gets the collection of log messages.
    /// </summary>
    public ObservableCollection<string> LogMessages { get; } = [];

    /// <summary>
    /// Gets the combined log text for display.
    /// </summary>
    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    /// <summary>
    /// Gets whether the log can be cleared.
    /// </summary>
    public bool CanClearLog => LogMessages.Count > 0;

    /// <summary>
    /// Gets whether the log can be copied.
    /// </summary>
    public bool CanCopyLog => !string.IsNullOrEmpty(LogText);

    /// <summary>
    /// Appends a message to the log.
    /// </summary>
    public void AppendLogMessage(string message)
    {
        lock (_logLock)
        {
            var timestampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            LogMessages.Add(timestampedMessage);
            LogText = string.Join(Environment.NewLine, LogMessages) + Environment.NewLine;
            OnPropertyChanged(nameof(CanClearLog));
            OnPropertyChanged(nameof(CanCopyLog));
            ClearLogCommand.NotifyCanExecuteChanged();
            CopyLogCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanClearLog))]
    private void ClearLog()
    {
        lock (_logLock)
        {
            LogMessages.Clear();
            LogText = string.Empty;
            OnPropertyChanged(nameof(CanClearLog));
            OnPropertyChanged(nameof(CanCopyLog));
            ClearLogCommand.NotifyCanExecuteChanged();
            CopyLogCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopyLog))]
    private void CopyLog()
    {
        try
        {
            if (!string.IsNullOrEmpty(LogText))
            {
                System.Windows.Clipboard.SetText(LogText);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            App.LogErrorAsync(ex, "Error copying log");

            // Notify user
            MessageBoxLibrary.FailedToCopyLogContentMessageBox();
            DebugLogger.Log("Failed to copy log content.");
        }
    }
}
