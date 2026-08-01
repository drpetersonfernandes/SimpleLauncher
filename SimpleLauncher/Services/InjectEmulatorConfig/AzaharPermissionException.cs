namespace SimpleLauncher.Services.InjectEmulatorConfig;

/// <summary>
/// Exception thrown when Azahar configuration cannot be modified due to file permission issues.
/// </summary>
public class AzaharPermissionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AzaharPermissionException"/> class with a message.
    /// </summary>
    /// <param name="message">The message describing the permission error.</param>
    public AzaharPermissionException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzaharPermissionException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The message describing the permission error.</param>
    /// <param name="innerException">The exception that caused the permission error.</param>
    public AzaharPermissionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
