#nullable enable
using System.ComponentModel;

namespace SimpleLauncher.Services.GameItemFactory;

public class GameButtonViewModel : INotifyPropertyChanged
{
    public bool IsFavorite
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    public bool HasAchievements
    {
        get;
        set
        {
            if (field == value) return;

            field = value;
            OnPropertyChanged(nameof(HasAchievements));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}