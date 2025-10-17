using System;

namespace SimpleLauncher.Models;

// Custom exception for RetroAchievements API Unauthorized errors
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