using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the UpdateLogWindow ViewModel (Phase 4.1 port).
/// </summary>
public class UpdateLogViewModelTests
{
    [Fact]
    public void Ctor_StartsWithEmptyLog()
    {
        var vm = new UpdateLogViewModel();
        Assert.Equal("", vm.LogText);
    }

    [Fact]
    public void AppendLog_FormatsTimestampedLine()
    {
        var vm = new UpdateLogViewModel();
        vm.AppendLog("downloaded package");

        Assert.Matches(@"^\d{2}:\d{2}:\d{2} - downloaded package\r?\n$", vm.LogText);
    }

    [Fact]
    public void AppendLog_AccumulatesMultipleLines()
    {
        var vm = new UpdateLogViewModel();
        vm.AppendLog("first");
        vm.AppendLog("second");

        var lines = vm.LogText.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.All(lines, line => Assert.Contains(" - ", line));
    }
}