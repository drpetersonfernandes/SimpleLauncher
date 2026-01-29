using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleLauncher.SharedModels;

public class GameListViewItem : INotifyPropertyChanged
{
    public string FilePath { get; init; }
    public string FolderPath { get; init; }
    public System.Windows.Controls.ContextMenu ContextMenu { get; set; }

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