namespace SimpleLauncher.Services.InjectEmulatorConfig;

/// <summary>
/// Exception thrown when Azahar configuration cannot be modified due to file permission issues.
/// </summary>
public class AzaharPermissionException : Exception
{
    public AzaharPermissionException(string message) : base(message)
    {
    }

    public AzaharPermissionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
