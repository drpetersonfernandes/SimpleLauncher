using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using SimpleLauncher.Services;

namespace SimpleLauncher.Models;

public class GameListViewItem : INotifyPropertyChanged
{
    public string FilePath { get; init; }
    public string FolderPath { get; init; }
    public System.Windows.Controls.ContextMenu ContextMenu { get; set; }

    // Backing field for the actual size in bytes, initialized to -1 ("Calculating...")
    private long _internalFileSizeBytes = -1;

    public bool IsFavorite
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string FileName
    {
        get;
        init // FileName is set once at creation and doesn't change
        ;
    }

    public string MachineDescription
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public string TimesPlayed
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = "0";

    public string PlayTime
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    } = "0m 0s";

    // Not directly bound in XAML, but triggers update for the formatted FileSize
    public long FileSizeBytes
    {
        get => _internalFileSizeBytes;
        set
        {
            if (_internalFileSizeBytes == value) return;

            _internalFileSizeBytes = value;
            OnPropertyChanged(nameof(FileSize)); // Notify that the formatted FileSize string has changed
        }
    }

    // This is the property bound in the DataGrid (XAML uses "FileSize")
    public string FileSize =>
        _internalFileSizeBytes == -1 ? (string)Application.Current.TryFindResource("Calculating") ?? "Calculating..." : // Show "Calculating..." if size is -1
        _internalFileSizeBytes < -1 ? (string)Application.Current.TryFindResource("NotAvailable") ?? "Not Available" : // Show "N/A" for other negative values (errors/not found)
        FormatFileSize.FormatToMb(_internalFileSizeBytes);

    public bool HasAchievements
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int AchievementsEarned
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged();
        }
    }

    public int AchievementsTotal
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