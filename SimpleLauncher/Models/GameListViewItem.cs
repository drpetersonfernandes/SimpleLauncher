using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimpleLauncher.Services;

namespace SimpleLauncher.Models;

public class GameListViewItem : INotifyPropertyChanged
{
    private string _machineDescription;
    private string _timesPlayed = "0";

    private string _playTime = "0m 0s";
    // FileSize string property will now be a getter based on _internalFileSizeBytes

    public string FilePath { get; init; }
    public System.Windows.Controls.ContextMenu ContextMenu { get; set; }
    private bool _isFavorite;

    // Backing field for the actual size in bytes, initialized to -1 ("Calculating...")
    private long _internalFileSizeBytes = -1;

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value) return;

            _isFavorite = value;
            OnPropertyChanged();
        }
    }

    public string FileName
    {
        get;
        init // Assuming FileName is set once at creation and doesn't change
        ;
    }

    public string MachineDescription
    {
        get => _machineDescription;
        set
        {
            if (_machineDescription == value) return;

            _machineDescription = value;
            OnPropertyChanged();
        }
    }

    public string TimesPlayed
    {
        get => _timesPlayed;
        set
        {
            if (_timesPlayed == value) return;

            _timesPlayed = value;
            OnPropertyChanged();
        }
    }

    public string PlayTime
    {
        get => _playTime;
        set
        {
            if (_playTime == value) return;

            _playTime = value;
            OnPropertyChanged();
        }
    }

    // This property will be set by the background task
    // Not directly bound in XAML, but triggers update for the formatted FileSize
    public long FileSizeBytes
    {
        get => _internalFileSizeBytes;
        set
        {
            if (_internalFileSizeBytes == value) return;

            _internalFileSizeBytes = value;
            // OnPropertyChanged(); // If FileSizeBytes itself were bound
            OnPropertyChanged(nameof(FileSize)); // Notify that the formatted FileSize string has changed
        }
    }

    // This is the property bound in the DataGrid (XAML uses "FileSize")
    public string FileSize =>
        _internalFileSizeBytes == -1 ? "Calculating..." :
        _internalFileSizeBytes < -1 ? "N/A" : // For errors or file not found
        FormatFileSize.Format(_internalFileSizeBytes);

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}