using System.ComponentModel;
using System.Runtime.CompilerServices;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.DownloadService.Models;

public class ImagePackDownloadItem : INotifyPropertyChanged
{
    public string DisplayName { get; set; }
    public string DownloadUrl { get; set; }
    public string ExtractPath { get; set; }

    public DownloadButtonState State
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsIdle));
            OnPropertyChanged(nameof(IsDownloading));
            OnPropertyChanged(nameof(IsDownloaded));
            OnPropertyChanged(nameof(IsFailed));
            OnPropertyChanged(nameof(CanStartDownload));
        }
    }

    // Convenience properties for XAML binding
    public bool IsIdle => State == DownloadButtonState.Idle;
    public bool IsDownloading => State == DownloadButtonState.Downloading;
    public bool IsDownloaded => State == DownloadButtonState.Downloaded;
    public bool IsFailed => State == DownloadButtonState.Failed;
    public bool CanStartDownload => State is DownloadButtonState.Idle or DownloadButtonState.Failed;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}