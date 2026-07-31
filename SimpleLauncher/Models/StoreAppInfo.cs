namespace SimpleLauncher.Models;

/// <summary>
/// Represents a Microsoft Store application with its metadata.
/// </summary>
public class StoreAppInfo
{
    public string Name { get; set; } = null!;
    public string AppId { get; set; } = null!;
    public string InstallLocation { get; set; } = null!;
    public string PackageFamilyName { get; set; } = null!;
    public string LogoRelativePath { get; set; } = null!;
}
