namespace SimpleLauncher.Interfaces;

public interface IFormatFileSizeService
{
    string FormatToMb(long bytes);
    string FormatToHumanReadable(long bytes);
}
