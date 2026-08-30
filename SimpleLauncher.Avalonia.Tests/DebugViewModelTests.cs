using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the DebugWindow ViewModel (Phase 4.1 port). The view model posts
///     dispatcher work to the dedicated UI thread provided by the headless platform.
/// </summary>
public class DebugViewModelTests
{
    private const int MaxMessageCount = 5000;

    public DebugViewModelTests()
    {
        HeadlessAvalonia.EnsureInitialized();
        DebugWindowSink.Disconnect();
    }

    [Fact]
    public void Ctor_StartsWithEmptyLog()
    {
        var vm = new DebugViewModel();
        Assert.Empty(vm.LogMessages);
        Assert.Equal("", vm.LogText);
        Assert.False(vm.CanClearLog);
        Assert.False(vm.CanCopyLog);
        DebugWindowSink.Disconnect();
    }

    [Fact]
    public async Task AppendLogMessage_AddsMessageAndEnablesCommands()
    {
        var vm = new DebugViewModel();

        vm.AppendLogMessage("first log entry");

        await HeadlessAvalonia.WaitUntilAsync(() => vm.LogMessages.Count == 1);

        Assert.Contains("first log entry", vm.LogText, StringComparison.OrdinalIgnoreCase);
        Assert.True(vm.CanClearLog);
        Assert.True(vm.CanCopyLog);
        Assert.True(vm.ClearLogCommand.CanExecute(null));
        Assert.True(vm.CopyLogCommand.CanExecute(null));
        DebugWindowSink.Disconnect();
    }

    [Fact]
    public async Task LoadBufferedMessages_EvictsOldestBeyondLimit()
    {
        var vm = new DebugViewModel();
        var messages = Enumerable.Range(0, MaxMessageCount + 10).Select(i => $"msg {i}").ToList();

        vm.LoadBufferedMessages(messages);

        await HeadlessAvalonia.WaitUntilAsync(() => vm.LogMessages.Count == MaxMessageCount);

        Assert.Equal(MaxMessageCount, vm.LogMessages.Count);
        Assert.Equal("msg 10", vm.LogMessages[0]); // oldest 10 evicted
        Assert.Equal($"msg {MaxMessageCount + 9}", vm.LogMessages[^1]);
        DebugWindowSink.Disconnect();
    }

    [Fact]
    public async Task ClearLogCommand_ClearsLogAndDisablesCommands()
    {
        var vm = new DebugViewModel();
        vm.AppendLogMessage("temp entry");
        await HeadlessAvalonia.WaitUntilAsync(() => vm.LogMessages.Count == 1);

        vm.ClearLogCommand.Execute(null);

        await HeadlessAvalonia.WaitUntilAsync(() => vm.LogMessages.Count == 0);
        Assert.Equal("", vm.LogText);
        Assert.False(vm.CanClearLog);
        Assert.False(vm.CopyLogCommand.CanExecute(null));
        DebugWindowSink.Disconnect();
    }

    [Fact]
    public void CopyLogCommand_WithNoLifetime_DoesNotThrow()
    {
        var vm = new DebugViewModel();
        vm.CopyLogCommand.Execute(null); // headless: no IClassicDesktopStyleApplicationLifetime → no-op
        DebugWindowSink.Disconnect();
    }
}