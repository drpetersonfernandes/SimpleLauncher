namespace SimpleLauncher.Core.Services.DownloadService;

/// <summary>
///     Classifies download exceptions that are expected user/network conditions
///     (timeouts, connection failures, cancellations) versus genuine code defects.
///     Expected failures must be logged at Information level so the bug report
///     API (Warning+) does not pick them up (see bug 65965).
/// </summary>
public static class DownloadErrorClassifier
{
    /// <summary>
    ///     Determines whether the exception represents an expected download failure —
    ///     resilience-pipeline timeout, unreachable/reset connection, network read failure,
    ///     or cancellation — rather than an application bug.
    /// </summary>
    /// <param name="ex">The exception thrown during the download process.</param>
    /// <returns>True when the failure is an expected user/network condition.</returns>
    public static bool IsExpectedDownloadException(Exception ex)
    {
        return ex is Polly.Timeout.TimeoutRejectedException
            or HttpRequestException
            or OperationCanceledException
            or IOException;
    }
}