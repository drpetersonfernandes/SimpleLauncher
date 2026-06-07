namespace SimpleLauncher.Core.Services.GameScan.Models;

/// <summary>
/// Represents a Microsoft Store application with its metadata.
/// </summary>
public class StoreAppInfo
{
    public string Name { get; set; }
    public string AppId { get; set; }
    public string InstallLocation { get; set; }
    public string PackageFamilyName { get; set; }
    public string LogoRelativePath { get; set; }
}