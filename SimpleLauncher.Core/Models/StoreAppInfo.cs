namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents a Microsoft Store application with its metadata.
/// </summary>
public class StoreAppInfo
{
    /// <summary>
    /// The display name of the Microsoft Store application.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// The unique application identifier in the Microsoft Store.
    /// </summary>
    public string AppId { get; set; } = null!;

    /// <summary>
    /// The installation directory path of the application.
    /// </summary>
    public string InstallLocation { get; set; } = null!;

    /// <summary>
    /// The package family name used to identify the application package.
    /// </summary>
    public string PackageFamilyName { get; set; } = null!;

    /// <summary>
    /// The relative path to the application's logo image within the installation directory.
    /// </summary>
    public string LogoRelativePath { get; set; } = null!;
}