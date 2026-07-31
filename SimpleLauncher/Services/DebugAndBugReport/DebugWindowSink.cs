using Serilog.Core;
using Serilog.Events;
using SimpleLauncher.ViewModels;

namespace SimpleLauncher.Services.DebugAndBugReport;

public class DebugWindowSink : ILogEventSink
{
    private static readonly object SinkLock = new();
    private static readonly List<string> MessageBuffer = [];
    private static DebugViewModel? _viewModel;

    public static DebugViewModel? ViewModel
    {
        get
        {
            lock (SinkLock) { return _viewModel; }
        }
    }

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

    public static void Disconnect()
    {
        lock (SinkLock)
        {
            _viewModel = null;
        }
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage();
        var formattedMessage = $"{logEvent.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{logEvent.Level}] {message}";

        lock (SinkLock)
        {
            MessageBuffer.Add(formattedMessage);

            if (_viewModel != null)
            {
                _viewModel.AppendLogMessage(formattedMessage);
            }
        }
    }
}
