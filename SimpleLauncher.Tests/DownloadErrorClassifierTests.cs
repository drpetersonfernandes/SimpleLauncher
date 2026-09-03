using SimpleLauncher.Core.Services.DownloadService;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="DownloadErrorClassifier" /> — expected download failures
///     (timeouts, connection failures, cancellations) must never reach the bug report
///     API as Warning/Error (see bug 65965).
/// </summary>
public class DownloadErrorClassifierTests
{
    [Fact]
    public void ResiliencePipelineTimeoutIsExpected()
    {
        var ex = new Polly.Timeout.TimeoutRejectedException(
            "The operation didn't complete within the allowed timeout.");

        Assert.True(DownloadErrorClassifier.IsExpectedDownloadException(ex));
    }

    [Fact]
    public void NetworkFailuresAreExpected()
    {
        Assert.True(DownloadErrorClassifier.IsExpectedDownloadException(
            new HttpRequestException("The SSL connection could not be established.")));
        Assert.True(DownloadErrorClassifier.IsExpectedDownloadException(
            new IOException("Unable to read data from the transport connection.")));
    }

    [Fact]
    public void CancellationsAreExpected()
    {
        Assert.True(DownloadErrorClassifier.IsExpectedDownloadException(new OperationCanceledException()));
        Assert.True(DownloadErrorClassifier.IsExpectedDownloadException(new TaskCanceledException()));
    }

    [Fact]
    public void UnexpectedExceptionsAreNotExpected()
    {
        Assert.False(DownloadErrorClassifier.IsExpectedDownloadException(new NullReferenceException()));
        Assert.False(DownloadErrorClassifier.IsExpectedDownloadException(new InvalidOperationException()));
        Assert.False(DownloadErrorClassifier.IsExpectedDownloadException(new UnauthorizedAccessException()));
    }
}