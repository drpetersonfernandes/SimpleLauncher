using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the DebugWindow.
/// </summary>
public partial class DebugViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;
    private readonly object _logLock = new();
    private string _logText = "";

    public DebugViewModel(ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _debugLogger = debugLogger;
    }

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
            LogText = "";
            OnPropertyChanged(nameof(CanClearLog));
            OnPropertyChanged(nameof(CanCopyLog));
            ClearLogCommand.NotifyCanExecuteChanged();
            CopyLogCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCopyLog))]
    private async Task CopyLogAsync()
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
            _logErrors.LogAndForget(ex, "Error copying log");

            // Notify user
            await _messageBox.FailedToCopyLogContentMessageBox();
            _debugLogger.Log("Failed to copy log content.");
        }
    }
}
