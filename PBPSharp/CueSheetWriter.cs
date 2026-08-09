using System.Text;
using PBPSharp.Models;

namespace PBPSharp;

/// <summary>
/// Generates CUE sheet content from disc TOC data.
/// </summary>
public static class CueSheetWriter
{
    private const string DataTrackType = "MODE2/2352";
    private const string AudioTrackType = "AUDIO";

    /// <summary>
    /// Generates a CUE sheet string for the given TOC entries.
    /// </summary>
    /// <param name="binFileName">The BIN file name (without path) referenced in the CUE sheet.</param>
    /// <param name="tocEntries">The disc's Table of Contents entries.</param>
    /// <returns>The complete CUE sheet content as a string.</returns>
    public static string GenerateCueSheet(string binFileName, IReadOnlyList<TocEntry> tocEntries)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"FILE \"{binFileName}\" BINARY");

        foreach (var track in tocEntries)
        {
            var dataType = track.TrackType == TrackType.Audio ? AudioTrackType : DataTrackType;
            sb.AppendLine(null, $"  TRACK {track.TrackNo:00} {dataType}");

            if (track.TrackType == TrackType.Audio)
            {
                var index0 = SubtractLeadin(track.Minutes, track.Seconds, track.Frames, 150);
                sb.AppendLine($"    INDEX 00 {FormatMsf(index0.Minutes, index0.Seconds, index0.Frames)}");
            }

            sb.AppendLine($"    INDEX 01 {FormatMsf(track.Minutes, track.Seconds, track.Frames)}");
        }

        return sb.ToString();
    }

    private static string FormatMsf(int minutes, int seconds, int frames)
    {
        return $"{minutes:00}:{seconds:00}:{frames:00}";
    }

    private static (int Minutes, int Seconds, int Frames) SubtractLeadin(int minutes, int seconds, int frames, int leadinFrames)
    {
        var totalFrames = (minutes * 60 * 75) + (seconds * 75) + frames - leadinFrames;
        if (totalFrames < 0)
        {
            totalFrames = 0;
        }

        var m = totalFrames / (60 * 75);
        var remainder = totalFrames % (60 * 75);
        var s = remainder / 75;
        var f = remainder % 75;

        return (m, s, f);
    }
}
