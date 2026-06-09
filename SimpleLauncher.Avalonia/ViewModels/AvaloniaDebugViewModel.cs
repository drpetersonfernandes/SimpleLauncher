using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaDebugViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;
    private readonly object _logLock = new();
    private string _logText = string.Empty;

    public AvaloniaDebugViewModel(ILogErrors logErrors, IMessageBoxLibraryService messageBox, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _debugLogger = debugLogger;
    }

    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    public bool CanClearLog => !string.IsNullOrEmpty(LogText);
    public bool CanCopyLog => !string.IsNullOrEmpty(LogText);

    public Func<string, Task>? ClipboardSetText { get; set; }

    public void AppendLogMessage(string message)
    {
        lock (_logLock)
        {
            var timestampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
            LogText += timestampedMessage + Environment.NewLine;
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
            LogText = string.Empty;
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
                if (ClipboardSetText is not null)
                    await ClipboardSetText(LogText);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error copying log");
            await _messageBox.FailedToCopyLogContentMessageBox();
            _debugLogger.Log("Failed to copy log content.");
        }
    }
}
