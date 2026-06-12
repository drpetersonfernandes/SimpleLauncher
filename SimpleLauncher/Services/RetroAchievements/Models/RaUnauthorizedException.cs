namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Exception thrown when the RetroAchievements API returns an Unauthorized (401) response.
/// </summary>
public class RaUnauthorizedException : Exception
{
    public RaUnauthorizedException()
    {
    }

    public RaUnauthorizedException(string message) : base(message)
    {
    }

    public RaUnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
