using System.Globalization;
using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests the <see cref="DebugViewModel"/> log message management, command execution, and property change notifications.
/// </summary>
public class DebugViewModelTests : IDisposable
{
    public DebugViewModelTests()
    {
        ServiceProviderMock.Install();
    }

    public void Dispose()
    {
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that the constructor initializes an empty log with disabled commands.
    /// </summary>
    [Fact]
    public void ConstructorInitializesEmptyLog()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    /// <summary>
    /// Verifies that appending a log message adds it with a timestamp and enables clear/copy commands.
    /// </summary>
    [Fact]
    public void AppendLogMessageAddsMessageWithTimestamp()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        const string message = "Test message";

        viewModel.AppendLogMessage(message);

        Assert.Single(viewModel.LogMessages);
        Assert.Contains(message, viewModel.LogMessages[0]);
        Assert.Contains(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture).Substring(0, 5), viewModel.LogMessages[0]);
        Assert.Contains(message, viewModel.LogText);
        Assert.True(viewModel.CanClearLog);
        Assert.True(viewModel.CanCopyLog);
    }

    /// <summary>
    /// Verifies that appending multiple log messages adds each one to the log.
    /// </summary>
    [Fact]
    public void AppendLogMessageAddsMultipleMessages()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");
        viewModel.AppendLogMessage("Message 3");

        Assert.Equal(3, viewModel.LogMessages.Count);
        Assert.Contains("Message 1", viewModel.LogText);
        Assert.Contains("Message 2", viewModel.LogText);
        Assert.Contains("Message 3", viewModel.LogText);
    }

    /// <summary>
    /// Verifies that the clear log command can execute when the log contains messages.
    /// </summary>
    [Fact]
    public void ClearLogCommandCanExecuteWhenLogHasMessages()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    /// <summary>
    /// Verifies that the clear log command cannot execute when the log is empty.
    /// </summary>
    [Fact]
    public void ClearLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.False(canExecute); // Can only execute when there are messages to clear
    }

    /// <summary>
    /// Verifies that the clear log command removes all messages and resets state.
    /// </summary>
    [Fact]
    public void ClearLogCommandClearsAllMessages()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");

        viewModel.ClearLogCommand.Execute(null);

        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    /// <summary>
    /// Verifies that the copy log command can execute when the log contains content.
    /// </summary>
    [Fact]
    public void CopyLogCommandCanExecuteWhenLogHasContent()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    /// <summary>
    /// Verifies that the copy log command cannot execute when the log is empty.
    /// </summary>
    [Fact]
    public void CopyLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.False(canExecute);
    }

    /// <summary>
    /// Verifies that the copy log command exists and can execute when the log has content.
    /// </summary>
    [Fact]
    public void CopyLogCommandExistsAndCanExecuteWhenLogHasContent()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test message for clipboard");

        // Verify command exists and CanExecute returns true when there's content
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    /// <summary>
    /// Verifies that LogText contains all appended messages joined together.
    /// </summary>
    [Fact]
    public void LogTextContainsAllMessagesJoined()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("First");
        viewModel.AppendLogMessage("Second");

        var logText = viewModel.LogText;

        Assert.Contains("First", logText);
        Assert.Contains("Second", logText);
        Assert.EndsWith(Environment.NewLine, logText);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for CanClearLog when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForCanClearLogWhenMessagesAdded()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DebugViewModel.CanClearLog))
            {
                propertyChangedRaised = true;
            }
        };

        viewModel.AppendLogMessage("Test");

        Assert.True(propertyChangedRaised);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for CanCopyLog when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForCanCopyLogWhenMessagesAdded()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DebugViewModel.CanCopyLog))
            {
                propertyChangedRaised = true;
            }
        };

        viewModel.AppendLogMessage("Test");

        Assert.True(propertyChangedRaised);
    }

    /// <summary>
    /// Verifies that PropertyChanged is raised for LogText when messages are added.
    /// </summary>
    [Fact]
    public void PropertyChangedRaisedForLogTextWhenMessagesAdded()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(DebugViewModel.LogText))
            {
                propertyChangedRaised = true;
            }
        };

        viewModel.AppendLogMessage("Test");

        Assert.True(propertyChangedRaised);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
