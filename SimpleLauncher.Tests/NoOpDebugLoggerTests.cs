using SimpleLauncher.Core.Interfaces;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="NoOpDebugLogger"/>: the fallback logger must never throw.
/// </summary>
public class NoOpDebugLoggerTests
{
    [Fact]
    public void Log_DoesNotThrow()
    {
        var logger = new NoOpDebugLogger();
        logger.Log("some message");
    }

    [Fact]
    public void LogException_DoesNotThrow()
    {
        var logger = new NoOpDebugLogger();
        logger.LogException(new InvalidOperationException("boom"));
        logger.LogException(null!);
    }

    [Fact]
    public void LogException_WithContextMessage_DoesNotThrow()
    {
        var logger = new NoOpDebugLogger();
        logger.LogException(new InvalidOperationException("boom"), "context");
    }

    [Fact]
    public void OpenDebugWindow_DoesNotThrow()
    {
        var logger = new NoOpDebugLogger();
        logger.OpenDebugWindow();
    }
}
