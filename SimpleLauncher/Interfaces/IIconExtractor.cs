namespace SimpleLauncher.Interfaces;

/// <summary>
/// Provides methods to extract icons from executable files.
/// </summary>
public interface IIconExtractor
{
    /// <summary>
    /// Extracts the icon from an executable file and saves it to the specified path.
    /// </summary>
    /// <param name="exePath">The path to the executable file.</param>
    /// <param name="savePath">The path where the extracted icon will be saved.</param>
    /// <param name="logErrors">The logger for recording errors.</param>
    void SaveIconFromExe(string exePath, string savePath, ILogger logErrors);
}
