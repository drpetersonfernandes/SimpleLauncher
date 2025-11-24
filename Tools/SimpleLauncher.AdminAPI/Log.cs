namespace SimpleLauncher.AdminAPI;

public static class Log
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "An error occurred while seeding the database")]
    public static void DatabaseSeedingError(ILogger logger, Exception ex)
    {
        throw new NotImplementedException();
    }
}