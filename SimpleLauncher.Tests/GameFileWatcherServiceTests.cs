using SimpleLauncher.Core.Services.GameFileWatcher;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Integration-style tests for <see cref="GameFileWatcherService"/> using real temp directories
/// and the OS <see cref="FileSystemWatcher"/>. These are inherently timing-based; the debounce
/// delay is shortened to keep the tests fast.
/// </summary>
public class GameFileWatcherServiceTests : IDisposable
{
    private readonly string _watchDir;
    private readonly GameFileWatcherService _service;

    public GameFileWatcherServiceTests()
    {
        _watchDir = Path.Combine(Path.GetTempPath(), $"SL_Watcher_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_watchDir);
        _service = new GameFileWatcherService(new NoOpLogger())
        {
            DebounceDelay = TimeSpan.FromMilliseconds(100)
        };
    }

    public void Dispose()
    {
        _service.Dispose();
        try
        {
            if (Directory.Exists(_watchDir))
            {
                Directory.Delete(_watchDir, true);
            }
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static Task<string> WaitForEventAsync(TaskCompletionSource<string> tcs, int timeoutMs = 3000)
    {
        return tcs.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
    }

    private static async Task AssertNoEventAsync(TaskCompletionSource<string> tcs, int waitMs = 600)
    {
        await Task.Delay(waitMs);
        Assert.False(tcs.Task.IsCompleted, "No event should have been raised.");
    }

    [Fact]
    public Task StartWatching_NonexistentFolder_DoesNotThrowAndRaisesNoEvents()
    {
        var missingDir = Path.Combine(_watchDir, "does-not-exist");
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) => tcs.TrySetResult(e.Value);

        _service.StartWatching([missingDir], "NES");

        return AssertNoEventAsync(tcs);
    }

    [Fact]
    public async Task FileChange_RaisesEventWithSystemName()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) => tcs.TrySetResult(e.Value);

        _service.StartWatching([_watchDir], "NES");

        // Give the OS watcher a moment to start before creating the file
        await Task.Delay(100);
        File.WriteAllText(Path.Combine(_watchDir, "game.zip"), "rom-data");

        var systemName = await WaitForEventAsync(tcs);
        Assert.Equal("NES", systemName);
    }

    [Fact]
    public async Task ExtensionFilter_IgnoresNonMatchingFiles()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) => tcs.TrySetResult(e.Value);

        _service.StartWatching([_watchDir], "NES", ["zip"]);

        await Task.Delay(100);
        File.WriteAllText(Path.Combine(_watchDir, "notes.txt"), "not a rom");

        await AssertNoEventAsync(tcs);

        File.WriteAllText(Path.Combine(_watchDir, "game.zip"), "rom-data");

        Assert.Equal("NES", await WaitForEventAsync(tcs));
    }

    [Fact]
    public async Task RapidFileChanges_AreDebouncedIntoSingleEvent()
    {
        var eventCount = 0;
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, _) =>
        {
            Interlocked.Increment(ref eventCount);
            tcs.TrySetResult();
        };

        _service.StartWatching([_watchDir], "SNES");

        await Task.Delay(100);
        File.WriteAllText(Path.Combine(_watchDir, "game1.sfc"), "data");
        await Task.Delay(20);
        File.WriteAllText(Path.Combine(_watchDir, "game2.sfc"), "data");

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
        // Allow any spurious second event to surface before asserting
        await Task.Delay(300);

        Assert.Equal(1, eventCount);
    }

    [Fact]
    public async Task Rename_AlsoRaisesEvent()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) => tcs.TrySetResult(e.Value);

        _service.StartWatching([_watchDir], "NES");

        await Task.Delay(100);
        var source = Path.Combine(_watchDir, "old-name.zip");
        var renamed = Path.Combine(_watchDir, "new-name.zip");
        File.WriteAllText(source, "data");
        File.Move(source, renamed);

        Assert.Equal("NES", await WaitForEventAsync(tcs));
    }

    [Fact]
    public async Task StopWatching_PreventsFurtherEvents()
    {
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) => tcs.TrySetResult(e.Value);

        _service.StartWatching([_watchDir], "NES");
        await Task.Delay(100);
        _service.StopWatching();

        File.WriteAllText(Path.Combine(_watchDir, "game.zip"), "rom-data");

        await AssertNoEventAsync(tcs);
    }

    [Fact]
    public async Task StartWatching_NewCall_StopsPreviousWatchers()
    {
        var events = new List<string>();
        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        _service.GameFilesChanged += (_, e) =>
        {
            lock (events)
            {
                events.Add(e.Value);
            }

            tcs.TrySetResult();
        };

        _service.StartWatching([_watchDir], "System A");
        await Task.Delay(100);
        _service.StartWatching([_watchDir], "System B");
        await Task.Delay(100);

        File.WriteAllText(Path.Combine(_watchDir, "game.zip"), "data");
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(3));

        Assert.DoesNotContain("System A", events, StringComparer.Ordinal);
        Assert.Contains("System B", events, StringComparer.Ordinal);
    }

    [Fact]
    public void StartWatching_AfterDispose_ThrowsObjectDisposedException()
    {
        var service = new GameFileWatcherService(new NoOpLogger());
        service.Dispose();

        Assert.Throws<ObjectDisposedException>(() => service.StartWatching([_watchDir], "NES"));
    }

    [Fact]
    public void Dispose_Twice_DoesNotThrow()
    {
        _service.Dispose();
        _service.Dispose();
    }
}
