using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DebugViewModel"/> class covering log message appending, clearing, and copy functionality.
/// </summary>
public class DebugViewModelTests
{
    private static DebugViewModel CreateViewModel()
    {
        return new DebugViewModel();
    }

    /// <summary>
    /// Verifies that the constructor initializes empty log messages and disabled command states.
    /// </summary>
    [Fact]
    public void ConstructorInitializesEmptyLog()
    {
        var viewModel = CreateViewModel();
        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    /// <summary>
    /// Verifies that AppendLogMessage adds a message with a timestamp and enables clear/copy commands.
    /// </summary>
    [Fact]
    public void AppendLogMessageAddsMessageWithTimestamp()
    {
        var viewModel = CreateViewModel();
        var timestamp = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
        const string message = "Test message";
        var formattedMessage = $"{timestamp} [Debug] {message}";
        viewModel.AppendLogMessage(formattedMessage);
        Assert.Single(viewModel.LogMessages);
        Assert.Contains(message, viewModel.LogMessages[0], StringComparison.Ordinal);
        Assert.Contains(timestamp.Substring(0, 7), viewModel.LogMessages[0], StringComparison.Ordinal);
        Assert.Contains(message, viewModel.LogText, StringComparison.Ordinal);
        Assert.True(viewModel.CanClearLog);
        Assert.True(viewModel.CanCopyLog);
    }

    /// <summary>
    /// Verifies that AppendLogMessage adds multiple messages to the log.
    /// </summary>
    [Fact]
    public void AppendLogMessageAddsMultipleMessages()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");
        viewModel.AppendLogMessage("Message 3");
        Assert.Equal(3, viewModel.LogMessages.Count);
        Assert.Contains("Message 1", viewModel.LogText, StringComparison.Ordinal);
        Assert.Contains("Message 2", viewModel.LogText, StringComparison.Ordinal);
        Assert.Contains("Message 3", viewModel.LogText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that ClearLogCommand can execute when the log has messages.
    /// </summary>
    [Fact]
    public void ClearLogCommandCanExecuteWhenLogHasMessages()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test");
        Assert.True(viewModel.ClearLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that ClearLogCommand cannot execute when the log is empty.
    /// </summary>
    [Fact]
    public void ClearLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = CreateViewModel();
        Assert.False(viewModel.ClearLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that ClearLogCommand clears all messages and disables clear/copy commands.
    /// </summary>
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

    /// <summary>
    /// Verifies that CopyLogCommand can execute when the log has content.
    /// </summary>
    [Fact]
    public void CopyLogCommandCanExecuteWhenLogHasContent()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test");
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that CopyLogCommand cannot execute when the log is empty.
    /// </summary>
    [Fact]
    public void CopyLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = CreateViewModel();
        Assert.False(viewModel.CopyLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that CopyLogCommand can execute when the log has a test message.
    /// </summary>
    [Fact]
    public void CopyLogCommandExistsAndCanExecuteWhenLogHasContent()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("Test message for clipboard");
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that LogText contains all appended messages joined with newlines.
    /// </summary>
    [Fact]
    public void LogTextContainsAllMessagesJoined()
    {
        var viewModel = CreateViewModel();
        viewModel.AppendLogMessage("First");
        viewModel.AppendLogMessage("Second");
        var logText = viewModel.LogText;
        Assert.Contains("First", logText, StringComparison.Ordinal);
        Assert.Contains("Second", logText, StringComparison.Ordinal);
        Assert.EndsWith(Environment.NewLine, logText, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for CanClearLog when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForCanClearLogWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, nameof(DebugViewModel.CanClearLog), StringComparison.Ordinal))
            {
                raised = true;
            }
        };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for CanCopyLog when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForCanCopyLogWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, nameof(DebugViewModel.CanCopyLog), StringComparison.Ordinal))
            {
                raised = true;
            }
        };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for LogText when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForLogTextWhenMessagesAdded()
    {
        var viewModel = CreateViewModel();
        var raised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (string.Equals(e.PropertyName, nameof(DebugViewModel.LogText), StringComparison.Ordinal))
            {
                raised = true;
            }
        };
        viewModel.AppendLogMessage("Test");
        Assert.True(raised);
    }
}
