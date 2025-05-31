using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher.Models;

public class SearchResult : INotifyPropertyChanged
{
    private long _fileSizeBytes = -1; // Backing field, initialized to -1 (placeholder for "Calculating...")

    public string FileName { get; init; }
    public string FileNameWithExtension { get; init; }

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

    public string MachineName { get; init; }
    public string FolderName { get; init; }
    public string FilePath { get; init; }
    public string SystemName { get; init; }
    public SystemManager.Emulator EmulatorConfig { get; init; }

    private int _score;

    public int Score // If the Score can be updated after display and is bound, it should also notify.
    {
        get => _score;
        set
        {
            if (_score == value) return;

            _score = value;
            OnPropertyChanged();
        }
    }

    public string CoverImage { get; init; }
    public string DefaultEmulator => EmulatorConfig?.EmulatorName ?? "No Default Emulator";

    public string FormattedFileSize =>
        _fileSizeBytes == -1 ? "Calculating..." : // Show "Calculating..." if size is -1
        _fileSizeBytes < -1 ? "N/A" : // Show "N/A" for other negative values (errors/not found)
        FormatFileSize.FormatToMb(_fileSizeBytes); // Otherwise, format the size

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}