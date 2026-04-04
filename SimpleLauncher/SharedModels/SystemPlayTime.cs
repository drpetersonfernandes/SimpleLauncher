using System;
using System.Globalization;
using MessagePack;

namespace SimpleLauncher.SharedModels;

[MessagePackObject]
public class SystemPlayTime
{
    [Key(0)]
    public string SystemName { get; init; }

    [Key(1)]
    public long PlayTimeSeconds { get; set; }

    [IgnoreMember]
    public string FormattedPlayTime =>
        TimeSpan.FromSeconds(PlayTimeSeconds).ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
}
