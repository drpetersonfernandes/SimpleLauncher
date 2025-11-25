namespace SimpleLauncher.AdminAPI;

public static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "An error occurred while seeding the database")]
    public static partial void DatabaseSeedingError(ILogger logger, Exception ex);

    // Add these two new methods:
    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "User logged in.")]
    public static partial void UserLoggedIn(ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = "User account locked out.")]
    public static partial void UserAccountLockedOut(ILogger logger);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "Imported {Count} systems for architecture {Arch}.")]
    public static partial void ImportedSystems(ILogger logger, int count, string arch);

    [LoggerMessage(
        EventId = 5,
        Level = LogLevel.Error,
        Message = "Error importing XML")]
    public static partial void ErrorImportingXml(ILogger logger, Exception ex);

    // New LoggerMessage delegates for BugReportService
    [LoggerMessage(
        EventId = 6,
        Level = LogLevel.Warning,
        Message = "Bug Report Service URL or API Key is not configured. Skipping bug report.")]
    public static partial void BugReportServiceNotConfigured(ILogger logger);

    [LoggerMessage(
        EventId = 7,
        Level = LogLevel.Information,
        Message = "Successfully sent bug report. Response: {Response}")]
    public static partial void BugReportSentSuccess(ILogger logger, string response);

    [LoggerMessage(
        EventId = 8,
        Level = LogLevel.Error,
        Message = "Failed to send bug report. Status Code: {StatusCode}. Response: {ErrorBody}")]
    public static partial void BugReportSentFailure(ILogger logger, System.Net.HttpStatusCode statusCode, string errorBody);

    [LoggerMessage(
        EventId = 9,
        Level = LogLevel.Error,
        Message = "An exception occurred while trying to send a bug report.")]
    public static partial void BugReportSendException(ILogger logger, Exception ex);

    // New LoggerMessage delegate for GlobalExceptionHandler
    [LoggerMessage(
        EventId = 10,
        Level = LogLevel.Error,
        Message = "An unhandled exception has occurred.")]
    public static partial void UnhandledException(ILogger logger, Exception ex);
}
