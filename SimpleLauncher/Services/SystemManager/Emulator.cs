using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.SystemManager;

/// <summary>
/// Represents an emulator configuration with its executable path, parameters, and image pack download links.
/// </summary>
public class Emulator : IEmulator
{
    /// <summary>Gets the display name of the emulator.</summary>
    public string EmulatorName { get; init; }

    /// <summary>Gets the file path to the emulator executable.</summary>
    public string EmulatorLocation { get; init; }

    /// <summary>Gets the command-line parameters passed to the emulator.</summary>
    public string EmulatorParameters { get; init; }

    /// <summary>Gets whether to show a notification when the emulator encounters an error.</summary>
    public bool ReceiveANotificationOnEmulatorError { get; init; }

    /// <summary>Gets the primary image pack download URL.</summary>
    public string ImagePackDownloadLink { get; init; }

    /// <summary>Gets the secondary image pack download URL.</summary>
    public string ImagePackDownloadLink2 { get; init; }

    /// <summary>Gets the third image pack download URL.</summary>
    public string ImagePackDownloadLink3 { get; init; }

    /// <summary>Gets the fourth image pack download URL.</summary>
    public string ImagePackDownloadLink4 { get; init; }

    /// <summary>Gets the fifth image pack download URL.</summary>
    public string ImagePackDownloadLink5 { get; init; }

    /// <summary>Gets the path where downloaded image packs are extracted.</summary>
    public string ImagePackDownloadExtractPath { get; init; }
}