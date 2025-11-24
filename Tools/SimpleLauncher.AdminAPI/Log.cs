namespace SimpleLauncher.AdminAPI;

public static class Log
{
    public static void DatabaseSeedingError(ILogger logger, Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}