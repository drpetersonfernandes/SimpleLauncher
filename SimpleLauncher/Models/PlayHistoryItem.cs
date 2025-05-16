using System;
using MessagePack;
using SimpleLauncher.Services;

namespace SimpleLauncher.Models;

[MessagePackObject]
public class PlayHistoryItem
{
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

    [IgnoreMember]
    public long FileSizeBytes { get; set; }

    [IgnoreMember]
    public string FormattedFileSize =>
        // Use the FormatFileSize helper
        FormatFileSize.Format(FileSizeBytes);

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
}