namespace SimpleLauncher.Core.Models;

/// <summary>
///     Provides data for the event that occurs after a game has finished playing.
/// </summary>
public sealed class GamePlayedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GamePlayedEventArgs" /> class.
    /// </summary>
    /// <param name="fileName">The file name of the played game.</param>
    /// <param name="systemName">The system name the game belongs to.</param>
    public GamePlayedEventArgs(string fileName, string systemName)
    {
        FileName = fileName;
        SystemName = systemName;
    }

    /// <summary>Gets the file name of the played game.</summary>
    public string FileName { get; }

    /// <summary>Gets the system name the game belongs to.</summary>
    public string SystemName { get; }
}