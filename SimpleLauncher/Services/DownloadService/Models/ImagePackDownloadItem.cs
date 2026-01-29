using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher.Services.DownloadService.Models;

public class ImagePackDownloadItem : INotifyPropertyChanged
{
    public string DisplayName { get; set; }
    public string DownloadUrl { get; set; }
    public string ExtractPath { get; set; }

    public bool IsDownloaded
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}