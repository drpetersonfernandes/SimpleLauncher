namespace SimpleLauncher.AdminAPI;

public static partial class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "An error occurred while seeding the database")]
    public static partial void DatabaseSeedingError(ILogger logger, Exception ex);
}