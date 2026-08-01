namespace SimpleLauncher.Services.InjectEmulatorConfig;

/// <summary>
/// Exception thrown when PCSX2 configuration cannot be modified due to file permission issues.
/// </summary>
public class Pcsx2PermissionException : Exception
{
    public Pcsx2PermissionException(string message) : base(message)
    {
    }

    public Pcsx2PermissionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
