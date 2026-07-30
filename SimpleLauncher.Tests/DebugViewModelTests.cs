using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

public class DebugViewModelTests
{
    private static DebugViewModel CreateViewModel() => new();

    [Fact]
    public void ConstructorInitializesEmptyLog()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    [Fact]
    public void AppendLogMessageAddsMessageWithTimestamp()
    {
        var viewModel = CreateViewModel();
        var timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        const string message = "Test message";
        var formattedMessage = $"{timestamp} [Debug] {message}";
        viewModel.AppendLogMessage(formattedMessage);
        Assert.Single(viewModel.LogMessages);
        Assert.Contains(message, viewModel.LogMessages[0]);
        Assert.Contains(timestamp.Substring(0, 7), viewModel.LogMessages[0]);
        Assert.Contains(message, viewModel.LogText);
        Assert.True(viewModel.CanClearLog);
        Assert.True(viewModel.CanCopyLog);
    }

    [Fact]
    public void AppendLogMessageAddsMultipleMessages()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");
        viewModel.AppendLogMessage("Message 3");
        Assert.Equal(3, viewModel.LogMessages.Count);
        Assert.Contains("Message 1", viewModel.LogText);
        Assert.Contains("Message 2", viewModel.LogText);
        Assert.Contains("Message 3", viewModel.LogText);
    }

    [Fact]
    public void ClearLogCommandCanExecuteWhenLogHasMessages()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test");
        Assert.True(viewModel.ClearLogCommand.CanExecute(null));
    }

    [Fact]
    public void ClearLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = CreateViewModel();
        Assert.False(viewModel.ClearLogCommand.CanExecute(null));
    }

    [Fact]
    public void ClearLogCommandClearsAllMessages()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");
        viewModel.ClearLogCommand.Execute(null);
        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    [Fact]
    public void CopyLogCommandCanExecuteWhenLogHasContent()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test");
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    [Fact]
    public void CopyLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = CreateViewModel();
        Assert.False(viewModel.CopyLogCommand.CanExecute(null));
    }

    [Fact]
    public void CopyLogCommandExistsAndCanExecuteWhenLogHasContent()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test message for clipboard");
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    [Fact]
    public void LogTextContainsAllMessagesJoined()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("First");
        viewModel.AppendLogMessage("Second");
        var logText = viewModel.LogText;
        Assert.Contains("First", logText);
        Assert.Contains("Second", logText);
        Assert.EndsWith(Environment.NewLine, logText);
    }

    [Fact]
    public void PropertyChangedRaisedForCanClearLogWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(DebugViewModel.CanClearLog)) raised = true; };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }

    [Fact]
    public void PropertyChangedRaisedForCanCopyLogWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(DebugViewModel.CanCopyLog)) raised = true; };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }

    [Fact]
    public void PropertyChangedRaisedForLogTextWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(DebugViewModel.LogText)) raised = true; };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }
}
