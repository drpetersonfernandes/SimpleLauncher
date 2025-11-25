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
}