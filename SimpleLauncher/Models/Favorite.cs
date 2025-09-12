#nullable enable
using MessagePack;
using SimpleLauncher.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace SimpleLauncher.Models;

// MessagePack format
[MessagePackObject]
public class Favorite : INotifyPropertyChanged
{
    private long _fileSizeBytes = -1; // Backing field, initialized to -1 (e.g., "Calculating...")
    private string? _defaultEmulator; // Backing field for DefaultEmulator

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
        get => _defaultEmulator;
        set
        {
            if (_defaultEmulator == value) return;

            _defaultEmulator = value;
            OnPropertyChanged();
        }
    }

    [IgnoreMember]
    public long FileSizeBytes
    {
        get => _fileSizeBytes;
        set
        {
            if (_fileSizeBytes == value) return;

            _fileSizeBytes = value;
            OnPropertyChanged(); // Notify for FileSizeBytes itself (if bound directly)
            OnPropertyChanged(nameof(FormattedFileSize)); // Notify for the derived FormattedFileSize
        }
    }

    // Add property to format file size using the helper (ignored for serialization)
    [IgnoreMember]
    public string FormattedFileSize =>
        _fileSizeBytes == -1 ? (string)Application.Current.TryFindResource("Calculating") ?? "Calculating..." : // Show "Calculating..." if size is -1
        _fileSizeBytes < -1 ? (string)Application.Current.TryFindResource("NotAvailable") ?? "N/A" : // Show "N/A" for other negative values (errors/not found)
        FormatFileSize.FormatToMb(_fileSizeBytes); // Otherwise, format the size

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}