namespace SimpleLauncher.AdminAPI.Models;

/// <summary>
/// Represents the data payload sent to the external bug report service.
/// </summary>
public class BugReportPayload
{
    public string? Message { get; set; }
    public string? ApplicationName { get; set; }
    public string? Version { get; set; }
    public string? UserInfo { get; set; }
    public string? StackTrace { get; set; }
}