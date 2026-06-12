#nullable enable

using System.ComponentModel;

namespace SimpleLauncher.Services.GameItemFactory;

/// <summary>
/// View model for game buttons in the grid view, exposing favorite and achievement state
/// with property change notification.
/// </summary>
public class GameButtonViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets whether this game is marked as a favorite.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether this game has RetroAchievements associated with it.
    /// </summary>
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

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
