using System.ComponentModel;

namespace SimpleLauncher.Models;

public class GameListViewItem : INotifyPropertyChanged
{
    private readonly string _fileName;
    private string _machineDescription;
    private string _timesPlayed = "0";
    private string _playTime = "0m 0s";
    private string _fileSize;
    public string FilePath { get; init; }
    public System.Windows.Controls.ContextMenu ContextMenu { get; set; }
    private bool _isFavorite;

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            _isFavorite = value;
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    public string FileName
    {
        get => _fileName;
        init
        {
            _fileName = value;
            OnPropertyChanged(nameof(FileName));
        }
    }

    public string MachineDescription
    {
        get => _machineDescription;
        set
        {
            _machineDescription = value;
            OnPropertyChanged(nameof(MachineDescription));
        }
    }

    public string TimesPlayed
    {
        get => _timesPlayed;
        set
        {
            _timesPlayed = value;
            OnPropertyChanged(nameof(TimesPlayed));
        }
    }

    public string PlayTime
    {
        get => _playTime;
        set
        {
            _playTime = value;
            OnPropertyChanged(nameof(PlayTime));
        }
    }

    public string FileSize
    {
        get => _fileSize;
        set
        {
            _fileSize = value;
            OnPropertyChanged(nameof(FileSize));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
