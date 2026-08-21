using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
/// ViewModel for the debug window, managing log message collection and display.
/// Avalonia port of the WPF DebugViewModel.
/// </summary>
public partial class DebugViewModel : ObservableObject
{
    private readonly Lock _logLock = new();
    private string _logText = "";

    private const int MaxMessageCount = 5000;

    /// <summary>Initializes a new instance of the <see cref="DebugViewModel"/> and connects to the debug window sink.</summary>
    public DebugViewModel()
    {
        DebugWindowSink.Connect(this);
    }

    /// <summary>Gets the collection of formatted log messages.</summary>
    public ObservableCollection<string> LogMessages { get; } = [];

    /// <summary>Gets the full log text for display or clipboard operations.</summary>
    public string LogText
    {
        get => _logText;
        private set => SetProperty(ref _logText, value);
    }

    /// <summary>Gets whether there are log messages that can be cleared.</summary>
    public bool CanClearLog => LogMessages.Count > 0;

    /// <summary>Gets whether there is log text that can be copied to the clipboard.</summary>
    public bool CanCopyLog => !string.IsNullOrEmpty(LogText);

    /// <summary>Appends a formatted log message to the log collection, evicting old entries if the limit is exceeded.</summary>
    /// <param name="formattedMessage">The formatted log message to append.</param>
    public void AppendLogMessage(string formattedMessage)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => AppendLogMessage(formattedMessage));
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

    /// <summary>Loads a batch of pre-formatted log messages into the log collection.</summary>
    /// <param name="formattedMessages">The collection of formatted messages to load.</param>
    public void LoadBufferedMessages(IEnumerable<string> formattedMessages)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(() => LoadBufferedMessages(formattedMessages));
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
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.InvokeAsync(ClearLog);
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
            if (!string.IsNullOrEmpty(LogText) &&
                Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime &&
                TopLevel.GetTopLevel(lifetime.MainWindow)?.Clipboard is { } clipboard)
            {
                var dataTransfer = new DataTransfer();
                dataTransfer.Add(DataTransferItem.CreateText(LogText));
                await clipboard.SetDataAsync(dataTransfer);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error copying log to clipboard");
        }
    }
}