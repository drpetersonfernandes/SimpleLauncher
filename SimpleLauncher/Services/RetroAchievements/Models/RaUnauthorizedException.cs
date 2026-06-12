using System.Runtime.Serialization;

namespace SimpleLauncher.Services.RetroAchievements.Models;

/// <summary>
/// Exception thrown when the RetroAchievements API returns an Unauthorized (401) response.
/// </summary>
[Serializable]
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

    [Obsolete("This API supports obsolete formatter-based serialization.")]
    protected RaUnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
