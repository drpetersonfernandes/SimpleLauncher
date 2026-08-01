namespace SimpleLauncher.Interfaces;

/// <summary>
/// Defines the configuration properties for an emulator, including its location, parameters, and image pack download links.
/// </summary>
public interface IEmulator
{
    /// <summary>
    /// Gets the display name of the emulator.
    /// </summary>
    string EmulatorName { get; }

    /// <summary>
    /// Gets the file path to the emulator executable.
    /// </summary>
    string EmulatorLocation { get; }

    /// <summary>
    /// Gets the command-line parameters passed to the emulator.
    /// </summary>
    string EmulatorParameters { get; }

    /// <summary>
    /// Gets a value indicating whether the user should receive a notification when the emulator encounters an error.
    /// </summary>
    bool ReceiveANotificationOnEmulatorError { get; }

    /// <summary>
    /// Gets the primary download link for the emulator's image pack.
    /// </summary>
    string ImagePackDownloadLink { get; }

    /// <summary>
    /// Gets the second download link for the emulator's image pack.
    /// </summary>
    string ImagePackDownloadLink2 { get; }

    /// <summary>
    /// Gets the third download link for the emulator's image pack.
    /// </summary>
    string ImagePackDownloadLink3 { get; }

    /// <summary>
    /// Gets the fourth download link for the emulator's image pack.
    /// </summary>
    string ImagePackDownloadLink4 { get; }

    /// <summary>
    /// Gets the fifth download link for the emulator's image pack.
    /// </summary>
    string ImagePackDownloadLink5 { get; }

    /// <summary>
    /// Gets the path where the downloaded image pack should be extracted.
    /// </summary>
    string ImagePackDownloadExtractPath { get; }
}
