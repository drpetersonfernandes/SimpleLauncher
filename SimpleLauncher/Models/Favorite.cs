#nullable enable
using MessagePack;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher.Models;

[MessagePackObject]
public class Favorite : INotifyPropertyChanged
{
    [Key(0)]
    public required string FileName { get; init; }

    [Key(1)]
    public required string SystemName { get; init; }

    [IgnoreMember]
    public string? MachineDescription { get; init; }

    [IgnoreMember]
    public string? CoverImage { get; init; }

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