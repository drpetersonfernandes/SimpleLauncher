namespace SimpleLauncher.Services.EasyMode.Models;

/// <summary>
/// Configuration for a single emulator, including download links, core details, and image pack metadata.
/// </summary>
public class EmulatorConfig
{
    /// <summary>Gets or sets the display name of the emulator.</summary>
    public string EmulatorName { get; set; }
    /// <summary>Gets or sets the local file path where the emulator is installed.</summary>
    public string EmulatorLocation { get; set; }
    /// <summary>Gets or sets the command-line parameters to pass to the emulator.</summary>
    public string EmulatorParameters { get; set; }
    /// <summary>Gets or sets the URL of the emulator's download page.</summary>
    public string EmulatorDownloadPage { get; set; }
    /// <summary>Gets or sets the latest available version string for the emulator.</summary>
    public string EmulatorLatestVersion { get; set; }
    /// <summary>Gets or sets the direct download URL for the emulator archive.</summary>
    public string EmulatorDownloadLink { get; set; }
    /// <summary>Gets or sets the extraction path relative to the emulator download.</summary>
    public string EmulatorDownloadExtractPath { get; set; }
    /// <summary>Gets or sets the local file path where the libretro core is installed.</summary>
    public string CoreLocation { get; set; }
    /// <summary>Gets or sets the latest available version string for the core.</summary>
    public string CoreLatestVersion { get; set; }
    /// <summary>Gets or sets the direct download URL for the core archive.</summary>
    public string CoreDownloadLink { get; set; }
    /// <summary>Gets or sets the extraction path relative to the core download.</summary>
    public string CoreDownloadExtractPath { get; set; }
    /// <summary>Gets or sets the local folder path where image packs are stored.</summary>
    public string ImagePackLocation { get; set; }
    /// <summary>Gets or sets the latest available version string for the image pack.</summary>
    public string ImagePackLatestVersion { get; set; }
    /// <summary>Gets or sets the primary direct download URL for the image pack.</summary>
    public string ImagePackDownloadLink { get; set; }
    /// <summary>Gets or sets an alternative download URL for the image pack.</summary>
    public string ImagePackDownloadLink2 { get; set; }
    /// <summary>Gets or sets a third download URL for the image pack.</summary>
    public string ImagePackDownloadLink3 { get; set; }
    /// <summary>Gets or sets a fourth download URL for the image pack.</summary>
    public string ImagePackDownloadLink4 { get; set; }
    /// <summary>Gets or sets a fifth download URL for the image pack.</summary>
    public string ImagePackDownloadLink5 { get; set; }
    /// <summary>Gets or sets the extraction path relative to the image pack download.</summary>
    public string ImagePackDownloadExtractPath { get; set; }
}
