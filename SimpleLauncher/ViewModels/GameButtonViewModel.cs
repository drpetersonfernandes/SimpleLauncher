#nullable enable
using System.ComponentModel;

namespace SimpleLauncher.ViewModels;

public class GameButtonViewModel : INotifyPropertyChanged
{
    private bool _isFavorite;
    private bool _hasAchievements;

    public bool IsFavorite
    {
        get => _isFavorite;
        set
        {
            if (_isFavorite == value) return;

            _isFavorite = value;
            OnPropertyChanged(nameof(IsFavorite));
        }
    }

    public bool HasAchievements
    {
        get => _hasAchievements;
        set
        {
            if (_hasAchievements == value) return;

            _hasAchievements = value;
            OnPropertyChanged(nameof(HasAchievements));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}