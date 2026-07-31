using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.ViewModels;

public partial class DebugViewModel : ObservableObject
{
    private readonly object _logLock = new();
    private string _logText = "";

    private const int MaxMessageCount = 5000;

    public DebugViewModel()
    {
        DebugWindowSink.Connect(this);
    }

    public ObservableCollection<string> LogMessages { get; } = [];

    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    public bool CanClearLog => LogMessages.Count > 0;

    public bool CanCopyLog => !string.IsNullOrEmpty(LogText);

    public void AppendLogMessage(string formattedMessage)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.BeginInvoke(() => AppendLogMessage(formattedMessage));
            return;
        }

        lock (_logLock)
        {
            LogMessages.Add(formattedMessage);

            while (LogMessages.Count > MaxMessageCount)
            {
                LogMessages.RemoveAt(0);
            }

            LogText = string.Join(Environment.NewLine, LogMessages) + Environment.NewLine;
            OnPropertyChanged(nameof(CanClearLog));
            OnPropertyChanged(nameof(CanCopyLog));
            ClearLogCommand.NotifyCanExecuteChanged();
            CopyLogCommand.NotifyCanExecuteChanged();
        }
    }

    public void LoadBufferedMessages(IEnumerable<string> formattedMessages)
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => LoadBufferedMessages(formattedMessages));
            return;
        }

        lock (_logLock)
        {
            foreach (var msg in formattedMessages)
            {
                LogMessages.Add(msg);
            }

            while (LogMessages.Count > MaxMessageCount)
            {
                LogMessages.RemoveAt(0);
            }

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
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(ClearLog);
            return;
        }

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
            Log.Error(ex, "Error copying log to clipboard");
        }

        await Task.CompletedTask;
    }
}
