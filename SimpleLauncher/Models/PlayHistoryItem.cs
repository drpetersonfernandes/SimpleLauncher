using System;
using MessagePack;
using SimpleLauncher.Services;
using System.ComponentModel; // Required for INotifyPropertyChanged
using System.Runtime.CompilerServices;
using System.Windows; // Required for CallerMemberName

namespace SimpleLauncher.Models;

[MessagePackObject]
public class PlayHistoryItem : INotifyPropertyChanged
{
    private long _internalFileSizeBytes = -1; // Backing field, initialized to -1 ("Calculating...")

    [Key(0)]
    public string FileName { get; set; }

    [Key(1)]
    public string SystemName { get; set; }

    [Key(2)]
    public long TotalPlayTime { get; set; } // In seconds

    [Key(3)]
    public int TimesPlayed { get; set; }

    [Key(4)]
    public string LastPlayDate { get; set; }

    [Key(5)]
    public string LastPlayTime { get; set; }

    [IgnoreMember]
    public string MachineDescription { get; set; }

    [IgnoreMember]
    public string CoverImage { get; set; }

    [IgnoreMember]
    public string DefaultEmulator { get; set; }

    // This property will be set by the background task
    // It updates the backing field and notifies that FormattedFileSize has changed
    [IgnoreMember]
    public long FileSizeBytes
    {
        get => _internalFileSizeBytes;
        set
        {
            if (_internalFileSizeBytes == value) return;

            _internalFileSizeBytes = value;
            OnPropertyChanged(); // If FileSizeBytes itself were bound
            OnPropertyChanged(nameof(FormattedFileSize)); // Notify that the formatted FileSize string has changed
        }
    }

    // This is the property bound in the DataGrid
    [IgnoreMember]
    public string FormattedFileSize =>
        _internalFileSizeBytes == -1 ? (string)Application.Current.TryFindResource("Calculating") ?? "Calculating..." : // Show "Calculating..." if size is -1
        _internalFileSizeBytes < -1 ? (string)Application.Current.TryFindResource("NotAvailable") ?? "Not Available" : // Show "N/A" for other negative values (errors/not found)
        FormatFileSize.FormatToMb(_internalFileSizeBytes); // Otherwise, format the size

    [IgnoreMember]
    public string FormattedPlayTime
    {
        get
        {
            var timeSpan = TimeSpan.FromSeconds(TotalPlayTime);
            return timeSpan.TotalHours >= 1
                ? $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
                : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}