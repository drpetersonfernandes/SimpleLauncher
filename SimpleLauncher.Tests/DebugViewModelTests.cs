using System.Globalization;
using SimpleLauncher.Core.Services.DebugAndBugReport;
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
    public void ConstructorInitializesEmptyLog()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        Assert.Empty(viewModel.LogMessages);
        Assert.Empty(viewModel.LogText);
        Assert.False(viewModel.CanClearLog);
        Assert.False(viewModel.CanCopyLog);
    }

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

    [Fact]
    public void ClearLogCommandCanExecuteWhenLogHasMessages()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    [Fact]
    public void ClearLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        var canExecute = viewModel.ClearLogCommand.CanExecute(null);

        Assert.False(canExecute); // Can only execute when there are messages to clear
    }

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

    [Fact]
    public void CopyLogCommandCanExecuteWhenLogHasContent()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test");

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.True(canExecute);
    }

    [Fact]
    public void CopyLogCommandCannotExecuteWhenLogIsEmpty()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());

        var canExecute = viewModel.CopyLogCommand.CanExecute(null);

        Assert.False(canExecute);
    }

    [Fact]
    public void CopyLogCommandExistsAndCanExecuteWhenLogHasContent()
    {
        var viewModel = new DebugViewModel(new NoOpLogErrors(), new NoOpMessageBoxLibraryService(), new NoOpDebugLogger());
        viewModel.AppendLogMessage("Test message for clipboard");

        // Verify command exists and CanExecute returns true when there's content
        Assert.True(viewModel.CopyLogCommand.CanExecute(null));
    }

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
