using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher.Models;

public class ImagePackDownloadItem : INotifyPropertyChanged
{
    private bool _isDownloaded;

    public string DisplayName { get; set; }
    public string DownloadUrl { get; set; }
    public string ExtractPath { get; set; }

    public bool IsDownloaded
    {
        get => _isDownloaded;
        set
        {
            if (_isDownloaded == value) return;

            _isDownloaded = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}