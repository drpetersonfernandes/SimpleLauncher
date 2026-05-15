using System.Globalization;
using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.ViewModels;
using Xunit;

namespace SimpleLauncher.Tests;

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

    [Fact]
    public void Constructor_InitializesEmptyLog()
    {
        var viewModel = new DebugViewModel();

        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    [Fact]
    public void AppendLogMessage_AddsMessageWithTimestamp()
    {
        var viewModel = new DebugViewModel();
        const string message = "Test message";

        viewModel.AppendLogMessage(message);

        Assert.Single(viewModel.LogMessages);
        Assert.Contains(message, viewModel.LogMessages[0]);
        Assert.Contains(DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture).Substring(0, 5), viewModel.LogMessages[0]);
        Assert.Contains(message, viewModel.LogText);
        Assert.True(viewModel.CanClearLog);
        Assert.True(viewModel.CanCopyLog);
    }

    [Fact]
    public void AppendLogMessage_AddsMultipleMessages()
    {
        var viewModel = new DebugViewModel();

        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");
        viewModel.AppendLogMessage("Message 3");

        Assert.Equal(3, viewModel.LogMessages.Count);
        Assert.Contains("Message 1", viewModel.LogText);
        Assert.Contains("Message 2", viewModel.LogText);
        Assert.Contains("Message 3", viewModel.LogText);
    }

    [Fact]
    public void ClearLogCommand_CanExecute_WhenLogHasMessages()
    {
        var viewModel = new DebugViewModel();
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    [Fact]
    public void ClearLogCommand_CannotExecute_WhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel();

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.False(canExecute); // Can only execute when there are messages to clear
    }

    [Fact]
    public void ClearLogCommand_ClearsAllMessages()
    {
        var viewModel = new DebugViewModel();
        viewModel.AppendLogMessage("Message 1");
        viewModel.AppendLogMessage("Message 2");

        viewModel.ClearLogCommand.Execute(null);

        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

    [Fact]
    public void CopyLogCommand_CanExecute_WhenLogHasContent()
    {
        var viewModel = new DebugViewModel();
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    [Fact]
    public void CopyLogCommand_CannotExecute_WhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel();

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.False(canExecute);
    }

    [Fact]
    public void CopyLogCommand_ExistsAndCanExecuteWhenLogHasContent()
    {
        var viewModel = new DebugViewModel();
        viewModel.AppendLogMessage("Test message for clipboard");

        // Verify command exists and CanExecute returns true when there's content
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

    [Fact]
    public void LogText_ContainsAllMessagesJoined()
    {
        var viewModel = new DebugViewModel();
        viewModel.AppendLogMessage("First");
        viewModel.AppendLogMessage("Second");

        var logText = viewModel.LogText;

        Assert.Contains("First", logText);
        Assert.Contains("Second", logText);
        Assert.EndsWith(Environment.NewLine, logText);
    }

    [Fact]
    public void PropertyChanged_RaisedForCanClearLog_WhenMessagesAdded()
    {
        var viewModel = new DebugViewModel();
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

    [Fact]
    public void PropertyChanged_RaisedForCanCopyLog_WhenMessagesAdded()
    {
        var viewModel = new DebugViewModel();
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

    [Fact]
    public void PropertyChanged_RaisedForLogText_WhenMessagesAdded()
    {
        var viewModel = new DebugViewModel();
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
}
