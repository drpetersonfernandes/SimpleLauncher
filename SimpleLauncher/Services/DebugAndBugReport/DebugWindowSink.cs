using Serilog.Core;
using Serilog.Events;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher.Services.DebugAndBugReport;

/// <summary>
/// A Serilog sink that forwards log events to the debug window view model, buffering messages until it is connected.
/// </summary>
public class DebugWindowSink : ILogEventSink
{
    private static readonly Lock SinkLock = new();
    private static readonly List<string> MessageBuffer = [];
    private static DebugViewModel? _viewModel;

    /// <summary>
    /// Gets the debug view model currently connected to the sink.
    /// </summary>
    public static DebugViewModel? ViewModel
    {
        get
        {
            lock (SinkLock)
            {
                return _viewModel;
            }
        }
    }

    /// <summary>
    /// Connects the sink to the given debug view model and flushes any buffered messages.
    /// </summary>
    /// <param name="viewModel">The debug view model to connect.</param>
    public static void Connect(DebugViewModel viewModel)
    {
        lock (SinkLock)
        {
            _viewModel = viewModel;
            if (_viewModel != null && MessageBuffer.Count > 0)
            {
                _viewModel.LoadBufferedMessages(MessageBuffer.ToList());
            }
        }
    }

    /// <summary>
    /// Disconnects the sink from the currently connected debug view model.
    /// </summary>
    public static void Disconnect()
    {
        lock (SinkLock)
        {
            _viewModel = null;
        }
    }

    /// <summary>
    /// Emits a log event to the sink, appending the formatted message to the buffer and the connected view model.
    /// </summary>
    /// <param name="logEvent">The log event to emit.</param>
    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var formattedMessage = $"{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{logEvent.Level}] {message}";

        lock (SinkLock)
        {
            MessageBuffer.Add(formattedMessage);

            _viewModel?.AppendLogMessage(formattedMessage);
        }
    }
}