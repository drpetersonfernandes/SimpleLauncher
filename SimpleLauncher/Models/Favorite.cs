#nullable enable
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a user's favorite game entry, persisted via MessagePack.
/// </summary>
[MessagePackObject]
public class Favorite : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the full file path of the favorite game.
    /// </summary>
    [Key(0)]
    public required string FileName { get; init; }

    /// <summary>
    /// Gets the name of the system this game belongs to.
    /// </summary>
    [Key(1)]
    public required string SystemName { get; init; }

    /// <summary>
    /// Gets the machine description from the ROM database.
    /// </summary>
    [IgnoreMember]
    public string? MachineDescription { get; init; }

    /// <summary>
    /// Gets the path to the cover image for this game.
    /// </summary>
    [IgnoreMember]
    public string? CoverImage { get; init; }

    /// <summary>
    /// Gets or sets the default emulator name for this favorite.
    /// </summary>
    [IgnoreMember]
    public string? DefaultEmulator
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}